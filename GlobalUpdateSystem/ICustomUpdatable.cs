using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public interface ICustomUpdatable : IRegisterUpdatable
    {
        void UpdateCustom();
        YieldInstruction Interval { get; }
    }
}