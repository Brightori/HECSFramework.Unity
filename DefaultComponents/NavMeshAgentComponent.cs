using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace Components
{
    [Serializable, BluePrint]
    public sealed class NavMeshAgentComponent : BaseComponent, IHaveActor, IInitable, IDisposable
    {
        private NavMeshAgent navmeshAgent;
        public NavMeshAgent NavMeshAgent => navmeshAgent;

        public Actor Actor { get; set; }
     
        public void Init()
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