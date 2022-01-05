using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable, Documentation(Doc.GameLogic, "Этим компонентом мы отмечаем актора который уже прожил AfterLife")]
    public class AfterLifeCompleteTagComponent : BaseComponent, IAfterLife
    {
    }
}