using System;
using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchScreen : EventTrigger
{
    private struct TouchData
    {
        public int PointerId;
        public Vector2 BeginTouchPos;
        public Vector3 TouchPos;
    }
    public event Action<Vector2> PointerDown, PointerUp, PointerClick;
    public event Action<Vector2, Vector2> Drag;
    public event Action<float> Zoom; //delta
    public event Action<Vector2, Vector2, float> Sweep;
    public event Action<float> RotateVertical, RotateHorizontal;

    private long beginDragTime;

    private readonly TouchData[] touches = new TouchData[10];
    private int touchCount = 0;
    private float lastZoomDistance = 0f;
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (touchCount >= touches.Length)
            return;

        PointerDown?.Invoke(eventData.position);
        touches[touchCount++] = new TouchData
        {
            PointerId = eventData.pointerId,
            BeginTouchPos = eventData.position,
            TouchPos = eventData.position
        };
        beginDragTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        var index = GetIndexOfTouch(eventData.pointerId);
        if (index == -1)
            return;
        PointerUp?.Invoke(eventData.position);
        Vector2 sweep = eventData.position - touches[index].BeginTouchPos;
        sweep.x /= Screen.width;
        sweep.y /= Screen.height;
        if (sweep.magnitude > 0.05f)
        {
            float sweepTime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - beginDragTime) / 1_000.0f;
            Sweep?.Invoke(touches[index].BeginTouchPos, sweep, sweepTime);
        }
        else PointerClick?.Invoke(touches[index].BeginTouchPos);

        for (int i = index; i < touchCount - 1; i++)
        {
            touches[i] = touches[i + 1];
        }
        touchCount--;
        // touchCount = 0;
        lastZoomDistance = 0;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        var index = GetIndexOfTouch(eventData.pointerId);
        if (index == -1)
            return;
        RotateVertical?.Invoke(eventData.delta.y / Screen.width);
        RotateHorizontal?.Invoke(eventData.delta.x / Screen.width);
        var touchData = touches[index];
        touchData.TouchPos = eventData.position;
        touches[index] = touchData;

        Vector2 sweep = eventData.position - touchData.BeginTouchPos;
        sweep.x /= Screen.width;
        sweep.y /= Screen.height;
        if (touchCount == 1)
        {
            Drag?.Invoke(touchData.BeginTouchPos, eventData.delta);
        }

        if (touchCount == 2)
        {
            var distance = Vector2.Distance(touches[0].TouchPos, touches[1].TouchPos);
            if (lastZoomDistance == 0f)
            {
                lastZoomDistance = distance;
                return;
            }
            Zoom?.Invoke(distance - lastZoomDistance);
            lastZoomDistance = distance;
        }
    }

    private int GetIndexOfTouch(int pointerId)
    {
        for (var i = 0; i < touches.Length; i++)
        {
            var touch = touches[i];
            if (touch.PointerId == pointerId)
            {
                return i;
            }
        }
        return -1;
    }
}