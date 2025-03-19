using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Abilities, Doc.HECS, Doc.Character, "We contains here layer mask for diff interactions")]
    public sealed class LayerInfoComponent : BaseComponent
    {
        public LayerMask LayerMask;
    }
}