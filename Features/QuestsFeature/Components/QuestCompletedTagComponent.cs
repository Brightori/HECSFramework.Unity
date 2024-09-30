using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.Quests, Doc.HECS, "This is how we mark quests that have already ended and for which we can force completion manually, this is necessary for cases when the player must call the completion of the quest and receive a reward")]
    public sealed class QuestCompletedTagComponent : BaseComponent
    {
    }
}