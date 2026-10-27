using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZooWorld.Cameras
{
    /// <summary>
    /// Tells whether a screen point is over UI, for world input (camera drag, kill on click)
    /// that must ignore presses meant for the control panel. Works for mouse and touch alike.
    /// </summary>
    public static class ScreenUiHitTest
    {
        private static readonly List<RaycastResult> Hits = new();
        private static PointerEventData _pointerData;

        public static bool IsOverUI(Vector2 screenPoint)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            if (_pointerData == null || _pointerData.currentInputModule != eventSystem.currentInputModule)
            {
                _pointerData = new PointerEventData(eventSystem);
            }

            _pointerData.position = screenPoint;
            Hits.Clear();
            eventSystem.RaycastAll(_pointerData, Hits);
            return Hits.Count > 0;
        }
    }
}
