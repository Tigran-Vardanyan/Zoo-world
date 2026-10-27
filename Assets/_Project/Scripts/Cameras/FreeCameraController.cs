using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using VContainer;
using ZooWorld.World;

namespace ZooWorld.Cameras
{
    /// <summary>
    /// Top-down camera the player can move and zoom:
    /// drag (mouse or one finger) or WASD/arrow keys to move, mouse wheel or two-finger pinch to zoom.
    /// The view centre is kept inside the play area, so the player can't get lost.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class FreeCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        [Tooltip("Orthographic size range: smaller = closer.")]
        [SerializeField] private Vector2 _zoomLimits = new(3f, 12f);

        [Tooltip("Zoom per mouse wheel notch.")]
        [SerializeField, Min(0f)] private float _wheelZoomStep = 0.8f;

        [SerializeField, Min(0f)] private float _keyPanSpeed = 12f;

        [Tooltip("Pixels a press must move before it counts as a drag (so a tap stays a tap).")]
        [SerializeField, Min(0f)] private float _dragThreshold = 8f;

        private IPlayArea _arena;
        private Vector2 _pressStart;
        private Vector2 _lastPointer;
        private bool _isPressing;
        private bool _isDragging;
        private float _lastPinchDistance;

        public Camera Camera => _camera;

        [Inject]
        public void Construct(IPlayArea arena)
        {
            _arena = arena;
        }

        /// <summary>Takes over from <paramref name="source"/> with exactly the same view, then starts rendering.</summary>
        public void Activate(Camera source)
        {
            CopyView(source);
            _camera.enabled = true;
        }

        public void Deactivate()
        {
            _camera.enabled = false;
            _isPressing = _isDragging = false;
        }

        public void CopyView(Camera source)
        {
            transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            _camera.orthographicSize = source.orthographicSize;
        }

        /// <summary>Positive = zoom in.</summary>
        public void Zoom(float amount)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - amount, _zoomLimits.x, _zoomLimits.y);
            KeepInsideArena();
        }

        private void Update()
        {
            if (!_camera.enabled)
            {
                return;
            }

            if (!HandlePinch())
            {
                HandleDrag();
            }

            HandleWheel();
            HandleKeys();
            KeepInsideArena();
        }

        private bool HandlePinch()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null || CountPressed(touchscreen) < 2)
            {
                _lastPinchDistance = 0f;
                return false;
            }

            var a = touchscreen.touches[0].position.ReadValue();
            var b = touchscreen.touches[1].position.ReadValue();
            var distance = Vector2.Distance(a, b);
            if (_lastPinchDistance > 0f && distance > 0f)
            {
                var size = _camera.orthographicSize * (_lastPinchDistance / distance);
                _camera.orthographicSize = Mathf.Clamp(size, _zoomLimits.x, _zoomLimits.y);
            }

            _lastPinchDistance = distance;
            _isPressing = _isDragging = false;
            return true;
        }

        private void HandleDrag()
        {
            var pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            var position = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame && !ScreenUiHitTest.IsOverUI(position))
            {
                _isPressing = true;
                _isDragging = false;
                _pressStart = _lastPointer = position;
            }

            if (_isPressing && pointer.press.isPressed)
            {
                if (!_isDragging && (position - _pressStart).sqrMagnitude >= _dragThreshold * _dragThreshold)
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    // Move the camera opposite to the drag so the ground follows the finger / cursor.
                    var worldPerPixel = 2f * _camera.orthographicSize / _camera.pixelHeight;
                    var delta = (position - _lastPointer) * worldPerPixel;
                    transform.position -= new Vector3(delta.x, 0f, delta.y);
                }

                _lastPointer = position;
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                _isPressing = _isDragging = false;
            }
        }

        private void HandleWheel()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f && !ScreenUiHitTest.IsOverUI(mouse.position.ReadValue()))
            {
                Zoom(Mathf.Sign(scroll) * _wheelZoomStep);
            }
        }

        private void HandleKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var input = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;

            if (input != Vector2.zero)
            {
                // Unscaled time: the camera keeps working while the game is paused.
                var step = input.normalized * (_keyPanSpeed * Time.unscaledDeltaTime);
                transform.position += new Vector3(step.x, 0f, step.y);
            }
        }

        private void KeepInsideArena()
        {
            var bounds = _arena.Bounds;
            var position = transform.position;
            position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
            position.z = Mathf.Clamp(position.z, bounds.yMin, bounds.yMax);
            transform.position = position;
        }

        private static int CountPressed(Touchscreen touchscreen)
        {
            var count = 0;
            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.isPressed)
                {
                    count++;
                }
            }

            return count;
        }

        private void Reset()
        {
            _camera = GetComponent<Camera>();
        }
    }
}
