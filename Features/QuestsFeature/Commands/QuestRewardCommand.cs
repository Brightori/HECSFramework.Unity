using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command to quests entity when we want activate reward funcs")]
    public struct QuestRewardCommand : ICommand
    {
    }
}