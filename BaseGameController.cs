using HECSFramework.Core;
using Systems;
using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(-50000), RequireComponent(typeof(LateStartProvider))]
    public abstract partial class BaseGameController : MonoBehaviour
    {
        [SerializeField, Range(1, 99)] private int worldCount = 1;

        [SerializeField] private ActorContainer playerContainer = default;
        [SerializeField] private ActorContainer gameLogicContainer = default;
        [SerializeField] private ActorContainer uiManagerContainer = default;
        [SerializeField] private ActorContainer sceneManagerContainer = default;
        [SerializeField] private ActorContainer inputManagerContainer = default;

        private EntityManager entityManager;
        private GlobalUpdateSystem updateSystem;

        private IEntity gameLogic;
        private IEntity player;
        private IEntity uiManager;
        private IEntity sceneManager;
        private IEntity inputManager;

        private World[] worlds;

        private void Awake()
        {
            HECSDebug.Init(new HECSDebugUnitySide());

            entityManager = new EntityManager(worldCount);
            updateSystem = EntityManager.Default.GlobalUpdateSystem;

            foreach (var w in EntityManager.Worlds)
                w.GlobalUpdateSystem.InitCustomUpdate(this);

            worlds = EntityManager.Worlds;

            gameLogic = new Entity("GameLogic");
            player = new Entity("Player");
            uiManager = new Entity("UiManager");
            sceneManager = new Entity("SceneManager");
            inputManager = new Entity("InputManager");
            BaseAwake();
            NetworkAwake();
            StrategiesInit();
        }

        partial void NetworkAwake();
        partial void InitNetWorkEntities();
        partial void StrategiesInit();

        private void InitEntities()
        {
            playerContainer?.Init(player);
            gameLogicContainer?.Init(gameLogic);
            uiManagerContainer?.Init(uiManager);
            sceneManagerContainer?.Init(sceneManager);
            inputManagerContainer?.Init(inputManager);

            player.Init();
            uiManager.Init();
            sceneManager.Init();
            gameLogic.Init();
            player.GenerateGuid();
            inputManager.Init();
        }

        public abstract void BaseAwake();
        public abstract void BaseStart();

        protected virtual void Start()
        {
            InitEntities();
            InitNetWorkEntities();
            BaseStart();
            updateSystem.Start();
        }

        public void LateStart()
        {
            if (gameLogic.TryGetSystem(out IStartSystem startSystem))
                startSystem.StartGame();

            updateSystem.LateStart();
        }

        private void Update()
        {
            var worldsCount = worlds.Length;
            for (int i = 0; i < worldsCount; i++)
            {
                worlds[i].GlobalUpdateSystem.Update();
                worlds[i].GlobalUpdateSystem.UpdateDelta(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            var worldsCount = worlds.Length;
            for (int i = 0; i < worldsCount; i++)
            {
                updateSystem.LateUpdate();
                updateSystem.FinishUpdate?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            var worldsCount = worlds.Length;
            for (int i = 0; i < worldsCount; i++)
            {
                updateSystem.FixedUpdate();
            }
        }

        private void OnDisable()
        {
            entityManager?.Dispose();
        }
    }
}