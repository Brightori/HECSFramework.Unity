using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.UI, "Command about zooming")]
    public struct ScreenZoomCommand : IGlobalCommand
    {
        public float Value;
    }
}