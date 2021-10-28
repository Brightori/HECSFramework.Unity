using UnityEngine;

namespace HECSFramework.Rewards
{
    public abstract class RewardsBluePrintBase : ScriptableObject
    {
        public abstract IReward GetReward();
    }

    public abstract class RewardsBluePrint<RewardType> : RewardsBluePrintBase where RewardType : IReward
    {
        [SerializeField] RewardType Reward;

        public override IReward GetReward() => Reward;
    }
}