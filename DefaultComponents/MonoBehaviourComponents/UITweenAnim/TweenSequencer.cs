using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using static AnimationStep;

public class TweenSequencer : MonoBehaviour, IGetScenarioTween
{
    public SequenceScenario[] SequenceScenario = new SequenceScenario[0];

    private HashSet<Tween> activeTweens = new HashSet<Tween>();

    private void Awake()
    {
        foreach (var s in SequenceScenario)
        {
            s.Init(this);
        }
    }

    public async UniTask PlayAsync(int actionIdentifier, CancellationToken cancellationToken = default)
    {
        var seq = DOTween.Sequence();

        foreach (var scenario in SequenceScenario)
        {
            if (scenario.ActionIdentifier.Id == actionIdentifier)
            {
                seq.Append(scenario.GetTween());
            }
        }
        
        cancellationToken.ThrowIfCancellationRequested();   

        activeTweens.Add(seq);

        seq.Play();
        
        await seq.AsyncWaitForCompletion();
        
        seq.Kill();
        activeTweens.Remove(seq);
        
        cancellationToken.ThrowIfCancellationRequested();   
    }
    
    public void Play(int actionidentifier, Action onComplete = null)
    {
        var seq = DOTween.Sequence();

        foreach (var scenario in SequenceScenario)
        {
            if (scenario.ActionIdentifier.Id == actionidentifier)
            {
                seq.Append(scenario.GetTween());
            }
        }

        activeTweens.Add(seq);

        seq.OnComplete(() =>
            {
                onComplete?.Invoke();
                seq.Kill();
                activeTweens.Remove(seq);
            }
        );

        seq.Play();
    }

    public void Stop()
    {
        foreach (var tween in activeTweens)
        {
            //tween.Rewind();
            tween.Kill();
        }

        activeTweens.Clear();
    }

    [Button]
    public void Play(ActionIdentifier actionidentifier, Action onComplete = null)
    {
        Play(actionidentifier.Id, onComplete);
    }

    public Tween GetScenarioTween(ActionIdentifier actionIdentifier)
    {
        return SequenceScenario.FirstOrDefault(x => x.ActionIdentifier.Id == actionIdentifier.Id)?.GetTween();
    }
}

[Serializable]
public class SequenceScenario : INeedGetScenario
{
    [GUIColor("#c669f5")]
    public ActionIdentifier ActionIdentifier;

    [GUIColor("#9ab8b6")]
    [ListDrawerSettings(ShowFoldout = false, ShowIndexLabels = false )]
    public ScenarioStep[] Animations;

    public Tween GetTween()
    {
        var sequence = DOTween.Sequence();

        foreach (var scenarioStep in Animations)
        {
            switch (scenarioStep.JoinOrAppend)
            {
                case JoinOrAppend.Join:
                    sequence.Join(scenarioStep.GetTween());
                    break;
                case JoinOrAppend.Append:
                    sequence.Append(scenarioStep.GetTween());
                    break;
                case JoinOrAppend.Interval:
                    break;
            }
        }

        return sequence;
    }

    public void Init(IGetScenarioTween getScenarioTween)
    {
        foreach (var step in Animations)
        {
            step.Init(getScenarioTween);
        }
    }
}

[Serializable]
[GUIColor("#d3edeb")]
public class ScenarioStep : INeedGetScenario
{
    private IGetScenarioTween getScenarioTween;

    public enum TypeStep { Animations, Step, Particles }

    public JoinOrAppend JoinOrAppend = JoinOrAppend.Append;

    [GUIColor("#f5cb58")]
    public TypeStep ScenarioType = TypeStep.Animations;

    [ShowIf(nameof(ScenarioType), TypeStep.Animations)]
    public AnimationStep[] Animations;

    [ShowIf(nameof(ScenarioType), TypeStep.Step)]
    public ActionIdentifier AnimationScenario;

    [ShowIf(nameof(ScenarioType), TypeStep.Particles)]
    public ParticleSystem ParticleSystem;

    public void Init(IGetScenarioTween getScenarioTween)
    {
        this.getScenarioTween = getScenarioTween;
    }

    public Tween GetTween()
    {
        Sequence sequence = DOTween.Sequence();
        
        switch (ScenarioType)
        {
            case TypeStep.Animations:


                foreach (var a in Animations)
                {
                    switch (a.SequenceType)
                    {
                        case JoinOrAppend.Join:
                            a.DOTweenAnimation.CreateTween(false, false);
                            sequence.Join(a.DOTweenAnimation.tween);
                            break;
                        case JoinOrAppend.Append:
                            a.DOTweenAnimation.CreateTween(false, false);
                            sequence.Append(a.DOTweenAnimation.tween);
                            break;
                        case JoinOrAppend.Interval:
                            sequence.AppendInterval(a.Interval);
                            break;
                    }
                }

                return sequence;
            case TypeStep.Step:
                return getScenarioTween.GetScenarioTween(AnimationScenario);
            case TypeStep.Particles:
                return sequence.AppendInterval(0).AppendCallback(() => ParticleSystem.Play());
        }

        return null;
    }


}

[Serializable]
public class AnimationStep
{
    public enum JoinOrAppend { Join, Append, Interval }

    public JoinOrAppend SequenceType;

    [HideIf(nameof(SequenceType), JoinOrAppend.Interval)]
    public DOTweenAnimation DOTweenAnimation;

    [ShowIf(nameof(SequenceType), JoinOrAppend.Interval)]
    public float Interval;
}

public interface INeedGetScenario
{
    void Init(IGetScenarioTween getScenarioTween);
}

public interface IGetScenarioTween
{
    Tween GetScenarioTween(ActionIdentifier actionIdentifier);
}
