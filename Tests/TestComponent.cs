using System;
using HECSFramework.Core;
using Strategies;

namespace Components
{
    [Serializable][Documentation(Doc.Test, "we use this component on tests purpose")]
    public sealed class TestComponent : ModifiableFloatCounterComponent
    {
        public Strategy Strategy;

        public override int Id => 10;
        public override float SetupValue => 11;
    }
}