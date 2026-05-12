using System;
using System.Collections.Generic;
using Commands;
using Components;
using DG.Tweening;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.Audio;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.Audio, Doc.Global, Doc.HECS, "Default sound solution for HECS, its all about 2d sound on this moment")]
    public sealed partial class SoundGlobalSystem : BaseSystem, ISoundGlobalSystem, IHaveActor, IUpdatable,
        IReactGlobalCommand<PlaySoundCommand>,
        IReactGlobalCommand<StopSoundCommand>,
        IReactGlobalCommand<UpdateSoundOptionsCommand>,
        IReactGlobalCommand<ChangeVolumeCommand>
    {
        [SerializeField] private AudioMixerGroup audioMixerGroup;

        public SoundVolumeComponent volumeComponent;

        private HECSList<SoundSourceContainer> soundSources = new HECSList<SoundSourceContainer>(32);

        private bool isDirty;

        public Actor Actor { get; set; }

        public override void InitSystem()
        {
            volumeComponent = EntityManager.GetSingleComponent<SoundVolumeComponent>();

            Actor.TryGetComponents<AudioSource>(out var soundSources);

            if (soundSources != null)
            {
                foreach (var s in soundSources)
                    this.soundSources.Add(new SoundSourceContainer(s));
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
            foreach (var go in soundSources)
                go.AudioSource.loop = false;
        }

        private void PlaySound(PlaySoundCommand playAudioCommand)
        {
            if (!volumeComponent.IsSoundOn)
                return;

            if (playAudioCommand.Clip == null)
                return;

            if (playAudioCommand.AudioType == SoundType.Music)
            {
                PlayMusic(playAudioCommand);
                return;
            }

            for (int i = 0; i < soundSources.Count; i++)
            {
                SoundSourceContainer soundSource = soundSources[i];

                if (!soundSource.IsBusy)
                {
                    var source = soundSource.AudioSource;
                    soundSource.IsRepeatable = playAudioCommand.IsRepeatable;
                    soundSource.IsBusy = true;
                    soundSource.Owner = playAudioCommand.Owner;
                    soundSource.AudioType = playAudioCommand.AudioType;

                    source.volume = volumeComponent.SoundVolume * volumeComponent.MasterVolume;
                    source.clip = playAudioCommand.Clip;
                    source.loop = playAudioCommand.IsRepeatable;
                    source.Play();

                    soundSources[i] = soundSource;
                    return;
                }
            }
        }

        private void PlayMusic(PlaySoundCommand playAudioCommand)
        {
            for (int i = 0; i < soundSources.Count; i++)
            {
                ref SoundSourceContainer soundSource = ref soundSources[i];

                if (soundSource.AudioType == SoundType.Music)
                {
                    soundSource.StopFade();
                }
            }

            if (playAudioCommand.IsRepeatable)
            {
                StopFromSource(playAudioCommand.Owner, playAudioCommand.Clip);
            }

            for (int i = 0; i < soundSources.Count; i++)
            {
                ref SoundSourceContainer soundSource = ref soundSources[i];

                if (!soundSource.IsBusy && !soundSource.OnFade)
                {
                    var source = soundSource.AudioSource;
                    soundSource.IsRepeatable = playAudioCommand.IsRepeatable;
                    soundSource.IsBusy = true;
                    soundSource.Owner = playAudioCommand.Owner;
                    soundSource.AudioType = playAudioCommand.AudioType;
                    source.clip = playAudioCommand.Clip;
                    source.loop = playAudioCommand.IsRepeatable;
                    source.volume = volumeComponent.MusicVolume * volumeComponent.MasterVolume;
                    source.Play();
                    isDirty = true;
                    return;
                }
            }
        }

        public void StopAllFromSource(Guid owner)
        {
            for (int i = 0; i < soundSources.Count; i++)
            {
                soundSources.Data[i].StopFade();
            }
        }

        public void StopFromSource(Guid owner, AudioClip sound)
        {
            for (int i = 0; i < soundSources.Count; i++)
            {
                ref var soundSource = ref soundSources[i];

                if (soundSource.Owner == owner && soundSource.AudioSource.clip == sound)
                {
                    soundSource.StopFade();
                    return;
                }
            }
        }


        public void CommandGlobalReact(PlaySoundCommand command)
        {
            if (IsDisposed)
                return;

            PlaySound(command);
        }

        public void CommandGlobalReact(StopSoundCommand command)
        {
            if (IsDisposed)
                return;

            StopFromSource(command.Owner, command.Clip);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!EntityManager.IsAlive)
                return;

            foreach (var s in soundSources.ToArray())
            {
                if (s.AudioTween != null)
                    DOTween.Kill(s.AudioTween);

                s.AudioSource.Stop();
            }

            DOTween.KillAll();
        }

        public void CommandGlobalReact(UpdateSoundOptionsCommand command)
        {
            volumeComponent.IsSoundOn = command.IsSoundOn;
        }

        public void CommandGlobalReact(ChangeVolumeCommand command)
        {
            switch (command.VolumeType)
            {
                case VolumeType.Master:
                    volumeComponent.MasterVolume = command.Volume;
                    break;
                case VolumeType.Sound:
                    volumeComponent.SoundVolume = command.Volume;
                    break;
                case VolumeType.Music:
                    volumeComponent.MusicVolume = command.Volume;
                    break;
            }

            foreach (var s in soundSources)
            {
                s.UpdateVolume(volumeComponent);
            }
        }

        public void UpdateLocal()
        {
            if (!isDirty)
                return;

            isDirty = false;

            for (var i = 0; i < soundSources.Count; i++)
            {
                var s = soundSources[i];

                if (s.OnFade)
                {
                    s.Progress -= Time.deltaTime;
                    s.AudioSource.volume = Mathf.Lerp(s.AudioSource.volume, 0f, s.Progress);

                    if (s.Progress < 0f)
                    {
                        s.Stop();
                    }
                    else
                    {
                        isDirty = true;
                    }
                }
            }
        }
    }

    public class SoundSourceContainer
    {
        public AudioSource AudioSource;
        public Guid Owner;
        public SoundType AudioType;
        public Tween AudioTween;

        public bool IsRepeatable;
        public bool OnFade;
        public float Progress;

        private bool isBusy;

        public bool IsBusy { get => isBusy && AudioSource.isPlaying; set => isBusy = value; }

        public SoundSourceContainer(AudioSource audioSource)
        {
            AudioSource = audioSource;
        }

        public void StopFade()
        {
            Progress = 1;
            OnFade = true;
        }

        public void Stop()
        {
            AudioSource.Stop();
            AudioSource.clip = null;
            Owner = Guid.Empty;
            isBusy = false;
            IsRepeatable = false;
            AudioType = SoundType.Sound;
            AudioSource.loop = false;
            OnFade = false;
        }

        public override bool Equals(object obj)
        {
            return obj is SoundSourceContainer container &&
                   EqualityComparer<AudioSource>.Default.Equals(AudioSource, container.AudioSource) &&
                   Owner.Equals(container.Owner) &&
                   AudioType == container.AudioType;
        }

        public void UpdateVolume(SoundVolumeComponent volumeComponent)
        {
            switch (AudioType)
            {
                case SoundType.Sound:
                    AudioSource.volume = volumeComponent.SoundVolume * volumeComponent.MasterVolume;
                    break;
                case SoundType.Music:
                    AudioSource.volume = volumeComponent.MusicVolume * volumeComponent.MasterVolume;
                    break;
            }
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
        Sound = 0,
        Music = 1,
    }


    public interface ISoundGlobalSystem : ISystem { }
}