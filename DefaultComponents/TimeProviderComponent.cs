using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.GameLogic, Doc.Client, "this component provide time from world for client")]
    public sealed class TimeProviderComponent : BaseComponent, IWorldSingleComponent, ITimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}