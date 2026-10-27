using System.Linq;
using UnityEditor;
using UnityEngine;
using VContainer;
using ZooWorld.Animals;
using ZooWorld.Controls;
using ZooWorld.Installers;
using ZooWorld.Statistics;

namespace ZooWorld.Editor
{
    /// <summary>
    /// Play mode tool for testing the zoo by hand: spawn specific animals, force encounters,
    /// rebuild obstacles, kill animals and watch live checks. Uses the same <see cref="IZooCommands"/>
    /// as the in-game control panel, plus an exact off-screen check that is too costly for a build.
    /// </summary>
    public sealed class ZooDebugWindow : EditorWindow
    {
        private const float RepaintInterval = 0.25f;

        private static Mesh _bakedMesh;
        private static Mesh BakedMesh => _bakedMesh != null ? _bakedMesh : _bakedMesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };

        private int _firstSpecies;
        private int _secondSpecies = 1;
        private int _batchSize = 10;
        private double _nextRepaint;
        private string _message;
        private Vector2 _scroll;

        [MenuItem("Tools/Zoo World/Debug Window", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<ZooDebugWindow>();
            window.titleContent = new GUIContent("Zoo World");
            window.minSize = new Vector2(320f, 420f);
        }

        private void OnEnable() => EditorApplication.update += RepaintWhilePlaying;

        private void OnDisable() => EditorApplication.update -= RepaintWhilePlaying;

        private void RepaintWhilePlaying()
        {
            if (EditorApplication.isPlaying && EditorApplication.timeSinceStartup >= _nextRepaint)
            {
                _nextRepaint = EditorApplication.timeSinceStartup + RepaintInterval;
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("The tools work in Play mode.", MessageType.Info);
                if (GUILayout.Button("Enter Play mode", GUILayout.Height(28f)))
                {
                    EditorApplication.isPlaying = true;
                }

                return;
            }

            var scope = FindAnyObjectByType<GameLifetimeScope>();
            if (scope == null || scope.Container == null)
            {
                EditorGUILayout.HelpBox("No running GameLifetimeScope found. Open the Zoo scene.", MessageType.Warning);
                return;
            }

            var game = new Game(scope.Container);
            if (game.Commands.Species.Count == 0)
            {
                EditorGUILayout.HelpBox("The animal catalog is empty.", MessageType.Warning);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSpawning(game.Commands);
            DrawEncounter(game.Commands);
            DrawWorld(game.Commands);
            DrawChecks(game);
            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(_message))
            {
                EditorGUILayout.HelpBox(_message, MessageType.None);
            }
        }

        private void DrawSpawning(IZooCommands commands)
        {
            Header("Spawning");
            commands.IsSpawningPaused = EditorGUILayout.Toggle("Pause auto spawning", commands.IsSpawningPaused);
            commands.TimeScale = EditorGUILayout.Slider("Time scale", commands.TimeScale, 0f, 5f);

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var species in commands.Species)
                {
                    if (GUILayout.Button($"Spawn {species.Config.DisplayName}"))
                    {
                        _message = commands.Spawn(species) != null ? $"Spawned a {species.Config.DisplayName}." : "No free spot found.";
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _batchSize = EditorGUILayout.IntSlider(_batchSize, 1, 30);
                if (GUILayout.Button("Spawn random batch", GUILayout.Width(140f)))
                {
                    _message = $"Spawned {commands.SpawnRandom(_batchSize)} of {_batchSize} (others found no free spot).";
                }
            }
        }

        private void DrawEncounter(IZooCommands commands)
        {
            Header("Force an encounter");
            var names = commands.Species.Select(species => species.Config.DisplayName).ToArray();
            using (new EditorGUILayout.HorizontalScope())
            {
                _firstSpecies = Mathf.Clamp(EditorGUILayout.Popup(_firstSpecies, names), 0, names.Length - 1);
                GUILayout.Label("vs", GUILayout.Width(18f));
                _secondSpecies = Mathf.Clamp(EditorGUILayout.Popup(_secondSpecies, names), 0, names.Length - 1);
            }

            var first = commands.Species[_firstSpecies];
            var second = commands.Species[_secondSpecies];
            EditorGUILayout.LabelField("Expected: " + EncounterForecast.Describe(first.Config, second.Config), EditorStyles.miniLabel);
            if (GUILayout.Button("Spawn both and push them together"))
            {
                _message = commands.ForceEncounter(first, second)
                    ? $"{first.Config.DisplayName} and {second.Config.DisplayName} pushed together."
                    : "No free spot big enough for an encounter.";
            }
        }

        private void DrawWorld(IZooCommands commands)
        {
            Header("World");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild obstacles"))
                {
                    commands.RebuildObstacles();
                    _message = $"Placed {commands.ObstacleCount} obstacles.";
                }

                if (GUILayout.Button("Kill random animal"))
                {
                    var victim = commands.KillRandom();
                    _message = victim != null ? $"Killed a {victim.Config.DisplayName}." : "Nobody to kill.";
                }

                if (GUILayout.Button("Kill all"))
                {
                    _message = $"Killed {commands.KillAll()} animals.";
                }
            }
        }

        private static void DrawChecks(Game game)
        {
            Header("Live checks");
            var alive = game.Registry.Alive;
            var outside = alive.Count(animal => !IsFullyOnScreen(game.Camera, animal));

            EditorGUILayout.LabelField("Alive animals", alive.Count.ToString());
            foreach (var group in alive.GroupBy(animal => animal.Config.DisplayName))
            {
                EditorGUILayout.LabelField($"    {group.Key}", group.Count().ToString());
            }

            EditorGUILayout.LabelField("Dead prey / predators", $"{game.Statistics.DeadPrey} / {game.Statistics.DeadPredators}");
            EditorGUILayout.LabelField("Obstacles", game.Commands.ObstacleCount.ToString());

            var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = outside == 0 ? new Color(0.3f, 0.8f, 0.3f) : Color.red } };
            EditorGUILayout.LabelField("Animals partly off screen", outside.ToString(), style);
        }

        private static bool IsFullyOnScreen(Camera camera, Animal animal)
        {
            foreach (var renderer in animal.GetComponentsInChildren<Renderer>())
            {
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    // Skinned bounds are much larger than the posed mesh, so check the actual deformed vertices.
                    skinned.BakeMesh(BakedMesh, true);
                    var toWorld = skinned.transform.localToWorldMatrix;
                    foreach (var vertex in BakedMesh.vertices)
                    {
                        if (!IsInViewport(camera, toWorld.MultiplyPoint3x4(vertex)))
                        {
                            return false;
                        }
                    }
                }
                else if (renderer is MeshRenderer
                         && (!IsInViewport(camera, renderer.bounds.min) || !IsInViewport(camera, renderer.bounds.max)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsInViewport(Camera camera, Vector3 worldPoint)
        {
            var viewport = camera.WorldToViewportPoint(worldPoint);
            return viewport.x >= 0f && viewport.y >= 0f && viewport.x <= 1f && viewport.y <= 1f;
        }

        private static void Header(string title)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>The game services the window works with, resolved from the running scope.</summary>
        private readonly struct Game
        {
            public Game(IObjectResolver container)
            {
                Commands = container.Resolve<IZooCommands>();
                Registry = container.Resolve<IAnimalRegistry>();
                Statistics = container.Resolve<IDeathStatistics>();
                Camera = container.Resolve<Camera>();
            }

            public IZooCommands Commands { get; }
            public IAnimalRegistry Registry { get; }
            public IDeathStatistics Statistics { get; }
            public Camera Camera { get; }
        }
    }
}
