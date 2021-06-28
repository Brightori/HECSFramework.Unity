using Components;
using HECSFramework.Core;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HECSFramework.Unity.Helpers
{
    public static class SerializeExtentions
    {
        public static async Task<IActor> GetActorFromResolver(this EntityResolver entityResolver, bool needForceAdd = true, int worldIndex = 0)
        {
            var unpack = new UnPackEntityResolver(entityResolver);
            var actorID = unpack.Components.FirstOrDefault(x => x is ActorContainerID containerID);

            if (actorID == null)
                return null;

            var container = actorID as ActorContainerID;

            var loaded = await Addressables.LoadAssetAsync<ScriptableObject>(container.ID).Task;
            var loadedContainer = loaded as EntityContainer;
            var actor = await loadedContainer.GetActor();
            actor.LoadEntityFromResolver(entityResolver, needForceAdd);

            return actor;
        }
    }
}