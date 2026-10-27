using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZooWorld.UI
{
    /// <summary>
    /// Passive view of the in-game control panel at the bottom of the screen. It only exposes
    /// what the player did and setters for what to show; <see cref="ControlPanelPresenter"/> does the rest.
    /// The panel slides down so only its header stays visible, and back up (arrow button or swipe).
    /// </summary>
    public sealed class ControlPanelView : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Button _collapseButton;
        [SerializeField] private RectTransform _collapseArrow;
        [SerializeField] private PanelSwipe _swipe;
        [SerializeField, Min(0.01f)] private float _slideDuration = 0.25f;

        [Header("Spawning")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TMP_Text _pauseLabel;
        [SerializeField] private Transform _speciesButtonsRoot;
        [SerializeField] private SpeciesButton _speciesButtonPrefab;
        [SerializeField] private Button _spawnBatchButton;

        [Header("Encounter")]
        [SerializeField] private TMP_Dropdown _firstSpecies;
        [SerializeField] private TMP_Dropdown _secondSpecies;
        [SerializeField] private TMP_Text _forecast;
        [SerializeField] private Button _encounterButton;

        [Header("World")]
        [SerializeField] private Button _rebuildObstaclesButton;
        [SerializeField] private Button _killOnClickButton;
        [SerializeField] private TMP_Text _killOnClickLabel;
        [SerializeField] private Image _killOnClickBackground;
        [SerializeField] private Color _killOnClickOffColor = new(0.33f, 0.17f, 0.16f);
        [SerializeField] private Color _killOnClickOnColor = new(0.85f, 0.2f, 0.17f);
        [SerializeField] private Button _killAllButton;

        [Header("Camera")]
        [SerializeField] private Button _cameraModeButton;
        [SerializeField] private TMP_Text _cameraModeLabel;
        [SerializeField] private Button _zoomInButton;
        [SerializeField] private Button _zoomOutButton;
        [SerializeField] private Button _resetViewButton;
        [SerializeField] private TMP_Text _cameraHint;

        [Header("Simulation")]
        [SerializeField] private Slider _timeScale;
        [SerializeField] private TMP_Text _timeScaleLabel;
        [SerializeField] private TMP_Text _info;
        [SerializeField] private TMP_Text _status;

        private readonly List<SpeciesButton> _speciesButtons = new();
        private float _slide;       // 0 = fully shown, 1 = only the header is visible
        private float _slideTarget;

        public event Action CollapseClicked;
        public event Action SwipedUp;
        public event Action SwipedDown;
        public event Action PauseClicked;
        public event Action<int> SpawnSpeciesClicked;
        public event Action SpawnBatchClicked;
        public event Action EncounterSelectionChanged;
        public event Action EncounterClicked;
        public event Action RebuildObstaclesClicked;
        public event Action KillOnClickToggled;
        public event Action KillAllClicked;
        public event Action CameraModeClicked;
        public event Action ZoomInClicked;
        public event Action ZoomOutClicked;
        public event Action ResetViewClicked;
        public event Action<float> TimeScaleChanged;

        public int FirstSpecies => _firstSpecies.value;
        public int SecondSpecies => _secondSpecies.value;

        private void Awake()
        {
            _collapseButton.onClick.AddListener(() => CollapseClicked?.Invoke());
            _swipe.SwipedUp += () => SwipedUp?.Invoke();
            _swipe.SwipedDown += () => SwipedDown?.Invoke();
            _pauseButton.onClick.AddListener(() => PauseClicked?.Invoke());
            _spawnBatchButton.onClick.AddListener(() => SpawnBatchClicked?.Invoke());
            _firstSpecies.onValueChanged.AddListener(_ => EncounterSelectionChanged?.Invoke());
            _secondSpecies.onValueChanged.AddListener(_ => EncounterSelectionChanged?.Invoke());
            _encounterButton.onClick.AddListener(() => EncounterClicked?.Invoke());
            _rebuildObstaclesButton.onClick.AddListener(() => RebuildObstaclesClicked?.Invoke());
            _killOnClickButton.onClick.AddListener(() => KillOnClickToggled?.Invoke());
            _killAllButton.onClick.AddListener(() => KillAllClicked?.Invoke());
            _cameraModeButton.onClick.AddListener(() => CameraModeClicked?.Invoke());
            _zoomInButton.onClick.AddListener(() => ZoomInClicked?.Invoke());
            _zoomOutButton.onClick.AddListener(() => ZoomOutClicked?.Invoke());
            _resetViewButton.onClick.AddListener(() => ResetViewClicked?.Invoke());
            _timeScale.onValueChanged.AddListener(value => TimeScaleChanged?.Invoke(value));
        }

        private void Update()
        {
            if (Mathf.Approximately(_slide, _slideTarget))
            {
                return;
            }

            // Unscaled: the panel must work while the game is paused.
            _slide = Mathf.MoveTowards(_slide, _slideTarget, Time.unscaledDeltaTime / _slideDuration);
            ApplySlide();
        }

        public void SetSpecies(IReadOnlyList<string> names)
        {
            foreach (var button in _speciesButtons)
            {
                Destroy(button.gameObject);
            }

            _speciesButtons.Clear();
            for (var i = 0; i < names.Count; i++)
            {
                var button = Instantiate(_speciesButtonPrefab, _speciesButtonsRoot);
                button.Bind(i, names[i]);
                button.Clicked += index => SpawnSpeciesClicked?.Invoke(index);
                _speciesButtons.Add(button);
            }

            var options = new List<string>(names);
            _firstSpecies.ClearOptions();
            _firstSpecies.AddOptions(options);
            _secondSpecies.ClearOptions();
            _secondSpecies.AddOptions(options);
            _secondSpecies.SetValueWithoutNotify(Mathf.Min(1, names.Count - 1));
        }

        public void SetCollapsed(bool isCollapsed, bool animate = true)
        {
            _slideTarget = isCollapsed ? 1f : 0f;
            _collapseArrow.localEulerAngles = new Vector3(0f, 0f, isCollapsed ? 180f : 0f);
            if (!animate)
            {
                _slide = _slideTarget;
                ApplySlide();
            }
        }

        public void SetPaused(bool isPaused) => _pauseLabel.SetText(isPaused ? "Resume spawning" : "Pause spawning");

        public void SetKillOnClick(bool isOn)
        {
            _killOnClickLabel.SetText(isOn ? "Kill on click: ON" : "Kill on click: OFF");
            _killOnClickBackground.color = isOn ? _killOnClickOnColor : _killOnClickOffColor;
        }

        public void SetCameraMode(bool isFree)
        {
            _cameraModeLabel.SetText(isFree ? "Camera: Free" : "Camera: Static");
            _zoomInButton.interactable = isFree;
            _zoomOutButton.interactable = isFree;
            _resetViewButton.interactable = isFree;
            _cameraHint.SetText(isFree ? "Drag, WASD or arrows to move. Wheel or pinch to zoom." : "Fixed view of the whole zoo.");
        }

        public void SetForecast(string forecast) => _forecast.SetText(forecast);

        public void SetTimeScale(float timeScale)
        {
            _timeScale.SetValueWithoutNotify(timeScale);
            _timeScaleLabel.SetText(timeScale <= 0f ? "Time: paused" : $"Time: ×{timeScale:0.0}");
        }

        public void SetInfo(string info) => _info.SetText(info);

        public void ShowStatus(string message) => _status.SetText(message);

        private void ApplySlide()
        {
            var eased = Mathf.SmoothStep(0f, 1f, _slide);
            _panel.anchoredPosition = new Vector2(_panel.anchoredPosition.x, -_content.rect.height * eased);
        }
    }
}
