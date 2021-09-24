﻿using HECSFramework.Core;
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

        private EntityManager entityManager;
        private GlobalUpdateSystem updateSystem;

        private IEntity gameLogic;
        private IEntity player;
        private IEntity uiManager;
        private IEntity sceneManager;

        private void Awake()
        {
            HECSDebug.Init(new HECSDebugUnitySide());

            entityManager = new EntityManager(worldCount);
            updateSystem = EntityManager.Default.GlobalUpdateSystem;

            foreach (var w in EntityManager.Worlds)
                w.GlobalUpdateSystem.InitCustomUpdate(this);

            gameLogic = new Entity("GameLogic");
            player = new Entity("Player");
            uiManager = new Entity("UiManager");
            sceneManager = new Entity("SceneManager");
            BaseAwake();
            NetworkAwake();
        }

        partial void NetworkAwake();
        partial void InitNetWorkEntities();

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
            updateSystem.Update();
        }

        private void LateUpdate()
        {
            updateSystem.LateUpdate();
        }

        private void FixedUpdate()
        {
            updateSystem.FixedUpdate();
        }

        private void OnDisable()
        {
            entityManager?.Dispose();
        }
    }
}