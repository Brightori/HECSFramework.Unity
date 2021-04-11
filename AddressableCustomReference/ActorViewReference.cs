using HECSFramework.Unity;
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