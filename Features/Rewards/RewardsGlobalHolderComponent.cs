using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Rewards, "base holder of all rewards")]
    public sealed class RewardsGlobalHolderComponent : BaseContainerHolderComponent<RewardTagComponent>, IWorldSingleComponent
    {
    }
}