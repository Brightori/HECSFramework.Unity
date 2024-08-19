using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class AsyncActionBluePrint : ScriptableObject
    {
        public abstract IAsyncAction GetAction();
    }
}
