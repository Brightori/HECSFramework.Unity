using HECSFramework.Core;
using TMPro;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial interface IActor  
    {
        bool TryGetComponent<T>(out T component, bool lookInChildsToo = false);
        bool TryGetComponents<T>(out T[] components);

        IEntity Entity { get; }

        ActorContainer ActorContainer { get; }
        GameObject GameObject { get; }

        public void Init(World world = null);
        void Command<T>(T command) where T : struct, ICommand;
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