using HECSFramework.Core;
using HECSFramework.Unity;

namespace Commands
{
    [Documentation(Doc.GameLogic, "Invoked when we detect that actor disposed and we need release asset references and poolable views")]
    public struct ActorViewDisposedCommand : IGlobalCommand
    {
        public Actor Actor;
    }
}