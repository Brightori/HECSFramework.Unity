using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Documentation(Doc.HECS, Doc.UI, "here we hold states of ui for back to previous ui state functionality")]
    public sealed class UIStackComponent : BaseComponent
    {
        public Stack<List<int>> UIStack = new();
    }
}