using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using ZooWorld.Animals;
using ZooWorld.Cameras;
using ZooWorld.Controls;

namespace ZooWorld.UI
{
    /// <summary>
    /// While active, a tap or click on the zoo kills the animal under it. It reacts on release and only when
    /// the pointer barely moved, so dragging to move the free camera never kills anything; presses that start
    /// on UI are ignored. Uses <see cref="Pointer"/>, which covers mouse, pen and touch.
    /// </summary>
    public sealed class KillOnClickInput : ITickable
    {
        private const float TapTolerance = 10f;

        private readonly IZooCommands _commands;
        private Vector2 _pressPosition;
        private bool _isTapCandidate;

        public KillOnClickInput(IZooCommands commands)
        {
            _commands = commands;
        }

        public event Action<Animal> Killed;
        public event Action Missed;

        public bool IsActive { get; set; }

        public void Tick()
        {
            var pointer = Pointer.current;
            if (!IsActive || pointer == null)
            {
                _isTapCandidate = false;
                return;
            }

            var position = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                _pressPosition = position;
                _isTapCandidate = !ScreenUiHitTest.IsOverUI(position);
            }

            if (_isTapCandidate && pointer.press.isPressed && (position - _pressPosition).sqrMagnitude > TapTolerance * TapTolerance)
            {
                _isTapCandidate = false;
            }

            if (pointer.press.wasReleasedThisFrame && _isTapCandidate)
            {
                _isTapCandidate = false;
                var victim = _commands.KillAt(position);
                if (victim != null)
                {
                    Killed?.Invoke(victim);
                }
                else
                {
                    Missed?.Invoke();
                }
            }
        }
    }
}
