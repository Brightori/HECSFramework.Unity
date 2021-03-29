using HECSFramework.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HECSFramework.Core
{
    public class UpdateModuleCustom : IRegisterUpdate<ICustomUpdatable>
    {
        private readonly IDictionary<ICustomUpdatable, Coroutine> customUpdatables = new Dictionary<ICustomUpdatable, Coroutine>(8);
        private readonly MonoBehaviour owner;
        
        public UpdateModuleCustom(MonoBehaviour monoBehaviour)
        {
            owner = monoBehaviour;
        }

        private static IEnumerator Coroutine(ICustomUpdatable updatable)
        {
            while (true)
            {
                yield return updatable.Interval;
                updatable.UpdateCustom();
            }
        }

        public void Register(ICustomUpdatable updatable, bool add)
        {
            if (add)
                customUpdatables[updatable] = owner.StartCoroutine(Coroutine(updatable));
            else
            {
                owner.StopCoroutine(customUpdatables[updatable]);
                customUpdatables.Remove(updatable);
            }
        }
    }
    
    public class CustomUpdateHandler : ICustomUpdatable
    {
        private readonly Action onUpdate;
        private readonly float interval;
        
        public YieldInstruction Interval { get; }
        public float Elapsed { get; private set; }

        public CustomUpdateHandler(float interval, Action onUpdate)
        {
            this.onUpdate = onUpdate;
            this.interval = interval;
            Interval = new WaitForSeconds(interval);
            Elapsed = 0;
        }

        public void UpdateCustom()
        {
            onUpdate();
            Elapsed += interval;
        }
    }
    
    public class CustomUpdateCounterHandler : ICustomUpdatable
    {
        private readonly Action onUpdate;
        public YieldInstruction Interval { get; }
        public int RoundsPassed { get; private set; }

        public CustomUpdateCounterHandler(float interval, Action onUpdate)
        {
            this.onUpdate = onUpdate;
            Interval = new WaitForSeconds(interval);
            RoundsPassed = 0;
        }

        public void UpdateCustom()
        {
            onUpdate();
            RoundsPassed += 1;
        }
    }
}