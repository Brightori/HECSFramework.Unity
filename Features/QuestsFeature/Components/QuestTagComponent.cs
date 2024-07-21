using System;
using HECSFramework.Core;

namespace Components
{
    [Feature("Quest")]
    [RequiredAtContainer(typeof(NameComponent), typeof(PredicatesComponent))]
    [Serializable][Documentation(Doc.Quests, "we mark quests containers|entitis by this component")]
    public sealed class QuestTagComponent : BaseComponent
    {
    }
}