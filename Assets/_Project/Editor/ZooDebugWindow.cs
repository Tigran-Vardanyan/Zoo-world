using System.Linq;
using UnityEditor;
using UnityEngine;
using VContainer;
using ZooWorld.Animals;
using ZooWorld.Installers;
using ZooWorld.Spawning;
using ZooWorld.Statistics;
using ZooWorld.Utils;
using ZooWorld.World;

namespace ZooWorld.Editor
{
    /// <summary>
    /// Play mode tool for testing the zoo by hand: spawn specific animals, force encounters,
    /// rebuild obstacles, kill animals and watch live checks. Talks to the game only through its services.
    /// </summary>
    public sealed class ZooDebugWindow : EditorWindow
    {
        private const float RepaintInterval = 0.25f;
        private const float SpawnClearance = 1.2f;
        private const float EncounterGap = 2f;
        private const float EncounterPush = 4f;

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
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSpawning(game);
            DrawEncounter(game);
            DrawWorld(game);
            DrawChecks(game);
            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(_message))
            {
                EditorGUILayout.HelpBox(_message, MessageType.None);
            }
        }

        private void DrawSpawning(Game game)
        {
            Header("Spawning");
            game.Spawner.IsPaused = EditorGUILayout.Toggle("Pause auto spawning", game.Spawner.IsPaused);
            Time.timeScale = EditorGUILayout.Slider("Time scale", Time.timeScale, 0f, 5f);

            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var entry in game.Catalog.Entries.Where(entry => entry.Prefab != null))
                {
                    if (GUILayout.Button($"Spawn {entry.Prefab.Config.DisplayName}"))
                    {
                        SpawnAtFreeSpot(game, entry.Prefab);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _batchSize = EditorGUILayout.IntSlider(_batchSize, 1, 30);
                if (GUILayout.Button("Spawn random batch", GUILayout.Width(140f)))
                {
                    var spawned = 0;
                    for (var i = 0; i < _batchSize; i++)
                    {
                        spawned += SpawnAtFreeSpot(game, game.Catalog.PickRandom()) ? 1 : 0;
                    }

                    _message = $"Spawned {spawned} of {_batchSize} (others found no free spot).";
                }
            }
        }

        private void DrawEncounter(Game game)
        {
            Header("Force an encounter");
            var prefabs = game.Catalog.Entries.Select(entry => entry.Prefab).Where(prefab => prefab != null).ToArray();
            if (prefabs.Length == 0)
            {
                EditorGUILayout.HelpBox("The animal catalog is empty.", MessageType.Warning);
                return;
            }

            var names = prefabs.Select(prefab => prefab.Config.DisplayName).ToArray();
            using (new EditorGUILayout.HorizontalScope())
            {
                _firstSpecies = Mathf.Clamp(EditorGUILayout.Popup(_firstSpecies, names), 0, names.Length - 1);
                GUILayout.Label("vs", GUILayout.Width(18f));
                _secondSpecies = Mathf.Clamp(EditorGUILayout.Popup(_secondSpecies, names), 0, names.Length - 1);
            }

            EditorGUILayout.LabelField(DescribeExpectedOutcome(prefabs[_firstSpecies].Config, prefabs[_secondSpecies].Config), EditorStyles.miniLabel);
            if (GUILayout.Button("Spawn both and push them together"))
            {
                ForceEncounter(game, prefabs[_firstSpecies], prefabs[_secondSpecies]);
            }
        }

        private void DrawWorld(Game game)
        {
            Header("World");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild obstacles"))
                {
                    game.Obstacles.Rebuild();
                    _message = $"Placed {game.Obstacles.Placed.Count} obstacles.";
                }

                if (GUILayout.Button("Kill random animal"))
                {
                    var alive = game.Registry.Alive.ToArray();
                    if (alive.Length > 0)
                    {
                        var victim = alive[Random.Range(0, alive.Length)];
                        _message = $"Killed a {victim.Config.DisplayName}.";
                        victim.Die();
                    }
                }

                if (GUILayout.Button("Kill all"))
                {
                    var alive = game.Registry.Alive.ToArray();
                    foreach (var animal in alive)
                    {
                        animal.Die();
                    }

                    _message = $"Killed {alive.Length} animals.";
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
            EditorGUILayout.LabelField("Obstacles", game.Obstacles.Placed.Count.ToString());

            var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = outside == 0 ? new Color(0.3f, 0.8f, 0.3f) : Color.red } };
            EditorGUILayout.LabelField("Animals partly off screen", outside.ToString(), style);
        }

        private bool SpawnAtFreeSpot(Game game, Animal prefab)
        {
            if (prefab == null || !game.FreeSpots.TryFind(SpawnClearance, out var point))
            {
                _message = "No free spot found.";
                return false;
            }

            game.Spawner.Spawn(prefab, point, GroundDirection.Random());
            _message = $"Spawned a {prefab.Config.DisplayName}.";
            return true;
        }

        private void ForceEncounter(Game game, Animal first, Animal second)
        {
            if (!game.FreeSpots.TryFind(EncounterGap + SpawnClearance, out var center))
            {
                _message = "No free spot big enough for an encounter.";
                return;
            }

            var axis = GroundDirection.Random();
            var a = game.Spawner.Spawn(first, center - axis * (EncounterGap * 0.5f), axis);
            var b = game.Spawner.Spawn(second, center + axis * (EncounterGap * 0.5f), -axis);
            a.Push(axis * EncounterPush);
            b.Push(-axis * EncounterPush);

            Selection.objects = new Object[] { a.gameObject, b.gameObject };
            _message = $"{first.Config.DisplayName} and {second.Config.DisplayName} pushed together (selected in the Hierarchy).";
        }

        private static string DescribeExpectedOutcome(AnimalConfig first, AnimalConfig second)
        {
            var firstEats = first.CanEat(second);
            var secondEats = second.CanEat(first);
            if (firstEats && secondEats)
            {
                return "Expected: duel, one random survivor says \"Tasty!\".";
            }

            if (firstEats || secondEats)
            {
                var predator = firstEats ? first : second;
                var prey = firstEats ? second : first;
                return $"Expected: the {predator.DisplayName} eats the {prey.DisplayName}.";
            }

            return "Expected: they bounce apart, both survive.";
        }

        private static bool IsFullyOnScreen(Camera camera, Animal animal)
        {
            var bounds = new Bounds(animal.transform.position, Vector3.zero);
            foreach (var renderer in animal.GetComponentsInChildren<MeshRenderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            var min = camera.WorldToViewportPoint(bounds.min);
            var max = camera.WorldToViewportPoint(bounds.max);
            return min.x >= 0f && min.y >= 0f && max.x <= 1f && max.y <= 1f;
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
                Catalog = container.Resolve<AnimalCatalog>();
                Spawner = container.Resolve<IAnimalSpawner>();
                Registry = container.Resolve<IAnimalRegistry>();
                FreeSpots = container.Resolve<IFreeSpotFinder>();
                Statistics = container.Resolve<IDeathStatistics>();
                Obstacles = container.Resolve<ObstacleSpawner>();
                Camera = container.Resolve<Camera>();
            }

            public AnimalCatalog Catalog { get; }
            public IAnimalSpawner Spawner { get; }
            public IAnimalRegistry Registry { get; }
            public IFreeSpotFinder FreeSpots { get; }
            public IDeathStatistics Statistics { get; }
            public ObstacleSpawner Obstacles { get; }
            public Camera Camera { get; }
        }
    }
}
