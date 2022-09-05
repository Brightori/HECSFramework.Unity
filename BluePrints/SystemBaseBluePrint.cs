using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class SystemBaseBluePrint : ScriptableObject, ISystemContainer
    {
        #region Editor

        private string Name => name.Replace("BluePrint", "");
        #endregion

        public abstract ISystem GetSystem { get; }

        public ISystem GetSystemInstance()
        {
            var t = Instantiate(this);
            return t.GetSystem;
        }

    }
}
