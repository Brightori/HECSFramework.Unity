using HECSFramework.Core;
using System;
using Systems;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.Audio, "command to play sound")]
    public struct PlayAudioCommand : IGlobalCommand
    {
        public Guid Owner;
        public AudioClip Clip;
        public SoundType AudioType;
        public bool IsRepeatable;
    }

    [Documentation(Doc.Audio, "command to stop sound")]
    public struct StopAudioCommand : IGlobalCommand
    {
        public Guid Owner;
        public AudioClip Clip;
    }
}