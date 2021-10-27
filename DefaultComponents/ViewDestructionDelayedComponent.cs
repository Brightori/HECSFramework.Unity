using HECSFramework.Core;
using HECSFramework.Documentation;
using System;

namespace Components
{
    [Documentation(Doc.Tag, "Энтити помеченные этим тегом при уничтожении сохраняют свою вьюху в течение указанного промежутка времени")]
    [Serializable]
    public class ViewDestructionDelayedComponent : BaseComponent
    {
        public float Delay;
    }
}