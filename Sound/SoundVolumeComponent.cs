using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Sound, "This holds volume settings for music and fx")]
    public sealed partial class SoundVolumeComponent : BaseComponent, IWorldSingleComponent
    {
        [Range(0, 1)] public float MusicVolume = 0.6f;
        [Range(0, 1)] public float SoundVolume = 1.0f;
    }
}