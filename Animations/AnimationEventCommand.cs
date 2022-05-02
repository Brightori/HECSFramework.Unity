using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.Animation, "We send this command from animator event to Actor")]
    public struct AnimationEventCommand : ICommand
    {
        public int Id;
    }

    [Documentation(Doc.HECS, Doc.Animation, "We send this command from animator event to Actor, this command contains state id")]
    public struct EventStateAnimationCommand : ICommand
    {
        public int AnimationId;
        public int StateId;
    }
}