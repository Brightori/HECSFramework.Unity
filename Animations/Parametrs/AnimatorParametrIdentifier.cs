using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorParametrIdentifier : ScriptableObject
{
    [SerializeField] private int id;
    public void SetID (int id)
    {
        this.id = id;
    }
}