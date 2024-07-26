using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.Quests, "we send this command from global entities for update status of active quests")]
    public struct CheckQuestsGlobalCommand: IGlobalCommand
    {
    }
}