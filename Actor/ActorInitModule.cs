using HECSFramework.Core;
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
        [SerializeField] private string iD;
        private readonly IEntity entity;

        public string ID => iD;
        public InitActorMode InitActorMode => initActorMode;
        public int WorldIndex => worldIndex;
        public GuidGenerationRule GuidRule => guidRule;

        public ActorInitModule(IEntity entity)
        {
            this.entity = entity;

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
        InitOnStart = 1
    }

    public enum GuidGenerationRule
    {
        Default = 0,
        PersistentUnique = 1
    }
}