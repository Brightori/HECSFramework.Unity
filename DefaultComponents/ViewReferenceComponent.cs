using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;

namespace Components
{
    [Serializable, BluePrint]
    public partial class ViewReferenceComponent : BaseComponent
    {
        public ActorViewReference ViewReference;
    }

    [Serializable]
    public class ActorViewReference : ComponentReference<Actor>
    {
        public ActorViewReference(string guid) : base(guid)
        {
        }
    }
}