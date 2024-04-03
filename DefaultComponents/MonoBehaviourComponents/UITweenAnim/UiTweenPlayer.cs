using System.Collections;
using System.Collections.Generic;
using Components;
using Sirenix.OdinInspector;
using UnityEngine;

public class UiTweenPlayer : MonoBehaviour
{
    [SerializeField] private List<UiTweener> m_tweeners = new List<UiTweener>();
    
    [Button("Play")]
    public void Play()
    {
        foreach (var tweener in m_tweeners)
        {
            tweener.Play();
        }
    }
}
