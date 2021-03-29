using HECSFramework.Unity;
using UnityEngine;

namespace HECSFramework.Core
{
    public partial class GlobalUpdateSystem
    {
        private UpdateModuleCustom updateModuleCustom;

        public void InitCustomUpdate(MonoBehaviour monoBehaviour)
        {
            updateModuleCustom = new UpdateModuleCustom(monoBehaviour);
        }

        partial void UnityFuncs(IRegisterUpdatable registerUpdatable, bool add)
        {
            if (registerUpdatable is ICustomUpdatable customUpdatable)
            {
                updateModuleCustom.Register(customUpdatable, add);
            }
        }
    }
}