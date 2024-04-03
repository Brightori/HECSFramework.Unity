using System;
using UnityEngine;

namespace Playstrom.Core.Ui
{
    /// <summary>
    /// you cannot use custom enums for UnityEventAction
    /// </summary>
    public abstract class UiBaseAnimUtil : MonoBehaviour
    {
        private void ChangePlayState(Action<int> action, bool isInc)
        {
            action?.Invoke(isInc ? +1 : -1);
        }

        protected void OnPlayEnded(Action<int> playState)
        {
            ChangePlayState(playState, false);
        }
        
        public void Play()
        {
            Play(null);
        }

        public void PlayByIndex(int index)
        {
            PlayByIndex(index, null);
        }

        public void PlayRandom()
        {
            PlayRandom(null);
        }
        

        public virtual void Play(Action<int> playState)
        {
            ChangePlayState(playState, true);
        }

        public virtual void PlayByIndex(int index, Action<int> playState)
        {
            ChangePlayState(playState, true);
        }

        public virtual void PlayRandom(Action<int> playState)
        {
            ChangePlayState(playState, true);
        }

        public virtual void PlayRandomRange(int index1, int index2, Action<int> playState)
        {
            ChangePlayState(playState, true);
        }

        public virtual void PlayQueue(Action<int> playState)
        {
            ChangePlayState(playState, true);
        }

        public virtual void Stop()
        {
        }

        public virtual void StopByIndex(int index)
        {
        }

        public virtual void Restart()
        {
        }
    }
}