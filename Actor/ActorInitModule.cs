using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    public class ActorInitModule : IInitModule
    {
        [SerializeField] private InitActorMode initActorMode = InitActorMode.Default;
        [SerializeField] private GuidGenerationRule guidRule = GuidGenerationRule.Default;

        [SerializeField, ReadOnly] private string guid = string.Empty;

        [SerializeField] private int worldIndex = 0;
        [SerializeField, ReadOnly] private string iD;
        private Actor entity;

        public string ID => iD;
        public InitActorMode InitActorMode => initActorMode;
        public int WorldIndex => worldIndex;
        public GuidGenerationRule GuidRule => guidRule;

        public void InitModule(Actor actor)
        {
            if (string.IsNullOrEmpty(iD))
                iD = actor.GameObject.name;
        }

        public Guid Guid
        {
            get
            {
                switch (guidRule)
                {
                    case GuidGenerationRule.Default:
                        guid = Guid.NewGuid().ToString();
                        break;
                    case GuidGenerationRule.PersistentUnique:
                        if (string.IsNullOrEmpty(guid))
                            SetGuid(Guid.NewGuid());
                        break;
                }

                return new Guid(guid);
            }
        }

        public void SetGuid(Guid guid)
        {
            this.guid = guid.ToString();
        }

        public void SetID(string id)
        {
            iD = id;
        }
    }

    public interface IInitModule
    {
        Guid Guid { get; }
        string ID { get; }

        void SetID(string id);
    }

    public enum InitActorMode
    {
        Default = 0,
        InitOnStart = 1,
        InitOnAwake = 3,
        TryToLoadOrInitOnStart = 2,
    }

    public enum GuidGenerationRule
    {
        Default = 0,
        PersistentUnique = 1, 
    }
}