using System;
using Components;
using DG.Tweening;
using UnityEngine;

[Serializable]
    public class UiAnimTweenObj
    {
        [SerializeField] private UiTweener.TweenAnimType m_tweenAnimType = UiTweener.TweenAnimType.Rotate;
        [SerializeField] private RectTransform m_rect = null;
        
        [SerializeField] private Vector3 m_startValue = new Vector3();
        [SerializeField] private Vector3 m_endValue = new Vector3();
        
        [SerializeField] private Color m_startColor = Color.white;
        [SerializeField] private Color m_endColor = Color.white;
        
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_startAlpha = 0f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_endAlpha = 1f;
        [SerializeField] private bool m_changeInteractable = false;
        
        [SerializeField] private float m_time = 0f;
        [SerializeField] private float m_delay = 0f;
        [SerializeField] private bool m_needLoop = false;
        [SerializeField] private LoopType m_loopType = LoopType.Restart;
        [SerializeField] private AnimationCurve m_animCurveType = AnimationCurve.Linear(0, 0, 1, 1);

        public RectTransform Rect => m_rect;
        
        public Vector3 StartValue => m_startValue;
        public Vector3 EndValue => m_endValue;
        
        public Color StartColor => m_startColor;
        public Color EndColor => m_endColor;

        public float StartAlpha => m_startAlpha;
        public float EndAlpha => m_endAlpha;
        public bool ChangeInteractable => m_changeInteractable;
        
        public float Time => m_time;
        public float Delay => m_delay;
        public bool NeedLoop => m_needLoop;
        public AnimationCurve AnimCurveType => m_animCurveType;
        public LoopType LoopTweenType => m_loopType;
        public UiTweener.TweenAnimType TweenAnimType => m_tweenAnimType;
    }