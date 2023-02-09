using HECSFramework.Core;
using HECSFramework.Unity;
using Strategies;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.NONE, "")]
    public sealed class TestComponent : BaseComponent
    {
        public Strategy Strategy;
    }
}