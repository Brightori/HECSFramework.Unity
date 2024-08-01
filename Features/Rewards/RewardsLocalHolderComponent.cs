using System;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;

namespace Components
{
    [Serializable][Documentation(Doc.Rewards, "some entities can have various rewards containers, and we create entities from them, and provide execute rewards api")]
    public sealed partial class RewardsLocalHolderComponent : BaseComponent
    {
        [EntityContainerDropDown(nameof(RewardTagComponent))]
        public EntityContainer[] Rewards;

        private HECSList<Entity> rewards;

        public override void Init()
        {
            rewards = new HECSList<Entity>(Rewards.Length);

            foreach (var reward in Rewards) 
            { 
                rewards.Add(reward.GetEntity(Owner.World).Init());
            }
        }

        public void ExecuteRewards(ExecuteReward executeReward)
        {
            foreach (var r in rewards)
            {
                r.Command(executeReward);
            }
        }
    }
}