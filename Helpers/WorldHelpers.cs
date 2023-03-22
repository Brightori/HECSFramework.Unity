using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, "This helper load location to World")]
    public static partial class WorldHelpers
    {
        public static async Task<SceneInstance> LoadLocationToWorld(this World world, AssetReference location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }
        public static async Task<GameObject> LoadPrefabToWorld(this World world, AssetReference location)
        {
            var obj = await location.InstantiateAsync().Task;
            InitGameObject(world, obj);
            return obj;
        }

        public static async Task<SceneInstance> LoadLocationToWorld(this World world, string location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        public static async Task<GameObject> LoadPrefabToWorld(this World world, string location)
        {
            var obj = await Addressables.InstantiateAsync(location).Task;
            InitGameObject(world, obj);
            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitGameObject(World world, GameObject gameObject)
        {
            List<Actor> actorsList = new List<Actor>(16);
            var actors = gameObject.GetComponentsInChildren<Actor>();
            foreach (var a in actors)
            {
                if (a.IsAlive())
                {
                    HECSDebug.LogError(
                        "Actors on scene when we load it to World, should not be inited, remove init on start from them");
                    continue;
                }

                a.Init(world, true, true);
                actorsList.Add(a);
            }

            foreach (var a in actorsList)
            {
                foreach (var s in a.Entity.Systems)
                {
                    if (s is IStartOnScene startOnScene)
                    {
                        startOnScene.StartOnScene();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitGameObjects(World world, GameObject[] gameObjects)
        {
            var actorsList = new List<Actor>(16);

            foreach (var gameObject in gameObjects)
            {
                var actors = gameObject.GetComponentsInChildren<Actor>();

                foreach (var a in actors)
                {
                    if (a.IsAlive())
                    {
                        HECSDebug.LogError("Actors on scene when we load it to World, should not be inited, remove init on start from them");
                        continue;
                    }

                    a.Init(world, true, true);
                    actorsList.Add(a);
                }
            }

            foreach (var a in actorsList)
            {
                foreach (var s in a.Entity.Systems)
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