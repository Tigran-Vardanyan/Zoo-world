using UnityEngine;
using VContainer;
using ZooWorld.Utils;
using ZooWorld.World;

namespace ZooWorld.Animals.Movement
{
    /// <summary>
    /// Base class for the ways an animal can move. A new kind of movement (flying, swimming, crawling...)
    /// is a new subclass that only implements <see cref="Move"/>; staying on screen and bouncing off
    /// obstacles are handled here for all of them.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class AnimalMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody _body;

        private IPlayArea _playArea;

        public bool IsMoving { get; private set; }

        protected Rigidbody Body => _body;
        protected Vector3 Direction { get; private set; } = Vector3.forward;

        [Inject]
        public void Construct(IPlayArea playArea)
        {
            _playArea = playArea;
        }

        public void StartMoving(Vector3 direction)
        {
            SetDirection(direction);
            IsMoving = true;
            OnStartedMoving();
        }

        public void StopMoving()
        {
            IsMoving = false;
        }

        /// <summary>Called once per physics step while moving.</summary>
        protected abstract void Move(float deltaTime);

        protected virtual void OnStartedMoving()
        {
        }

        /// <summary>Called after the direction was changed by bouncing off an obstacle.</summary>
        protected virtual void OnDeflected()
        {
        }

        protected void SetDirection(Vector3 direction)
        {
            Direction = GroundDirection.Flatten(direction);
            KeepInsidePlayArea();
        }

        /// <summary>
        /// Turns the direction back inside the play area if moving <paramref name="distance"/> along it
        /// would end outside, e.g. for movements that cover a long distance at once.
        /// </summary>
        protected void AimInside(float distance)
        {
            Direction = _playArea.RedirectInside(_body.position + Direction * distance, Direction);
        }

        private void FixedUpdate()
        {
            if (!IsMoving)
            {
                return;
            }

            KeepInsidePlayArea();
            Move(Time.fixedDeltaTime);
        }

        private void OnCollisionEnter(Collision collision) => DeflectOffObstacle(collision);

        private void OnCollisionStay(Collision collision) => DeflectOffObstacle(collision);

        private void KeepInsidePlayArea()
        {
            var position = _body.position;
            Direction = _playArea.RedirectInside(position, Direction);

            // External forces (collisions, pushes, jumps) can also carry an animal out; bounce its velocity back.
            var velocity = _body.linearVelocity;
            var horizontal = new Vector3(velocity.x, 0f, velocity.z);
            var redirected = _playArea.RedirectInside(position, horizontal);
            if (redirected != horizontal)
            {
                _body.linearVelocity = new Vector3(redirected.x, velocity.y, redirected.z);
            }
        }

        private void DeflectOffObstacle(Collision collision)
        {
            if (!IsMoving || collision.collider.GetComponentInParent<Obstacle>() == null)
            {
                return;
            }

            var contact = collision.GetContact(0);
            var normal = contact.normal;
            if (Vector3.Dot(normal, _body.position - contact.point) < 0f)
            {
                normal = -normal;
            }

            var deflected = GroundDirection.Deflect(Direction, normal);
            if (deflected != Direction)
            {
                SetDirection(deflected);
                OnDeflected();
            }
        }

        protected virtual void Reset()
        {
            _body = GetComponent<Rigidbody>();
        }
    }
}
