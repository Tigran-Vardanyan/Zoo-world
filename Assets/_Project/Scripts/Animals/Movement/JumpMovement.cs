using UnityEngine;
using ZooWorld.Utils;

namespace ZooWorld.Animals.Movement
{
    /// <summary>
    /// Every <see cref="_interval"/> seconds jumps a fixed distance in a new random direction.
    /// The launch velocity is derived from the jump distance and gravity, so the distance stays
    /// correct when gravity or the jump angle are changed.
    /// </summary>
    public sealed class JumpMovement : AnimalMovement
    {
        [SerializeField, Min(0.1f)] private float _interval = 1.5f;
        [SerializeField, Min(0f)] private float _distance = 2f;
        [SerializeField, Range(10f, 80f)] private float _angle = 45f;

        private float _timeUntilJump;
        private bool _keepDirectionForNextJump;

        protected override void OnStartedMoving()
        {
            _timeUntilJump = Random.Range(0f, _interval);
            _keepDirectionForNextJump = false;
        }

        protected override void OnDeflected()
        {
            // After bouncing off an obstacle, jump away from it instead of picking a random direction.
            _keepDirectionForNextJump = true;
        }

        protected override void Move(float deltaTime)
        {
            _timeUntilJump -= deltaTime;
            if (_timeUntilJump > 0f)
            {
                return;
            }

            _timeUntilJump = _interval;
            if (!_keepDirectionForNextJump)
            {
                SetDirection(GroundDirection.Random());
            }

            _keepDirectionForNextJump = false;
            AimInside(_distance);
            Jump();
        }

        private void Jump()
        {
            // Projectile range on flat ground: d = v^2 * sin(2a) / g  =>  v = sqrt(d * g / sin(2a)).
            var angle = _angle * Mathf.Deg2Rad;
            var gravity = Mathf.Abs(Physics.gravity.y);
            var speed = Mathf.Sqrt(_distance * gravity / Mathf.Sin(2f * angle));

            Body.linearVelocity = Direction * (speed * Mathf.Cos(angle)) + Vector3.up * (speed * Mathf.Sin(angle));
            Body.rotation = Quaternion.LookRotation(Direction);
        }
    }
}
