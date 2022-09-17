﻿using System;
using Commands;
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

        private ConcurrencyList<World> worlds;
        private ConcurrencyList<World> waitForStart = new ConcurrencyList<World>();
        private ConcurrencyList<World> waitForLateStart = new ConcurrencyList<World>();

        private void Awake()
        {
            HECSDebug.Init(new HECSDebugUnitySide());

            entityManager = new EntityManager(worldCount);
            EntityManager.OnNewWorldAdded += NewWorldReact;
            
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

            foreach (var w in EntityManager.Worlds)
                w.GlobalUpdateSystem.Start();
        }

        public void LateStart()
        {
            if (gameLogic.TryGetSystem(out IStartSystem startSystem))
                startSystem.StartGame();

            foreach (var w in EntityManager.Worlds)
                w.GlobalUpdateSystem.LateStart();
        }

        private void Update()
        {
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

            for (int i = 0; i < worlds.Count; i++)
            {
                worlds.Data[i].GlobalUpdateSystem.Update();
                worlds.Data[i].GlobalUpdateSystem.UpdateDelta(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            var worldsCount = worlds.Count;
            for (int i = 0; i < worldsCount; i++)
            {
                worlds.Data[i].GlobalUpdateSystem.LateUpdate();
                worlds.Data[i].GlobalUpdateSystem.FinishUpdate?.Invoke();
            }
        }

        private void FixedUpdate()
        {
            var worldsCount = worlds.Count;
            for (int i = 0; i < worldsCount; i++)
            {
                worlds.Data[i].GlobalUpdateSystem.FixedUpdate();
            }
        }

        private void OnDisable()
        {
            entityManager?.Dispose();
        }

    }
}