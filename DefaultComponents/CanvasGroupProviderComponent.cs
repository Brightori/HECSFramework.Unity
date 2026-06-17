using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.UI, "this component provide canvas group for ui features like healthbars")]
    public sealed class CanvasGroupProviderComponent : BaseProviderComponent<CanvasGroup>
    {
       
    }
}