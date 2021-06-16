using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using UnityEngine;

namespace Components
{
    ///if u need add some project dependency to this class
    ///add partial part of this class to ur project, dont change this class
    ///if u need new functionality like add InetworkComponent interface - add them to part class

    [Serializable, BluePrint]
    public partial class TransformComponent : BaseComponent, ITransformComponent, IInitable, IHaveActor
    {
        private LazyMonoBehComponent<Transform> lazyComponent;
        public Vector3 GetPosition => Transform.position;
        public Transform Transform => lazyComponent.GetComponent();

        public IActor Actor { get; set; }

        public void Init()
        {
            lazyComponent = new LazyMonoBehComponent<Transform>(Owner);
        }

        public void SetPosition(Vector3 position)
        {
            Transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            Transform.rotation = rotation;
        }
    }

    public partial interface ITransformComponent : IComponent
    {
        Transform Transform { get; }
        Vector3 GetPosition { get; }
        void SetPosition(Vector3 position);
        void SetRotation(Quaternion rotation);
    }
}