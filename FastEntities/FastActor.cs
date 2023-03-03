using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace HECSFramework.Unity
{
    public class FastActor : MonoBehaviour
    {
        private ushort fastEntityIndex;
        private World world;

        public ref FastEntity FastEntity => ref world.FastEntities[fastEntityIndex];

        [NonSerialized]
        public Transform TransformCache;
        
        private FastComponentMonoProvider[] fastComponentMonoProviders;

        public virtual void Awake()
        {
            TransformCache = transform;
            fastComponentMonoProviders = GetComponents<FastComponentMonoProvider>();
        }

        public void AddEntity(FastEntity fastEntity)
        {
            world = fastEntity.World;
            fastEntityIndex = fastEntity.Index;
            FastEntity.GetOrAddComponent(new FastActorProvider { FastActor = this });

            foreach (var fcp in fastComponentMonoProviders)
                fcp.AddComponent(fastEntity);

            OnEntityAdded();
        }

        protected virtual void OnEntityAdded()
        {
        }
    }
}

namespace Components
{
    public struct FastActorProvider : IFastComponent
    {
        public FastActor FastActor;
    }
}