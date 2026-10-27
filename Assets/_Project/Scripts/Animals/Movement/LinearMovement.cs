using UnityEngine;

namespace ZooWorld.Animals.Movement
{
    /// <summary>
    /// Moves in a straight line at a constant speed, turning only at the play area border.
    /// </summary>
    public sealed class LinearMovement : AnimalMovement
    {
        [SerializeField, Min(0f)] private float _speed = 2f;

        protected override void Move(float deltaTime)
        {
            var velocity = Direction * _speed;
            velocity.y = Body.linearVelocity.y;
            Body.linearVelocity = velocity;

            Body.rotation = Quaternion.LookRotation(Direction);
        }
    }
}
