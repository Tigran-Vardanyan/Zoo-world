using UnityEngine;
using ZooWorld.World;

namespace ZooWorld.Cameras
{
    /// <summary>
    /// Owns which camera renders. The arena camera always defines the play area (it is never moved),
    /// so zooming or panning the free camera never changes where animals live or turn back.
    /// </summary>
    public sealed class CameraSwitcher : ICameraSwitcher, IActiveCamera
    {
        private readonly Camera _arenaCamera;
        private readonly FreeCameraController _freeCamera;

        public CameraSwitcher(Camera arenaCamera, FreeCameraController freeCamera)
        {
            _arenaCamera = arenaCamera;
            _freeCamera = freeCamera;
            _freeCamera.Deactivate();
        }

        public bool IsFree { get; private set; }

        public Camera Current => IsFree ? _freeCamera.Camera : _arenaCamera;

        public void SetFree(bool isFree)
        {
            if (isFree == IsFree)
            {
                return;
            }

            IsFree = isFree;
            if (isFree)
            {
                _freeCamera.Activate(_arenaCamera);
                _arenaCamera.enabled = false;
            }
            else
            {
                _arenaCamera.enabled = true;
                _freeCamera.Deactivate();
            }
        }

        public void Zoom(float amount)
        {
            if (IsFree)
            {
                _freeCamera.Zoom(amount);
            }
        }

        public void ResetView() => _freeCamera.CopyView(_arenaCamera);
    }
}
