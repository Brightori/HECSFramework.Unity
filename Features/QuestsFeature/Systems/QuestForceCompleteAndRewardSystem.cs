using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.Quests, "this system on quest entity reacts on ForceCompleteQuestCommand and execute rewards")]
    public sealed class QuestForceCompleteAndRewardSystem : BaseSystem, IReactCommand<ForceCompleteQuestCommand>
    {
        [Required]
        public PredicatesComponent Predicates;

        [Required]
        public RewardsLocalHolderComponent RewardsLocal;

        public void CommandReact(ForceCompleteQuestCommand command)
        {
            if (!Predicates.IsReady(Owner))
                return;

            RewardsLocal.ExecuteRewards(new ExecuteReward { Owner = command.From, Target = command.To });
        }

        public override void InitSystem()
        {
        }
    }
}