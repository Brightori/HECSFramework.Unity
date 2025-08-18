using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    private Vector2 startPosition;
    private bool hideJoystick;

    protected override void Start()
    {
        base.Start();
        startPosition = background.anchoredPosition;
        if (hideJoystick)
            background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        if (hideJoystick)
            background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (hideJoystick)
            background.gameObject.SetActive(false);
        background.anchoredPosition = startPosition;
        base.OnPointerUp(eventData);
    }
}