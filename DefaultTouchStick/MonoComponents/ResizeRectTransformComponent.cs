using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class ResizeRectTransformComponent : MonoBehaviour
{
    [Button]
    public void ToFullScreen()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(0.9f, 1);
        rt.sizeDelta = Vector2.zero;
    }

    [Button]
    public void ToDefault()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.sizeDelta = new Vector2(560, 545);
    }
}
