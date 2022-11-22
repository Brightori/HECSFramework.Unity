﻿using System;
using HECSFramework.Core;


namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Visual, Doc.Animation, Doc.Tag, "we mark entity when view ready for interaction")]
    public sealed class ViewReadyTagComponent : BaseComponent
    {
    }
}