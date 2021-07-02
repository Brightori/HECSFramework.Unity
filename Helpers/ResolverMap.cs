using Components;
using HECSFramework.Unity;
using MessagePack.Resolvers;
using MessagePack;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HECSFramework.Core
{
    public partial class ResolversMap
    {
        static ResolversMap()
        {
            if (isMessagePackInited)
                return;

            if (!isMessagePackInited)
                StaticCompositeResolver.Instance.Register(StandardResolver.Instance, GeneratedResolver.Instance);

            isMessagePackInited = true;

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = option;
        }

        public async Task<IEntity> GetEntityFromResolver(EntityResolver entityResolver, bool needInitFromContainer, bool needForceAdd = false, int worldIndex = 0)
        {
            var unpack = new UnPackEntityResolver(entityResolver);
            var actorID = unpack.Components.FirstOrDefault(x => x is ActorContainerID containerID);

            if (actorID == null)
                return null;

            var container = actorID as ActorContainerID;

            IEntity entity;
            entity = new Entity((actorID as ActorContainerID).ID, worldIndex);

            if (needInitFromContainer)
            {
                var loaded = await Addressables.LoadAssetAsync<ScriptableObject>(container.ID).Task;
                var loadedContainer = loaded as EntityContainer;
                loadedContainer.Init(entity);
                entity.LoadEntityFromResolver(entityResolver, needForceAdd);
            }

            entity.SetGuid(entityResolver.Guid);
            return entity;
        }
    }
}
