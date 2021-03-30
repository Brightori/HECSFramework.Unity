using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    public class ActorInitModule
    {
        [SerializeField] private InitActorMode initActorMode = InitActorMode.Default;
        [SerializeField] private GuidGenerationRule guidRule = GuidGenerationRule.Default;

        [SerializeField] private string guidInspector = string.Empty;

        [SerializeField] private int worldIndex = 0;
        [SerializeField] private string iD;

        public InitActorMode InitActorMode => initActorMode;
        public int WorldIndex => worldIndex;

        public Guid Guid
        {
            get
            {
                if (string.IsNullOrEmpty(guidInspector))
                    guidInspector = Guid.NewGuid().ToString();

                return new Guid(guidInspector);
            }
        }

        public void Reset()
        {

        }
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