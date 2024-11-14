using HECSFramework.Core;
using UnityEngine;

public enum UIAccessType { Image = 0, Button = 1, Text = 2, UIAccess = 3, CanvasGroup = 4, RectTransform = 5, Generic = 6 }

[Documentation(Doc.UI, Doc.HECS, "this component provide information for editor time auto additing needed ui access component")]
public class UIAccessGenericTagMonoComponent : MonoBehaviour
{
    public UIAccessType UIAccessType;
    public UIAccessIdentifier UIAccessIdentifier;
}