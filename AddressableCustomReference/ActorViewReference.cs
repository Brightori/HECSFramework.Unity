using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;

namespace Helpers
{
    [Serializable]
    public class ActorViewReference : ComponentReference<Actor>
    {
        public ActorViewReference(string guid) : base(guid)
        {
        }
    }
}