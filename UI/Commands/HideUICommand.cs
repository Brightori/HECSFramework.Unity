using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.UI, "Its core ui command, we send it for asking hide ui widget")]
    public struct HideUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
    }
}