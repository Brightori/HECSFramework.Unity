using HECSFramework.Core;
using TMPro;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial interface IActor : IEntity
    {
        bool TryGetComponent<T>(out T component, bool lookInChildsToo = false);
        bool TryGetComponents<T>(out T[] components);
        void SetWorld(World world = null);
        void InjectContainer(EntityContainer container, bool isAdditive = false);
        
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