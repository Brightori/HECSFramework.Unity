using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StickPanelWidget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform parent;

    public event Action<Vector2> PointerDown;
    public event Action<Vector2> PointerUp;
    public event Action<Vector2> Dragged;

    private void Awake()
    {
        var rect = (RectTransform)transform;
        parent = (RectTransform)rect.parent;
    }

    public void OnPointerDown(PointerEventData eventData)
        => PointerDown?.Invoke(eventData.position);

    public void OnPointerUp(PointerEventData eventData)
        => PointerUp?.Invoke(eventData.position);

    public void OnDrag(PointerEventData eventData)
        => Dragged?.Invoke(eventData.position);
}
