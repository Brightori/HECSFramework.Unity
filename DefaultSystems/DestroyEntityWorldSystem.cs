using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System.Threading.Tasks;
using UnityEngine;

namespace Systems
{
    public partial class DestroyEntityWorldSystem : BaseSystem
    {
        private HECSMask ViewDestructionDelayedComponentMask = HMasks.GetMask<ViewDestructionDelayedComponent>();
        private HECSMask PoolableTagComponentMask = HMasks.GetMask<PoolableTagComponent>();
        private HECSMask AfterLifeTagComponentMask = HMasks.GetMask<AfterLifeTagComponent>();
        private HECSMask AfterLifeCompleteTagComponentMask = HMasks.GetMask<AfterLifeCompleteTagComponent>();

        public async void ProcessDeathOfActor(Actor actor)
        {
            if (actor.ContainsMask(ref AfterLifeTagComponentMask) && !actor.ContainsMask(ref AfterLifeCompleteTagComponentMask))
            {
                AfterLife(actor);
                return;
            }

            if (!actor.ContainsMask(ref AfterLifeTagComponentMask) && !actor.ContainsMask(ref PoolableTagComponentMask)
                && actor.TryGetHecsComponent(ViewDestructionDelayedComponentMask, out ViewDestructionDelayedComponent delayedComponent))
            {
                var delay = delayedComponent.Delay;

                actor.EntityDestroy();

                if (delay > 0) await Task.Delay(delay.ToMilliseconds());
                MonoBehaviour.Destroy(actor.gameObject);
                return;
            }

            RemoveActor(actor);
        }

        private void AfterLife(Actor actor)
        {
            Owner.World.RegisterEntity(actor, false);
            var components = actor.GetAllComponents;
            var systems = actor.GetAllSystems.ToArray();

            var count = components.Length;
            var systemsCount = systems.Length;

            for (int i = 0; i < count; i++)
            {
                if (components[i] == null || components[i] is IAfterLife) continue;
                actor.RemoveHecsComponent(components[i]);
            }

            for (int i = 0; i < systemsCount; i++)
            {
                if (systems[i] == null || systems[i] is IAfterLife) continue;
                actor.RemoveHecsSystem(systems[i]);
            }

            if (actor.TryGetComponents(out IAfterLifeAction[] actions))
            {
                foreach (var a in actions)
                    a.Action();
            }

            actor.AddHecsComponent(new AfterLifeCompleteTagComponent(), actor);
        }

        private void RemoveActor(Actor actor)
        {
            if (actor.ContainsMask(ref PoolableTagComponentMask))
            {
                Owner.World.GetSingleSystem<PoolingSystem>().Release(actor);
                actor.EntityDestroy();
            }
            else
            {
                if (actor == null || actor.gameObject == null)
                    return;

                actor.EntityDestroy();
                MonoBehaviour.Destroy(actor.gameObject);
            }
        }
    }
}
