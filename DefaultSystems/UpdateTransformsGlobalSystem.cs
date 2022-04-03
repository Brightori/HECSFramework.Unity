using Components;
using HECSFramework.Core;
using Unity.Burst;
using UnityEditor.UIElements;
using UnityEngine;

namespace Systems
{
    [Documentation(Doc.HECS, "Unity system for assign values to Unity.Transforms")]
    public sealed class UpdateTransformsGlobalSystem : BaseSystem, IReactEntity, IUpdatable
    {
        private ConcurrencyList<TransformComponent> transformComponents = new ConcurrencyList<TransformComponent>();
        [Required] public TestEntity testEntity;

        private ConcurrencyList<IEntity> filter;
        private ConcurrencyList<GameObject> gameObjects = new ConcurrencyList<GameObject>();

        public void EntityReact(IEntity entity, bool isAdded)
        {
            if (isAdded)
            {
                if (entity.ContainsMask(ref HMasks.TransformComponent))
                {
                    transformComponents.Add(entity.GetTransformComponent());
                }
            }
        }

        public override void InitSystem()
        {
            for (int i = 0; i < 10000; i++)
            {
              var t =   MonoBehaviour.Instantiate(testEntity.prefab);
            }
        }

        public void UpdateLocal()
        {
            foreach (var g in gameObjects)
            {
                var t = g.transform.position;
            }
        }
    }
}
