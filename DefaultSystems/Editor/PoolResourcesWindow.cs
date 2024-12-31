using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Systems;
using UnityEditor;
using UnityEngine;

public class PoolResourcesWindow : OdinEditorWindow
{
    public int World;
    public DrawPool[] PoolResources = new DrawPool[0];

    [MenuItem("HECS Options/Helpers/Pool Resource Window")]
    public static void ShowPoolResourcesWindow()
    {
        var window = GetWindow<PoolResourcesWindow>();
        window.Redraw();
    }

    [Button]
    public void Redraw()
    {
        if (!EntityManager.IsAlive)
        {
            Debug.LogError("this works only in playmode");
            return;
        }

        var poolingSystem = EntityManager.Worlds[World].GetSingleSystem<PoolingSystem>();

        var pools = ReflectionHelpers.GetPrivateFieldValue<Dictionary<string, HECSPool>>(poolingSystem, "pools");

        var list = new List<DrawPool>(64);

        foreach (var pool in pools)
        {
            list.Add(new DrawPool
            {
                MainPrfb = ReflectionHelpers.GetPrivateFieldValue<AssetContainer<GameObject>>(pool.Value, "container").CurrentObject,
                MaxCount = ReflectionHelpers.GetPrivateFieldValue<int>(pool.Value, "maxCount"),
                CurrentViews = ReflectionHelpers.GetPrivateFieldValue<Queue<GameObject>>(pool.Value, "queue").ToArray(),
            });
        }

        PoolResources = list.ToArray();
    }
}

[Serializable]
public class DrawPool
{
    public GameObject MainPrfb;
    public GameObject[] CurrentViews;
    public int MaxCount;
}