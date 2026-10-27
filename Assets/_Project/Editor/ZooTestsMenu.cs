using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ZooWorld.Editor
{
    /// <summary>Runs the project's automated tests from the Tools menu and reports the result in the Console.</summary>
    public static class ZooTestsMenu
    {
        private const string TestAssembly = "ZooWorld.Tests.EditMode";

        [MenuItem("Tools/Zoo World/Run All Tests", priority = 20)]
        public static void RunAllTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new ResultLogger(api));
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { TestAssembly },
            }));
        }

        [MenuItem("Tools/Zoo World/Open Test Runner", priority = 21)]
        public static void OpenTestRunner() => EditorApplication.ExecuteMenuItem("Window/General/Test Runner");

        private sealed class ResultLogger : ICallbacks
        {
            private readonly TestRunnerApi _api;

            public ResultLogger(TestRunnerApi api) => _api = api;

            public void RunStarted(ITestAdaptor testsToRun) => Debug.Log($"[Zoo World] Running {testsToRun.TestCaseCount} tests...");

            public void RunFinished(ITestResultAdaptor result)
            {
                var summary = $"[Zoo World] Tests finished: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped.";
                if (result.FailCount > 0)
                {
                    Debug.LogError(summary + " Open Tools > Zoo World > Open Test Runner for details.");
                }
                else
                {
                    Debug.Log(summary);
                }

                _api.UnregisterCallbacks(this);
                Object.DestroyImmediate(_api);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
                {
                    Debug.LogError($"[Zoo World] FAILED {result.FullName}\n{result.Message}");
                }
            }
        }
    }
}
