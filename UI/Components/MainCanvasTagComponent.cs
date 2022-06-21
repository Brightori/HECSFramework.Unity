using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;

namespace Components
{
    [Serializable, BluePrint]
    [Documentation("UI", "??? ??? ???????? ???????")]
    public class MainCanvasTagComponent : BaseComponent, IAfterEntityInit
    {
        public void AfterEntityInit()
        {
            Owner.World.Command(new CanvasReadyCommand());
        }
    }
}