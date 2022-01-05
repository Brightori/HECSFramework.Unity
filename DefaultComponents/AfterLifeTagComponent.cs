using HECSFramework.Core;
using System;
using Systems;

namespace Components
{
    [Serializable, Documentation(Doc.GameLogic, "Это тэг актора, которому еще надо отработать некоторое время после смерти, обрабатывается в юнити части системы " + nameof(DestroyEntityWorldSystem))]
    public class AfterLifeTagComponent : BaseComponent, IAfterLife
    {
    }
}