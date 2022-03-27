using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    public partial class TransformComponent : BaseComponent
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public void Translate(Vector3 direction)
        {
            Position += direction;
        }
    }
}