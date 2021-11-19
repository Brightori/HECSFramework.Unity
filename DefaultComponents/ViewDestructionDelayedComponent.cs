using System;
using HECSFramework.Core;
using HECSFramework.Documentation;

namespace Components
{
    [Documentation(Doc.Tag, "Энтити помеченные этим тегом при уничтожении сохраняют свою вьюху в течение указанного промежутка времени")]
    [Serializable]
    public class ViewDestructionDelayedComponent : BaseComponent
    {
        public float Delay;
    }
}