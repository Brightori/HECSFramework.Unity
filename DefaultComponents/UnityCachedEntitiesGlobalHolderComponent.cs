using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    public sealed partial class CachedEntitiesGlobalHolderComponent
    {
        public Entity AddEntityToCache(EntityContainer container)
        {
            var entity = container.GetEntity();
            entity.Init();

            cachedEntities.Add(container.ContainerIndex, entity);
            return entity;
        }

        public Entity GetOrAddEntityToCache(EntityContainer container)
        {
            if (cachedEntities.TryGetValue(container.ContainerIndex, out var entity))
            {
                return entity;
            }

            return AddEntityToCache(container);
        }
    }
}
