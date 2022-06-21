using HECSFramework.Unity.Helpers;
using System;

namespace HECSFramework.Unity
{
    [Serializable]
    public class UIActorReference : ComponentReference<UIActor>
    {
        public UIActorReference(string guid) : base(guid)
        {
        }
    }
}