using System.Threading.Tasks;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using NUnit.Framework;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayModeTests : OdinEditorWindow
{
    public EntityContainer EntityContainer;


    [MenuItem("HECS Options/Debug/PlayMode Tests")]
    public static void ShowPlayModeTests()
    {
        GetWindow<PlayModeTests>();
    }

    [Button]
    public void InitActorTest()
    {
        var check = new GameObject();
        var actor = check.GetOrAddComponent<Actor>();
        actor.Init();
        actor.Entity.AddComponent(new TestComponent());
        actor.Entity.Init();

        Debug.Assert(actor.Entity.TryGetComponent<ActorProviderComponent>(out var provider)
            && provider.Actor != null
            && actor.Entity.TryGetComponent<TestComponent>(out var test));
    }

    [Button]
    public async void DestroyActorTest()
    {
        var check = new GameObject();
        var actor = check.GetOrAddComponent<Actor>();
        actor.Init();
        actor.Entity.AddComponent(new TestComponent());
        actor.Entity.Init();

        var entity = actor.Entity;

        actor.HecsDestroy();

        await UniTask.DelayFrame(3);

        var checkComponent = actor.Entity.GetComponent<TestComponent>();

        Debug.Assert(actor == null && !entity.IsAlive
            && entity.Components.Count == 0
            && entity.Systems.Count == 0
            && checkComponent != null
            && !checkComponent.IsAlive);
    }

    [Button]
    public async void CreateFromContainerTest()
    {
        if (!CheckContainer())
            return;

        var actor = await EntityContainer.GetActor(needLoadContainer: false);
        actor.Init(initEntity: false);
        EntityContainer.Init(actor.Entity);
        actor.Entity.Init();
    }

    [Button]
    public void SpawnAndRemove500Actors()
    {
        if (!CheckContainer())
            return;

        for (int i = 0; i < 500; i++)
        {
            CreateFromContainerTest();
        }

        UniTask.Delay(2000);
        var filter = EntityManager.Default.GetFilter<CheckTwoComponent>();
        filter.ForceUpdateFilter();

        foreach (var e in filter)
        {
            e.AsActor().HecsDestroy();
        }
    }

    [Button]
    public void Spawn5000Actors()
    {
        if (!CheckContainer())
            return;

        for (int i = 0; i < 5000; i++)
        {
            CreateFromContainerTest();
        }
    }


    [Button]
    public void Remove5000Actors()
    {
        var filter = EntityManager.Default.GetFilter<CheckTwoComponent>();
        filter.ForceUpdateFilter();

        foreach (var e in filter)
        {
            e.AsActor().HecsDestroy();
        }
    }

    private bool CheckContainer()
    {
        if (EntityContainer != null)
            return true;

        ShowNotification(new GUIContent("u must add container"));
        return false;
    }
}