using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;

namespace Components
{
    [Serializable]
    public class UIViewReference : ComponentReference<UIActor>
    {
        public UIViewReference(string guid) : base(guid)
        {
        }
    }
}