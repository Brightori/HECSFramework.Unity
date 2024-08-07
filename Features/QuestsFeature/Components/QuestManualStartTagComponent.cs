using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Quests, Doc.Tag, "quests marked with this tag are launched manually or through player input (via the UI for example) or through communication with an NPC")]
    public sealed class QuestManualStartTagComponent : BaseComponent
    {
    }
}