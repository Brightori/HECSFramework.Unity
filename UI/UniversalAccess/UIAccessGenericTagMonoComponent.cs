using HECSFramework.Core;
using UnityEngine;

public enum UIAccessType { Image, Button, Text, UIAccess, CanvasGroup }

[Documentation(Doc.UI, Doc.HECS, "this component provide information for editor time auto additing needed ui access component")]
public class UIAccessGenericTagMonoComponent : MonoBehaviour
{
    public UIAccessType UIAccessType;
    public UIAccessIdentifier UIAccessIdentifier; 
}