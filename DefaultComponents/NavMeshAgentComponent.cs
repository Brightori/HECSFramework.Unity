using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace Components
{
    [Serializable, BluePrint]
    public sealed class NavMeshAgentComponent : BaseComponent, IHaveActor, IDisposable
    {
        private NavMeshAgent navmeshAgent;
        public NavMeshAgent NavMeshAgent => navmeshAgent;

        public Actor Actor { get; set; }

        /// <summary>
        /// here we think owner have proportional scale and use 
        /// </summary>
        public float Radius => navmeshAgent.radius * Actor.Entity.GetTransform().localScale.x;

        public override void Init()
        {
            Actor.TryGetComponent(out navmeshAgent);
        }

        public void SetDestination(Vector3 destination)
        {
            NavMeshAgent.isStopped = false;
            NavMeshAgent.SetDestination(destination);
        }

        public void Stop()
        {
            NavMeshAgent.isStopped = true;
        }
     
        public void Dispose()
        {
            navmeshAgent = null;
        }
    }
}