using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchScreen : EventTrigger
{
    public event Action<Vector2> onPointerDown, onPointerUP, onPointerClick;
    public event Action<Vector2, Vector2> onDrag;
    public event Action<Vector2, Vector2, float> onSweep;
    public event Action onceCallPointerDown, onceCallPointerUP;
    public event Action<float> rotateVertical, rotateHorizontal;


    private long m_beginDragTime;
    private Vector2 m_beginDragMousePos = Vector3.zero;
    private Vector2 m_onDragMousePos = Vector3.zero;


    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke(eventData.position);
        onceCallPointerDown?.Invoke();
        onceCallPointerDown = null;

        m_beginDragMousePos = eventData.position;
        m_beginDragTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
   
        onPointerUP?.Invoke(eventData.position);
        onceCallPointerUP?.Invoke();
        onceCallPointerUP = null;

        

        Vector2 sweep = eventData.position - m_beginDragMousePos;
        sweep.x /= Screen.width;
        sweep.y /= Screen.height;
        if (sweep.magnitude > 0.05f)
        {
            float sweepTime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - m_beginDragTime) / 1_000.0f;
            onSweep?.Invoke(m_beginDragMousePos, sweep, sweepTime);
        }
        else onPointerClick?.Invoke(m_beginDragMousePos);

        m_onDragMousePos = eventData.position;
    }
    public override void OnDrag(PointerEventData eventData)
    {
        rotateVertical?.Invoke(eventData.delta.y / Screen.width);
        rotateHorizontal?.Invoke(eventData.delta.x / Screen.width);
        m_onDragMousePos = eventData.position;

        Vector2 sweep = eventData.position - m_beginDragMousePos;
        sweep.x /= Screen.width;
        sweep.y /= Screen.height;
        onDrag?.Invoke(m_beginDragMousePos, eventData.delta);
    }

    /// <summary>
    /// v - длина слайдера в пикселях
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public float GetVerticalSlider(int v)
    {
        return Mathf.Clamp01((m_onDragMousePos.y - m_beginDragMousePos.y) / v);
    }
}
