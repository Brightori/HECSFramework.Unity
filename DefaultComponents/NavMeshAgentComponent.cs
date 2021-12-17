using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace Components
{
    [Serializable, BluePrint]
    public class NavMeshAgentComponent : BaseComponent, IHaveActor, IInitable
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
            NavMeshAgent.SetDestination(destination);
            NavMeshAgent.isStopped = false;
        }
    }
}