using HECSFramework.Core;
using System;
using Systems;
using UnityEngine;

namespace Commands
{
    [Documentation(Doc.Audio, "command to play sound")]
    public struct PlaySoundCommand : IGlobalCommand
    {
        public Guid Owner;
        public AudioClip Clip;
        public SoundType AudioType;
        public bool IsRepeatable;
    }

    [Documentation(Doc.Audio, "command to stop sound")]
    public struct StopSoundCommand : IGlobalCommand
    {
        public Guid Owner;
        public AudioClip Clip;
    }

    [Documentation(Doc.Audio, "this command change volume on listeners")]
    public struct ChangeVolumeCommand : IGlobalCommand
    {
        public VolumeType VolumeType;
        public float Volume;
    }

    public enum VolumeType
    {
        Master = 0,
        Sound = 1,
        Music = 2,
    }
}