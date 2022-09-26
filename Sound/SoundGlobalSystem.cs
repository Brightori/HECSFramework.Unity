﻿using Commands;
using Components;
using DG.Tweening;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.Audio, Doc.Global, Doc.HECS, "Default sound solution for HECS, its all about 2d sound on this moment")]
    public class SoundGlobalSystem : BaseSystem, ISoundGlobalSystem, IHaveActor, IReactEntity,
        IReactGlobalCommand<PlaySoundCommand>,
        IReactGlobalCommand<StopSoundCommand>
    {
        [SerializeField] private AudioMixerGroup audioMixerGroup;

        [Required]
        public SoundVolumeComponent volumeComponent;

        private List<SoundSourceContainer> SoundSources = new List<SoundSourceContainer>(32);

        public IActor Actor { get; set; }

        public override void InitSystem()
        {
            Actor.TryGetComponents<AudioSource>(out var soundSources);

            if (soundSources != null)
            {
                foreach (var s in soundSources)
                    SoundSources.Add(new SoundSourceContainer(s));
            }
            else
                Debug.LogAssertion("нет аудио сорсов у актора глобальной аудио системы");

            SetupSoundGroup();

            if (Actor.TryGetComponent(out DefaultMusicMonoComponent defaultMusicMonoComponent))
            {
                CommandGlobalReact(new PlaySoundCommand { AudioType = SoundType.Music, IsRepeatable = true, Clip = defaultMusicMonoComponent.AudioClip, Owner = this.Owner.GUID });
            }
        }

        void SetupSoundGroup()
        {
            foreach (var go in SoundSources)
                go.AudioSource.loop = false;
        }

        private void PlaySound(PlaySoundCommand playAudioCommand)
        {
            if (playAudioCommand.Clip == null)
                return;

            if (playAudioCommand.AudioType == SoundType.Music)
            {
                PlayMusic(playAudioCommand);
                return;
            }

            for (int i = 0; i < SoundSources.Count; i++)
            {
                SoundSourceContainer soundSource = SoundSources[i];

                if (!soundSource.IsBusy)
                {
                    var source = soundSource.AudioSource;
                    soundSource.IsRepeatable = playAudioCommand.IsRepeatable;
                    soundSource.IsBusy = true;
                    soundSource.Owner = playAudioCommand.Owner;
                    soundSource.AudioType = playAudioCommand.AudioType;

                    source.volume = volumeComponent.SoundVolume;
                    source.clip = playAudioCommand.Clip;
                    source.loop = playAudioCommand.IsRepeatable;
                    source.Play();

                    SoundSources[i] = soundSource;
                    return;
                }
            }
        }

        private void PlayMusic(PlaySoundCommand playAudioCommand)
        {
            for (int i = 0; i < SoundSources.Count; i++)
            {
                SoundSourceContainer soundSource = SoundSources[i];

                if (soundSource.AudioType == SoundType.Music)
                {
                    Stop(i);
                    break;
                }
            }

            if (playAudioCommand.IsRepeatable)
            {
                StopFromSource(playAudioCommand.Owner, playAudioCommand.Clip);
            }

            for (int i = 0; i < SoundSources.Count; i++)
            {
                SoundSourceContainer soundSource = SoundSources[i];

                if (!soundSource.IsBusy)
                {
                    var source = soundSource.AudioSource;
                    soundSource.IsRepeatable = playAudioCommand.IsRepeatable;
                    soundSource.IsBusy = true;
                    soundSource.Owner = playAudioCommand.Owner;
                    soundSource.AudioType = playAudioCommand.AudioType;
                    source.clip = playAudioCommand.Clip;
                    source.loop = playAudioCommand.IsRepeatable;
                    source.Play();
                    source.DOFade(volumeComponent.MusicVolume, 1);

                    SoundSources[i] = soundSource;
                    return;
                }
            }
        }

        public void StopAllFromSource(Guid owner)
        {
            for (int i = 0; i < SoundSources.Count; i++)
            {
                var soundSource = SoundSources[i];
                var index = i;

                if (soundSource.Owner == owner)
                {
                    Stop(index);
                }
            }
        }

        public void StopFromSource(Guid owner, AudioClip sound)
        {
            for (int i = 0; i < SoundSources.Count; i++)
            {
                var soundSource = SoundSources[i];
                var index = i;

                if (soundSource.Owner == owner && soundSource.AudioSource.clip == sound)
                {
                    Stop(index);
                    return;
                }
            }
        }

        public void EntityReact(IEntity entity, bool isAdded)
        {
            if (!isAdded)
                StopAllFromSource(entity.GUID);
        }

        private void Stop(int indexOfSource)
        {
            SoundSources[indexOfSource].AudioSource.DOFade(0, 1).OnComplete(() => { SoundSources[indexOfSource].Stop(); });
        }

        public void CommandGlobalReact(PlaySoundCommand command)
        {
            PlaySound(command);
        }

        public void CommandGlobalReact(StopSoundCommand command)
        {
            StopFromSource(command.Owner, command.Clip);
        }
    }

    public struct SoundSourceContainer
    {
        public AudioSource AudioSource;
        public Guid Owner;
        public SoundType AudioType;

        public bool IsRepeatable;
        private bool isBusy;

        public bool IsBusy { get => isBusy && AudioSource.isPlaying; set => isBusy = value; }

        public SoundSourceContainer(AudioSource audioSource) : this()
        {
            AudioSource = audioSource;
        }

        public void Stop()
        {
            AudioSource.Stop();
            AudioSource.clip = null;
            Owner = Guid.Empty;
            isBusy = false;
            IsRepeatable = false;
            AudioType = SoundType.Default;
            AudioSource.loop = false;
        }

        public override bool Equals(object obj)
        {
            return obj is SoundSourceContainer container &&
                   EqualityComparer<AudioSource>.Default.Equals(AudioSource, container.AudioSource) &&
                   Owner.Equals(container.Owner) &&
                   AudioType == container.AudioType;
        }

        public override int GetHashCode()
        {
            int hashCode = 1844660361;
            hashCode = hashCode * -1521134295 + EqualityComparer<AudioSource>.Default.GetHashCode(AudioSource);
            hashCode = hashCode * -1521134295 + Owner.GetHashCode();
            hashCode = hashCode * -1521134295 + AudioType.GetHashCode();
            return hashCode;
        }
    }

    public enum SoundType
    {
        Default = 0,
        FX = 1,
        Music = 2,
    }

    public interface ISoundGlobalSystem : ISystem { }
}