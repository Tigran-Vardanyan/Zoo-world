using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZooWorld.UI
{
    /// <summary>
    /// Detects a vertical swipe (mouse drag or finger) that starts anywhere on this UI element.
    /// Controls that use drags themselves (sliders, dropdown lists) keep them; buttons pass them through.
    /// </summary>
    public sealed class PanelSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Minimum vertical distance, in screen pixels, for a drag to count as a swipe.")]
        [SerializeField, Min(1f)] private float _minDistance = 40f;

        private Vector2 _start;

        public event Action SwipedUp;
        public event Action SwipedDown;

        public void OnBeginDrag(PointerEventData eventData) => _start = eventData.position;

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var delta = eventData.position - _start;
            if (Mathf.Abs(delta.y) < _minDistance || Mathf.Abs(delta.y) < Mathf.Abs(delta.x))
            {
                return;
            }

            if (delta.y > 0f)
            {
                SwipedUp?.Invoke();
            }
            else
            {
                SwipedDown?.Invoke();
            }
        }
    }
}
