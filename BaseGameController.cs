using System;
using Commands;
using Components;
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

        private Entity gameLogic;
        private Entity player;
        private Entity uiManager;
        private Entity sceneManager;
        private Entity inputManager;

        private HECSList<World> waitForStart = new HECSList<World>();
        private HECSList<World> waitForLateStart = new HECSList<World>();

        private void Awake()
        {
            HECSDebug.Init(new HECSDebugUnitySide());

            entityManager = new EntityManager(worldCount);
            EntityManager.OnNewWorldAdded += NewWorldReact;

            updateSystem = EntityManager.Default.GlobalUpdateSystem;

            foreach (var w in EntityManager.Worlds)
                w?.GlobalUpdateSystem.InitCustomUpdate(this);


            gameLogic = Entity.Get("GameLogic");
            player = Entity.Get("Player");
            uiManager = Entity.Get("UiManager");
            sceneManager = Entity.Get("SceneManager");
            inputManager = Entity.Get("InputManager");
            BaseAwake();
            NetworkAwake();
        }

        private void OnApplicationQuit()
        {
            if (EntityManager.IsAlive)
            {
                var entity = EntityManager.Default.GetEntityFromPool("OnQuit");
                entity.AddComponent<OnApplicationQuitTagComponent>();
                entity.Init();
            }
        }

        private void NewWorldReact(World world)
        {
            world.GlobalUpdateSystem.InitCustomUpdate(this);

            if (updateSystem.IsGlobalStarted && world.IsInited)
                world.GlobalUpdateSystem.Start();
            else
                waitForStart.Add(world);

            if (world.GlobalUpdateSystem.IsLateStarted && world.IsInited)
                world.GlobalUpdateSystem.LateStart();
            {
                waitForLateStart.Add(world);
            }
        }

        partial void NetworkAwake();
        partial void InitNetWorkEntities();

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

            ThreadsInit();
            BaseStart();

            foreach (var w in EntityManager.Worlds)
                w?.GlobalUpdateSystem.Start();
        }

        private void ThreadsInit()
        {
            foreach (var w in EntityManager.Worlds)
            {
                if (w == null)
                    continue;

                foreach (var e in w.Entities)
                {
                    if (e.IsAlive && e.IsInited)
                    {
                        foreach (var s in e.Systems)
                        {
                            if (s is IOnThreadStartInit needed)
                            {
                                needed.OnThreadStartInit();
                            }
                        }
                    }
                }
            }
        }

        public void LateStart()
        {
            if (gameLogic.TryGetSystem(out IStartSystem startSystem))
                startSystem.StartGame();

            foreach (var w in EntityManager.Worlds)
                w?.GlobalUpdateSystem.LateStart();
        }

        private void Update()
        {
            var worlds = EntityManager.Worlds;

            for (int i = 0; i < waitForStart.Count; i++)
            {
                if (waitForStart.Data[i].IsInited)
                {
                    var world = waitForStart.Data[i];
                    world.GlobalUpdateSystem.Start();
                    EntityManager.Command(new WaitAndCallbackCommand { Timer = 0, CallBack = () => waitForStart.Remove(world) });
                }
            }

            for (int i = 0; i < waitForLateStart.Count; i++)
            {
                if (waitForLateStart.Data[i].IsInited)
                {
                    var world = waitForLateStart.Data[i];
                    world.GlobalUpdateSystem.LateStart();
                    EntityManager.Command(new WaitAndCallbackCommand { Timer = 0, CallBack = () => waitForLateStart.Remove(world) });
                }
            }

            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i]?.GlobalUpdateSystem.Update();
                worlds[i]?.GlobalUpdateSystem.UpdateDelta(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            var worlds = EntityManager.Worlds;
            var worldsCount = worlds.Length;
            for (int i = 0; i < worldsCount; i++)
            {
                worlds[i]?.GlobalUpdateSystem.LateUpdate();
                worlds[i]?.GlobalUpdateSystem.PreFinishUpdate?.Invoke();
                worlds[i]?.GlobalUpdateSystem.FinishUpdate?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            var worlds = EntityManager.Worlds;

            var worldsCount = worlds.Length;
            for (int i = 0; i < worldsCount; i++)
            {
                worlds[i]?.GlobalUpdateSystem.FixedUpdate();
            }
        }

        private void OnDisable()
        {
            entityManager?.Dispose();
        }

    }
}