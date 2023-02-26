using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.UI, "")]
    public struct ScreenDragCommand : IGlobalCommand
    {
        public Vector2 Delta { get; internal set; }
    }
}