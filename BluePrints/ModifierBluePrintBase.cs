using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class ModifierBluePrintBase : ScriptableObject
    {
        public abstract IModifier GetModifier();
    }
}
