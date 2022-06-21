using HECSFramework.Core;

namespace Commands
{
    /// <summary>
    /// это часть UI логики, здесь мы просим скрыть какой то элемент UI
    /// </summary>
    public struct HideUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
    }
}