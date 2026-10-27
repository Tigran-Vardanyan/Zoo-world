using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.World;

namespace ZooWorld.Controls
{
    /// <summary>
    /// Casts a thick ray from the camera the player is looking through and returns the first living animal it hits.
    /// Corpses have no colliders, so they are never picked.
    /// </summary>
    public sealed class CameraAnimalPicker : IAnimalPicker
    {
        private const float MaxDistance = 1000f;

        private readonly IActiveCamera _activeCamera;
        private readonly PickerSettings _settings;

        public CameraAnimalPicker(IActiveCamera activeCamera, PickerSettings settings)
        {
            _activeCamera = activeCamera;
            _settings = settings;
        }

        public bool TryPick(Vector2 screenPoint, out Animal animal)
        {
            animal = null;
            var camera = _activeCamera.Current;
            if (!camera.pixelRect.Contains(screenPoint))
            {
                return false;
            }

            var ray = camera.ScreenPointToRay(screenPoint);
            if (!Physics.SphereCast(ray, _settings.Radius, out var hit, MaxDistance, _settings.AnimalLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            animal = hit.collider.GetComponentInParent<Animal>();
            return animal != null && animal.IsAlive;
        }
    }
}
