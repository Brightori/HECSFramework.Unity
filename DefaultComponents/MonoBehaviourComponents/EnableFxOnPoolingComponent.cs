using HECSFramework.Core;
using UnityEngine;

[Documentation(Doc.Poolable, Doc.FX, Doc.Visual, "We tagging poolable fx with this component")]
public class EnableFxOnPoolingComponent : MonoBehaviour, IStartOnPooling, IStopOnPooling
{
    public enum ObjectsToEnable { ParticlesOnly, ParticlesAndAnimationClip, AnimationClipOnly }
    
    private ParticleSystem[] particleSystems;
    private Animation animationClip = default;
    [SerializeField] private ObjectsToEnable objectsToEnable = ObjectsToEnable.ParticlesAndAnimationClip;


    private void Awake()
    {
        animationClip = GetComponent<Animation>();
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public void StartOnPooling()
    {
        switch (objectsToEnable)
        {
            case ObjectsToEnable.ParticlesOnly:
                PlayParticles();
                break;
            case ObjectsToEnable.ParticlesAndAnimationClip:
                PlayParticles();
                animationClip.Play();
                break;
            case ObjectsToEnable.AnimationClipOnly:
                animationClip.Play();
                break;
        }
    }

    private void PlayParticles()
    {
        foreach (var p in particleSystems)
            p.Play();
    }

    public void Stop()
    {
        foreach (var p in particleSystems)
            p.Stop();
    }
}