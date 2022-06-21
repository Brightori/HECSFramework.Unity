using HECSFramework.Core;

namespace Commands
{
    /// <summary>
    /// ��� ����� UI ������, ����� �� ������ ������ ����� �� ������� UI
    /// </summary>
    public struct HideUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
    }
}