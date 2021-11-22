using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "identifier", menuName = "Identifiers/Identifier")]
    public class IdentifierContainer : ScriptableObject, IIdentifier
    {
        private int id; 

        public int Id 
        {
            get
            {
                if (id == 0)
                    id = IndexGenerator.GenerateIndex(name);
                return id;
            }
        }
    }
}