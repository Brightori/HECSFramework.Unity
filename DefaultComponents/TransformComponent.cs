using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable]
    public sealed partial class TransformComponent : BaseComponent
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public Vector3 Forward => Rotation * Vector3.forward;
        public Vector3 Right => Rotation * Vector3.right;

        public void Translate(Vector3 direction)
        {
            Position += direction;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
            IsDirty = true;
        }

        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
            IsDirty = true;
        }
    }
}