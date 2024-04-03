using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    [Documentation(Doc.UI, Doc.FX, "this is tweener for ui elements, we use it on win screen for example")]
    public class UiTweener : MonoBehaviour, IUITweener
    {
        [SerializeField] private ActionIdentifier actionIdentifier;

        public enum TweenAnimType
        {
            Rotate,
            Scale,
            MoveToAnchoredPosition,
            Color,
            FillAmount,
            CanvasGroup,
        }

        [SerializeField] private bool m_playOnAwake = false;
        [SerializeField] private List<UiAnimTweenObj> m_animTweenObjs = new List<UiAnimTweenObj>();

        private Dictionary<Tween, TweenAnimType> m_tweensMap = new Dictionary<Tween, TweenAnimType>();
        private bool m_isPlaying = false;

        public bool IsPlaying => m_isPlaying;

        public int ID
        {
            get
            {
                if (actionIdentifier != null)
                    return actionIdentifier.Id;

                return 0;
            }
        }

        private void Awake()
        {
            if (m_playOnAwake) Play();
        }

        private void OnEnable()
        {
            if (m_playOnAwake) Play();
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void OnTweenerComplete(Tween tweener)
        {
            m_tweensMap.Remove(tweener);
            m_isPlaying = m_tweensMap.Any(e => e.Key.IsPlaying());
        }

        public virtual void TweenerComplete()
        {
        }

        private void KillTween(IEnumerable<Tween> tweens)
        {
            foreach (var tw in tweens)
            {
                tw.Kill();
                m_tweensMap.Remove(tw);
            }
        }

        private List<Tween> GetAllTweensInObject(UiAnimTweenObj tween)
        {
            var tweensRect = DOTween.TweensByTarget(tween.Rect);
            var tweensGraphic = DOTween.TweensByTarget(tween.Rect.GetComponent<Graphic>());
            var tweensCanvas = DOTween.TweensByTarget(tween.Rect.GetComponent<CanvasGroup>());

            var allTypes = new[] {tweensRect, tweensGraphic, tweensCanvas}.Where(x => x != null).SelectMany(x => x)
                .ToList();
            return allTypes;
        }

        private void ApplyTweenAnim(UiAnimTweenObj tween)
        {
            List<Tween> tweens = GetAllTweensInObject(tween);
            if (tweens != null)
            {
                var forKill = tweens.Where(m_tweensMap.ContainsKey)
                    .Where(t => m_tweensMap[t] == tween.TweenAnimType);
                KillTween(forKill);
            }

            Tween tweener = null;
            switch (tween.TweenAnimType)
            {
                case TweenAnimType.Rotate:
                    tween.Rect.localRotation = Quaternion.Euler(tween.StartValue);
                    tweener = tween.Rect.DOLocalRotate(tween.EndValue, tween.Time, RotateMode.FastBeyond360);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() => OnTweenerComplete(tweener))
                        .SetUpdate(true);
                    break;
                case TweenAnimType.Scale:
                    tween.Rect.localScale = tween.StartValue;
                    tweener = tween.Rect.DOScale(tween.EndValue, tween.Time);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() => OnTweenerComplete(tweener))
                        .SetUpdate(true);
                    break;
                case TweenAnimType.MoveToAnchoredPosition:
                    tween.Rect.anchoredPosition = tween.StartValue;
                    tweener = tween.Rect.DOAnchorPos(tween.EndValue, tween.Time);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() => OnTweenerComplete(tweener))
                        .SetUpdate(true);
                    break;
                case TweenAnimType.Color:
                    var graphic = tween.Rect.GetComponent<Graphic>();
                    if (graphic == null) return;
                    graphic.color = tween.StartColor;
                    tweener = graphic.DOColor(tween.EndColor, tween.Time);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() => OnTweenerComplete(tweener))
                        .SetUpdate(true);
                    break;
                case TweenAnimType.FillAmount:
                    var image = tween.Rect.GetComponent<Image>();
                    if (image == null) return;
                    image.fillAmount = tween.StartAlpha;
                    tweener = image.DOFillAmount(tween.EndAlpha, tween.Time);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() => OnTweenerComplete(tweener))
                        .SetUpdate(true);
                    break;
                case TweenAnimType.CanvasGroup:
                    var canvas = tween.Rect.GetComponent<CanvasGroup>();
                    if (canvas == null) return;
                    canvas.alpha = tween.StartAlpha;
                    if (tween.ChangeInteractable && (tween.StartAlpha < 1 || tween.EndAlpha < 1))
                    {
                        canvas.interactable = false;
                        canvas.blocksRaycasts = false;
                    }

                    tweener = canvas.DOFade(tween.EndAlpha, tween.Time);
                    tweener.SetEase(tween.AnimCurveType).SetLoops(tween.NeedLoop ? -1 : 1, tween.LoopTweenType)
                        .SetDelay(tween.Delay)
                        .OnComplete(() =>
                        {
                            if (tween.ChangeInteractable && Math.Abs(tween.EndAlpha - 1) < 0.1f)
                            {
                                canvas.interactable = true;
                                canvas.blocksRaycasts = true;
                            }

                            OnTweenerComplete(tweener);
                        })
                        .SetUpdate(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tweener != null) m_tweensMap.Add(tweener, tween.TweenAnimType);
        }

        public float GetLongestTime()
        {
            return m_animTweenObjs.Max(ato => ato.Time);
        }

        public void Play(TweenCallback action = null)
        {
            foreach (UiAnimTweenObj tween in m_animTweenObjs)
            {
                ApplyTweenAnim(tween);
            }

            var delay = m_animTweenObjs.Max(ato => ato.Time + ato.Delay);
            Sequence mySequence = DOTween.Sequence();
            mySequence.PrependInterval(delay);
            mySequence.AppendCallback(action);
            m_isPlaying = true;
        }

        // protected void PlayByIndex(int index)
        // {
        //     if (m_animTweenObjs[index] == null) return;
        //     ApplyTweenAnim(m_animTweenObjs[index]);
        // }

        // protected void PlayRandom()
        // {
        //     var index = Random.Range(0, m_animTweenObjs.Count - 1);
        //     PlayByIndex(index);
        //     if (m_animTweenObjs[index] == null) return;
        //     ApplyTweenAnim(m_animTweenObjs[index]);
        // }
        //
        // protected void PlayRandomRange(int index1, int index2)
        // {
        //     var index = Random.Range(index1, index2);
        //     PlayByIndex(index);
        //     if (m_animTweenObjs[index] == null) return;
        //     ApplyTweenAnim(m_animTweenObjs[index]);
        // }

        public void Stop()
        {
            foreach (UiAnimTweenObj tween in m_animTweenObjs)
            {
                var tweens = GetAllTweensInObject(tween);
                if (tweens == null) return;
                var forKill = tweens.Where(m_tweensMap.ContainsKey);
                KillTween(forKill);
            }

            m_isPlaying = false;
        }

        // protected void StopByIndex(int index)
        // {
        //     if (m_animTweenObjs[index] == null) return;
        //     UiAnimTweenObj tween = m_animTweenObjs[index];
        //     if (tween == null) return;
        //
        //     List<Tween> tweens = DOTween.TweensByTarget(tween.Rect);
        //     if (tweens == null) return;
        //     var forKill = tweens.Where(m_tweensMap.ContainsKey);
        //     KillTween(forKill);
        // }

        public void Restart()
        {
            Stop();
            Play();
        }


        // public bool IsPlaying()
        // {
        //     return m_animTweenObjs.Any(tw =>
        //         DOTween.IsTweening(tw.Rect || tw.Rect.GetComponent<Graphic>() || tw.Rect.GetComponent<CanvasGroup>()));
        // }

        // public Tween GetLongest()
        // {
        //     Tween longest = null;
        //     foreach (var tweenAnimType in m_tweensMap)
        //     {
        //         if (longest == null)
        //         {
        //             longest = tweenAnimType.Key;
        //             continue;
        //         }
        //
        //         if (tweenAnimType.Key.Duration() > longest.Duration())
        //         {
        //             longest = tweenAnimType.Key;
        //         }
        //     }
        //     
        //     return longest;
        // }

        public bool HaveAnyLoopTween()
        {
            return m_animTweenObjs.Any(e => e.NeedLoop);
        }
    }

    public interface IUITweener
    {
        int ID { get; }
        void Play(TweenCallback action = null);
        void Stop();
    }
}