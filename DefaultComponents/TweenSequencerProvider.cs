using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.UI, Doc.Provider, "this component provide access to TweenSequencer")]
    public sealed class TweenSequencerProviderComponent : BaseProviderComponent<TweenSequencer>
    {
       
    }
}