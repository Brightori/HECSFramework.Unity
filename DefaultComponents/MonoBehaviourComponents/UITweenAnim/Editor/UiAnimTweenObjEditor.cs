using System;
using Components;
using UnityEditor;
using UnityEngine;

namespace Playstrom.Core.Ui
{
    [CustomPropertyDrawer(typeof(UiAnimTweenObj))]
    public class UiAnimTweenObjEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int counter = 8;
            if (property.displayName.StartsWith("Element"))
            {
                counter++;
                if (!property.isExpanded)
                {
                    return base.GetPropertyHeight(property, label);
                }
            }

            var needLoop = property.FindPropertyRelative("m_needLoop");
            if (needLoop.boolValue)
            {
                counter++;
            }
            var tweenAnimTypeProp = property.FindPropertyRelative("m_tweenAnimType");
            if ((UiTweener.TweenAnimType) tweenAnimTypeProp.intValue == UiTweener.TweenAnimType.CanvasGroup)
            {
                counter++;
            }

            return base.GetPropertyHeight(property, label) * counter + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.height = base.GetPropertyHeight(property, label);
            if (property.displayName.StartsWith("Element"))
            {
                EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), false);
                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                rect.y += base.GetPropertyHeight(property, label);
                EditorGUI.indentLevel++;
            }

            var tweenAnimTypeProp = property.FindPropertyRelative("m_tweenAnimType");
            EditorGUI.PropertyField(rect, tweenAnimTypeProp, new GUIContent(tweenAnimTypeProp.displayName));
            rect.y += base.GetPropertyHeight(property, label);

            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_rect"));
            rect.y += base.GetPropertyHeight(property, label);

            // Create property fields.
            SerializedProperty startValueProp = null;
            switch ((UiTweener.TweenAnimType) tweenAnimTypeProp.intValue)
            {
                case UiTweener.TweenAnimType.Rotate:
                case UiTweener.TweenAnimType.Scale:
                case UiTweener.TweenAnimType.MoveToAnchoredPosition:
                    startValueProp = property.FindPropertyRelative("m_startValue");
                    break;
                case UiTweener.TweenAnimType.Color:
                    startValueProp = property.FindPropertyRelative("m_startColor");
                    break;
                case UiTweener.TweenAnimType.FillAmount:
                    startValueProp = property.FindPropertyRelative("m_startAlpha");
                    break;
                case UiTweener.TweenAnimType.CanvasGroup:
                    startValueProp = property.FindPropertyRelative("m_startAlpha");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.PropertyField(rect, startValueProp, new GUIContent(startValueProp.displayName));
            rect.y += base.GetPropertyHeight(property, label);

            SerializedProperty endValueProp = null;
            switch ((UiTweener.TweenAnimType) tweenAnimTypeProp.intValue)
            {
                case UiTweener.TweenAnimType.Rotate:
                case UiTweener.TweenAnimType.Scale:
                case UiTweener.TweenAnimType.MoveToAnchoredPosition:
                    endValueProp = property.FindPropertyRelative("m_endValue");
                    break;
                case UiTweener.TweenAnimType.Color:
                    endValueProp = property.FindPropertyRelative("m_endColor");
                    break;
                case UiTweener.TweenAnimType.FillAmount:
                    endValueProp = property.FindPropertyRelative("m_endAlpha");
                    break;
                case UiTweener.TweenAnimType.CanvasGroup:
                    endValueProp = property.FindPropertyRelative("m_endAlpha");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.PropertyField(rect, endValueProp, new GUIContent(endValueProp.displayName));
            rect.y += base.GetPropertyHeight(property, label);
            
            if ((UiTweener.TweenAnimType) tweenAnimTypeProp.intValue == UiTweener.TweenAnimType.CanvasGroup)
            {
                var changeInteractable = property.FindPropertyRelative("m_changeInteractable");
                EditorGUI.PropertyField(rect, changeInteractable, new GUIContent(changeInteractable.displayName));
                rect.y += base.GetPropertyHeight(property, label);
            }

            DrawProp(property, rect, "m_time");
            rect.y += base.GetPropertyHeight(property, label);
            
            DrawProp(property, rect, "m_delay");
            rect.y += base.GetPropertyHeight(property, label);

            var needLoop = property.FindPropertyRelative("m_needLoop");
            EditorGUI.PropertyField(rect, needLoop, new GUIContent(needLoop.displayName));
            rect.y += base.GetPropertyHeight(property, label);

            if (needLoop.boolValue)
            {
                DrawProp(property, rect, "m_loopType");
                rect.y += base.GetPropertyHeight(property, label);
            }

            DrawProp(property, rect, "m_animCurveType");
            rect.y += base.GetPropertyHeight(property, label);

            if (property.displayName.StartsWith("Element"))
            {
                EditorGUI.EndProperty();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawProp(SerializedProperty property, Rect rect, string mTime)
        {
            var prop = property.FindPropertyRelative(mTime);
            EditorGUI.PropertyField(rect, prop, new GUIContent(prop.displayName));
        }
    }
}