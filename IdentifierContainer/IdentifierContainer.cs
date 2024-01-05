using HECSFramework.Core;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "identifier", menuName = "Identifiers/Identifier")]
    public class IdentifierContainer : ScriptableObject, IIdentifier, IValidate, IEquatable<IdentifierContainer>
    {
        [SerializeField, ReadOnly] private int id; 

        public int Id 
        {
            get
            {
                if (id == 0)
                    id = IndexGenerator.GenerateIndex(name);
                return id;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierContainer container &&
                   Id == container.Id;
        }

        public bool Equals(IdentifierContainer other)
        {
            if (other == null) 
                return false;

            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id);
        }

        public bool IsValid()
        {
            OnValidate();
            return true;
        }

        [Button]
        private void OnValidate()
        {
            id = IndexGenerator.GenerateIndex(name);
        }
    }

    public static class IdentifierContainerHelper
    {
        public static void InitId(this IdentifierContainer identifierContainer, ref int Id)
        {
            if (identifierContainer != null && Id == 0)
            {
                Id = identifierContainer.Id;
                return;
            }

            Id = 0;
        }
    }
}