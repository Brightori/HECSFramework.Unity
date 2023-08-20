using HECSFramework.Core;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "identifier", menuName = "Identifiers/Identifier")]
    public class IdentifierContainer : ScriptableObject, IIdentifier, IValidate
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