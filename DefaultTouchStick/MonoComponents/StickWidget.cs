using System;
using Helpers;
using UnityEngine;

namespace Components
{
    public class StickWidget : MonoBehaviour
    {
        [SerializeField] private bool fixedPosition;
        
        private RectTransform parent;
        private RectTransform controlArea;
        private Vector3 startPos;

        public RectTransform Rect { get; private set; }

        public Vector2 Center => new Vector2(parent.position.x, parent.position.y) + parent.rect.size * parent.lossyScale.Avg() / 2;

        public Vector2 Delta { get; set; }

        private void Awake()
        {
            Rect = (RectTransform) transform;
            parent = (RectTransform) Rect.parent;
            controlArea = (RectTransform)parent.parent;
            startPos = parent.anchoredPosition;
        }

        public void SetPosition(Vector2 position)
        {
            Rect.anchoredPosition = Vector2.zero;
            if (fixedPosition) return;

            parent.anchoredPosition = CalculateAnchoredPosition(position);
        }

        private GameObject prim;
        
        public void UpdatePosition(Vector2 delta)
        {
            if (fixedPosition) return;

            var minX = - controlArea.rect.width * parent.anchorMax.x;
            var minY = - controlArea.rect.height * parent.anchorMax.y;
            parent.anchoredPosition = new Vector2(
                Mathf.Clamp(parent.anchoredPosition.x + delta.x, minX, controlArea.rect.width),
                Mathf.Clamp(parent.anchoredPosition.y + delta.y, minY, controlArea.rect.height));
        }

        public void RestorePosition()
        {
            Rect.anchoredPosition = Vector2.zero;
            if (fixedPosition) return;
            
            parent.anchoredPosition = startPos;
        }

        private Vector2 CalculateAnchoredPosition(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(controlArea, position, null, out var localPoint);
            localPoint -= controlArea.rect.min;
            localPoint -= parent.rect.size / 2;
            localPoint -= controlArea.rect.size * parent.anchorMax;
            return localPoint;
        }
    }
}