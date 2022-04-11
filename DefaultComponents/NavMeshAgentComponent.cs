using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace Components
{
    [Serializable, BluePrint]
    public sealed class NavMeshAgentComponent : BaseComponent, IHaveActor, IInitable, IDisposable
    {
        private LazyMonoBehComponent<NavMeshAgent> navmeshAgent;
        public NavMeshAgent NavMeshAgent => navmeshAgent.GetComponent();

        public IActor Actor { get; set; }
     
        public void Init()
        {
            navmeshAgent = new LazyMonoBehComponent<NavMeshAgent>(Owner);
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
            navmeshAgent.Dispose();
        }
    }
}