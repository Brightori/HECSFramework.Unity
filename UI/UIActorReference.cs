using System;
using HECSFramework.Unity.Helpers;

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