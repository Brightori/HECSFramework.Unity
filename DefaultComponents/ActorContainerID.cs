using HECSFramework.Core;
using System;

namespace Components
{
    ///if u need add some project dependency to this class
    ///add partial part of this class to ur project, dont change this class
    ///if u need new functionality like add InetworkComponent interface - add them to part class

    [Serializable]
    public partial class ActorContainerID : BaseComponent, IActorContainerID
    {
        private string id;

        public string ID
        {
            get => id;
            set => id = value;
        }
    }

    public partial interface IActorContainerID : IComponent 
    {
        string ID { get; }
    }
}