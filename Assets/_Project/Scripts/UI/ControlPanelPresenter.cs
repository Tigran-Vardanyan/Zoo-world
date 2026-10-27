using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VContainer.Unity;
using ZooWorld.Animals;
using ZooWorld.Cameras;
using ZooWorld.Controls;
using ZooWorld.Statistics;

namespace ZooWorld.UI
{
    /// <summary>
    /// Connects <see cref="ControlPanelView"/> to the game: turns clicks into <see cref="IZooCommands"/>
    /// and refreshes the live info a few times per second (in real time, so it also updates while paused).
    /// </summary>
    public sealed class ControlPanelPresenter : IStartable, ITickable, IDisposable
    {
        private const int BatchSize = 10;
        private const float InfoRefreshInterval = 0.25f;
        private const float ZoomStep = 1.5f;

        private readonly IZooCommands _commands;
        private readonly IAnimalRegistry _registry;
        private readonly IDeathStatistics _statistics;
        private readonly KillOnClickInput _killOnClick;
        private readonly ICameraSwitcher _cameras;
        private readonly ControlPanelView _view;
        private readonly StringBuilder _info = new();
        private readonly Dictionary<AnimalConfig, int> _aliveBySpecies = new();

        private bool _isCollapsed;
        private float _timeUntilRefresh;

        public ControlPanelPresenter(
            IZooCommands commands,
            IAnimalRegistry registry,
            IDeathStatistics statistics,
            KillOnClickInput killOnClick,
            ICameraSwitcher cameras,
            ControlPanelView view)
        {
            _commands = commands;
            _registry = registry;
            _statistics = statistics;
            _killOnClick = killOnClick;
            _cameras = cameras;
            _view = view;
        }

        public void Start()
        {
            _view.SetSpecies(_commands.Species.Select(species => species.Config.DisplayName).ToList());
            _view.SetCollapsed(_isCollapsed, animate: false);
            _view.SetPaused(_commands.IsSpawningPaused);
            _view.SetTimeScale(_commands.TimeScale);
            _view.SetKillOnClick(_killOnClick.IsActive);
            _view.SetCameraMode(_cameras.IsFree);
            _view.ShowStatus(string.Empty);
            RefreshForecast();
            RefreshInfo();

            _view.CollapseClicked += OnCollapseClicked;
            _view.SwipedDown += OnSwipedDown;
            _view.SwipedUp += OnSwipedUp;
            _view.CameraModeClicked += OnCameraModeClicked;
            _view.ZoomInClicked += OnZoomInClicked;
            _view.ZoomOutClicked += OnZoomOutClicked;
            _view.ResetViewClicked += OnResetViewClicked;
            _view.PauseClicked += OnPauseClicked;
            _view.SpawnSpeciesClicked += OnSpawnSpeciesClicked;
            _view.SpawnBatchClicked += OnSpawnBatchClicked;
            _view.EncounterSelectionChanged += RefreshForecast;
            _view.EncounterClicked += OnEncounterClicked;
            _view.RebuildObstaclesClicked += OnRebuildObstaclesClicked;
            _view.KillOnClickToggled += OnKillOnClickToggled;
            _view.KillAllClicked += OnKillAllClicked;
            _view.TimeScaleChanged += OnTimeScaleChanged;
            _killOnClick.Killed += OnAnimalClickedToDeath;
            _killOnClick.Missed += OnKillClickMissed;
        }

        public void Tick()
        {
            _timeUntilRefresh -= Time.unscaledDeltaTime;
            if (_timeUntilRefresh <= 0f)
            {
                _timeUntilRefresh = InfoRefreshInterval;
                RefreshInfo();
            }
        }

        public void Dispose()
        {
            _view.CollapseClicked -= OnCollapseClicked;
            _view.SwipedDown -= OnSwipedDown;
            _view.SwipedUp -= OnSwipedUp;
            _view.CameraModeClicked -= OnCameraModeClicked;
            _view.ZoomInClicked -= OnZoomInClicked;
            _view.ZoomOutClicked -= OnZoomOutClicked;
            _view.ResetViewClicked -= OnResetViewClicked;
            _view.PauseClicked -= OnPauseClicked;
            _view.SpawnSpeciesClicked -= OnSpawnSpeciesClicked;
            _view.SpawnBatchClicked -= OnSpawnBatchClicked;
            _view.EncounterSelectionChanged -= RefreshForecast;
            _view.EncounterClicked -= OnEncounterClicked;
            _view.RebuildObstaclesClicked -= OnRebuildObstaclesClicked;
            _view.KillOnClickToggled -= OnKillOnClickToggled;
            _view.KillAllClicked -= OnKillAllClicked;
            _view.TimeScaleChanged -= OnTimeScaleChanged;
            _killOnClick.Killed -= OnAnimalClickedToDeath;
            _killOnClick.Missed -= OnKillClickMissed;
        }

        private void OnCollapseClicked() => SetCollapsed(!_isCollapsed);

        private void OnSwipedDown() => SetCollapsed(true);

        private void OnSwipedUp() => SetCollapsed(false);

        private void SetCollapsed(bool isCollapsed)
        {
            _isCollapsed = isCollapsed;
            _view.SetCollapsed(isCollapsed);
        }

        private void OnCameraModeClicked()
        {
            _cameras.SetFree(!_cameras.IsFree);
            _view.SetCameraMode(_cameras.IsFree);
            _view.ShowStatus(_cameras.IsFree ? "Free camera: move and zoom around the zoo" : "Static camera: whole zoo in view");
        }

        private void OnZoomInClicked() => _cameras.Zoom(ZoomStep);

        private void OnZoomOutClicked() => _cameras.Zoom(-ZoomStep);

        private void OnResetViewClicked() => _cameras.ResetView();

        private void OnPauseClicked()
        {
            _commands.IsSpawningPaused = !_commands.IsSpawningPaused;
            _view.SetPaused(_commands.IsSpawningPaused);
            _view.ShowStatus(_commands.IsSpawningPaused ? "Automatic spawning paused" : "Automatic spawning resumed");
        }

        private void OnSpawnSpeciesClicked(int index)
        {
            var species = _commands.Species[index];
            _view.ShowStatus(_commands.Spawn(species) != null
                ? $"Spawned a {species.Config.DisplayName}"
                : "No free spot right now");
            RefreshInfo();
        }

        private void OnSpawnBatchClicked()
        {
            var spawned = _commands.SpawnRandom(BatchSize);
            _view.ShowStatus(spawned == BatchSize ? $"Spawned {spawned} animals" : $"Spawned {spawned} of {BatchSize} (no more free spots)");
            RefreshInfo();
        }

        private void OnEncounterClicked()
        {
            var first = _commands.Species[_view.FirstSpecies];
            var second = _commands.Species[_view.SecondSpecies];
            _view.ShowStatus(_commands.ForceEncounter(first, second)
                ? $"{first.Config.DisplayName} and {second.Config.DisplayName} pushed together"
                : "No free spot big enough for an encounter");
            RefreshInfo();
        }

        private void OnRebuildObstaclesClicked()
        {
            _commands.RebuildObstacles();
            _view.ShowStatus($"New layout: {_commands.ObstacleCount} obstacles");
            RefreshInfo();
        }

        private void OnKillOnClickToggled()
        {
            _killOnClick.IsActive = !_killOnClick.IsActive;
            _view.SetKillOnClick(_killOnClick.IsActive);
            _view.ShowStatus(_killOnClick.IsActive ? "Click or tap an animal to kill it" : "Kill on click is off");
        }

        private void OnAnimalClickedToDeath(Animal victim)
        {
            _view.ShowStatus($"A {victim.Config.DisplayName} died");
            RefreshInfo();
        }

        private void OnKillClickMissed() => _view.ShowStatus("No animal there - click right on one");

        private void OnKillAllClicked()
        {
            _view.ShowStatus($"{_commands.KillAll()} animals died");
            RefreshInfo();
        }

        private void OnTimeScaleChanged(float value)
        {
            _commands.TimeScale = Mathf.Round(value * 10f) / 10f;
            _view.SetTimeScale(_commands.TimeScale);
        }

        private void RefreshForecast()
        {
            var first = _commands.Species[_view.FirstSpecies].Config;
            var second = _commands.Species[_view.SecondSpecies].Config;
            _view.SetForecast(EncounterForecast.Describe(first, second));
        }

        private void RefreshInfo()
        {
            _aliveBySpecies.Clear();
            foreach (var animal in _registry.Alive)
            {
                _aliveBySpecies.TryGetValue(animal.Config, out var count);
                _aliveBySpecies[animal.Config] = count + 1;
            }

            _info.Clear();
            _info.Append("Alive: ").Append(_registry.AliveCount);
            foreach (var species in _commands.Species)
            {
                _aliveBySpecies.TryGetValue(species.Config, out var count);
                _info.Append("   ").Append(species.Config.DisplayName).Append(' ').Append(count);
            }

            _info.Append('\n').Append("Dead prey: ").Append(_statistics.DeadPrey)
                .Append("   Dead predators: ").Append(_statistics.DeadPredators);
            _info.Append('\n').Append("Obstacles: ").Append(_commands.ObstacleCount)
                .Append("   Spawning: ").Append(_commands.IsSpawningPaused ? "paused" : "on");

            _view.SetInfo(_info.ToString());
        }
    }
}
