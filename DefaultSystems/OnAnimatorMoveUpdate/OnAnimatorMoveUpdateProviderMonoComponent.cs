using System;
using HECSFramework.Core;
using UnityEngine;

[Documentation(Doc.HECS, Doc.Animation, Doc.OnAnimatorMoveUpdate, "this system provide update functionality by animator, this is part of animation move update")]
public class OnAnimatorMoveUpdateProviderMonoComponent : MonoBehaviour
{
    public IUpdateOnAnimatorMove[] OnAnimatorMoveUpdatables = Array.Empty<IUpdateOnAnimatorMove>();

    private void OnAnimatorMove()
    {
        foreach (var onAnimatorUpdate  in OnAnimatorMoveUpdatables)
        {
            onAnimatorUpdate.UpdateOnAnimatorMove();
        }
    }
}