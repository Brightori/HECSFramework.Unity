using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial interface IActor
    {
        bool TryGetComponent<T>(out T component, bool lookInChildsToo = false);
        bool TryGetComponents<T>(out T[] components);

        Entity Entity { get; }

        ActorContainer ActorContainer { get; }
        GameObject GameObject { get; }

        public void Init(World world = null, bool initEntity = false, bool initWithContainer = false);
        void Command<T>(T command) where T : struct, ICommand;

        Entity InjectContainer(EntityContainer container, World world, bool isAdditive = false);

        /// <summary>
        /// Destroy actor and dispose entity
        /// </summary>
        void HecsDestroy();

        /// <summary>
        /// we process this case by entity
        /// </summary>
        void RemoveActorToPool();
    }
}

namespace HECSFramework.Unity
{
    public interface IHaveActor : INotCore
    {
        IActor Actor { get; set; }
    }
}

namespace HECSFramework.Core
{
    public interface IInitAferView
    {
        void InitAferView();
    }
}