using HECSFramework.Core;
using Systems;
using UnityEngine;

namespace HECSFramework.Unity
{
    /// <summary>
    /// its main part of partial GameController
    /// DONT CHANGE IT
    /// use another part to make project dependencies changes
    /// </summary>
    [DefaultExecutionOrder(-50000)]
    public abstract class BaseGameController : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int worldCount = 1;

        [SerializeField] private ActorContainer playerContainer = default;
        [SerializeField] private ActorContainer gameLogicContainer = default;
        [SerializeField] private ActorContainer uiManagerContainer = default;
        [SerializeField] private ActorContainer sceneManagerContainer = default;

        private EntityManager entityManager;

        private IEntity gameLogic;
        private IEntity player;
        private IEntity uiManager;
        private IEntity sceneManager;

        private void Awake()
        {
            entityManager = new EntityManager(worldCount);

            gameLogic = new Entity("GameLogic");
            player = new Entity("Player");
            uiManager = new Entity("UiManager");
            sceneManager = new Entity("SceneManager");
            BaseAwake();
        }

        private void InitEntities()
        {
            playerContainer?.Init(player);
            gameLogicContainer?.Init(gameLogic);
            uiManagerContainer?.Init(uiManager);
            sceneManagerContainer?.Init(sceneManager);

            player.Init();
            uiManager.Init();
            sceneManager.Init();
            gameLogic.Init();

            player.GenerateGuid();
            player.Init();
        }

        public abstract void BaseAwake();
        public abstract void BaseStart();

        private void Start()
        {
            InitEntities();
            BaseStart();

            if (gameLogic.TryGetSystem(out IStartSystem startSystem))
                startSystem.StartGame();
        }

        private void OnDisable()
        {
            entityManager?.Dispose();
        }
    }
}