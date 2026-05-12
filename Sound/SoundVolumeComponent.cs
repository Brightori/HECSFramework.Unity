using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.Sound, "This holds volume settings for music and fx")]
    public sealed partial class SoundVolumeComponent : BaseComponent, IWorldSingleComponent
    {
        [Field(0)]
        [Range(0, 1)]
        public float MasterVolume = 1.0f;


        [Field(1)]
        [Range(0, 1)]
        public float MusicVolume = 0.6f;

        [Field(2)]
        [Range(0, 1)] 
        public float SoundVolume = 1.0f;
    }
}