using System;
using BluePrints.Identifiers;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Helpers, "this is mono component for tagging some unique position on the gameobject")]
    public sealed class PositionMonoComponent : MonoBehaviour, IHavePosition 
    {
        public PositionIdentifier PositionIdentifier;

        private Transform cache;
        public Vector3 GetPosition => cache.position;
        
        public bool IsAlive { get; private set; } = true;


        private void Awake()
        {
            cache = GetComponent<Transform>();
        }


        private void OnDestroy()
        {
            IsAlive = false;
        }
    }
}