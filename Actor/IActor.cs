using HECSFramework.Core;
using TMPro;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial interface IActor : IEntity, IHECSEnable, IHECSDisable
    {
        bool TryGetComponent<T>(out T component, bool lookInChildsToo = false);
        bool TryGetComponents<T>(out T[] components);
        void SetWorld(World world = null);
        void InjectContainer(EntityContainer container, bool isAdditive = false);
        void InjectContainer(EntityContainer container, bool isAdditive = false, params IComponent[] components);
        Entity ReplaceEntity(Entity entity);

        ActorContainer ActorContainer { get; }
        GameObject GameObject { get; }
    }

    public interface IHaveActor : INotCore
    {
        IActor Actor { get; set; }
    }

    public interface IInitAferView
    {
        void InitAferView();
    }
}