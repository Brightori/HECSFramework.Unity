using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class SystemBaseBluePrint : ScriptableObject, ISystemContainer
    {
        #region Editor

        private string Name => name.Replace("BluePrint", "");
        #endregion

        public abstract ISystem GetSystem { get; }

        private int TypeIndex = 0;

        public ISystem GetSystemInstance()
        {
            var t = Instantiate(this);
            return t.GetSystem;
        }

        public int GetTypeIndex()
        {
            if (TypeIndex == 0)
                TypeIndex = IndexGenerator.GetIndexForType(GetSystem.GetType());

            return TypeIndex;
        }
    }
}
