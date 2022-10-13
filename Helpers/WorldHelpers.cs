using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, "This helper load location to world")]
    public static partial class WorldHelpers
    {
        // private static bool sceneIsLoaded;
        public static async UniTask<SceneInstance> LoadLocationToWorld(this World world, AssetReference location)
        {
            var scene = await Addressables.LoadSceneAsync(location, LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        public static async UniTask LoadLocationToWorld(this World world, string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByName(sceneName);
            InitGameObjects(world, scene.GetRootGameObjects());
        }
        // public static async UniTask LoadLocationToWorld(this World world, string sceneName)
        // {
        //     sceneIsLoaded = false;
        //     SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        //     SceneManager.sceneLoaded += OnSceneLoaded;
        //
        //     await UniTask.WaitUntil(() => sceneIsLoaded);
        //     var scene = SceneManager.GetSceneByName(sceneName);
        //     InitGameObjects(world, scene.GetRootGameObjects());
        // }

        // private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        // {
            // SceneManager.sceneLoaded -= OnSceneLoaded;
            // sceneIsLoaded = true;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitGameObjects(World world, GameObject[] gameObjects)
        {
            var actorsList = new List<Actor>(16);

            foreach (var gameObject in gameObjects)
            {
                var actors = gameObject.GetComponentsInChildren<Actor>();

                foreach (var a in actors)
                {
                    if (a.IsInited)
                    {
                        HECSDebug.LogError("Actors on scene when we load it to world, should not be inited, remove init on start from them");
                        continue;
                    }

                    a.Init(world.Index, true);
                    actorsList.Add(a);
                }
            }

            foreach (var a in actorsList)
            {
                foreach (var s in a.GetAllSystems)
                {
                    if (s is IStartOnScene startOnScene)
                    {
                        startOnScene.StartOnScene();
                    }
                }
            }
        }
    }
}