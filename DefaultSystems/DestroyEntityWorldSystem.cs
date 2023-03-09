using Components;
using HECSFramework.Core;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public sealed partial class DestroyEntityWorldSystem
    {
        partial void ProcessActor(Entity entity)
        {
            if (entity.ContainsMask<PoolableTagComponent>())
                entity.GetComponent<ActorProviderComponent>().Actor.RemoveActorToPool();
            else
                entity.GetComponent<ActorProviderComponent>().Actor.HecsDestroy();
        }
    }
}
