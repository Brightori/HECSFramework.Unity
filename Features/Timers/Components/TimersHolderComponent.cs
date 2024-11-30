using System;
using System.Collections.Generic;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Timers, Doc.HECS, "The timers component provides the function of numerous additions of waiting or progress parameters, we add a value by ID, maximum value and wind it up with the global system every frame")]
    public sealed class TimersHolderComponent : BaseComponent
    {
        private Dictionary<int, float> timers = new Dictionary<int, float>(8);
        private Queue<int> removeQueue = new Queue<int>();

        public void AddTimer(int index, float value)
        {
            timers[index] = value;
        }

        public void RemoveTimer(int index)
        {
            timers.Remove(index);
        }

        public bool IsContainsTimer(int index)
        {
            return timers.ContainsKey(index);
        }

        public void UpdateAllTimers(float value)
        {
            foreach (var timer in timers)
            {
                var newValue = timer.Value - value;
                if (newValue < 0)
                    removeQueue.Enqueue(timer.Key);
                else
                    timers[timer.Key] = newValue;
            }
        }

        public void UpdateTimer(int index, float value)
        {
            var newValue = timers[index] - value;

            if (newValue < 0)
                timers.Remove(index);
            else
                timers[index] = newValue;
        }
    }
}

public static partial class Doc
{
    public const string Timers = "Timers";
}