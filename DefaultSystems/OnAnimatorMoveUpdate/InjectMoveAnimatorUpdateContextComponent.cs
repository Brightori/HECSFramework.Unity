using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Animation, Doc.OnAnimatorMoveUpdate, Doc.HECS, "its part of providing move animator functionality")]
    public sealed class InjectMoveAnimatorUpdateContextComponent : BaseComponent
    {
        public OnAnimatorMoveUpdateProviderMonoComponent OnAnimatorMoveUpdateProviderMonoComponent;
    }
}