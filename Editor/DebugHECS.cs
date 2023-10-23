using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebugHECS : OdinEditorWindow
{
    [SerializeField, ShowInInspector, Range(0, 99)] private int worldIndex = 0;

    [ShowInInspector, ReadOnly]
    public int EntitiesCount;

    [ShowInInspector, ReadOnly]
    public int FreeIndeces;

    [ShowInInspector, Searchable]
    [ListDrawerSettings(ShowFoldout = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 100)]
    public List<DrawEntity> Entities = new List<DrawEntity>(256);

    [MenuItem("HECS Options/Debug HECS", priority = 2)]
    public static void ShowDebugHECSWindow()
    {
        GetWindow<DebugHECS>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RedrawWindow();
    }

    private void Update()
    {
        if (!EntityManager.IsAlive)
            return;

        if (EntityManager.Default == null)
            return;

        //RedrawWindow();
        Repaint();
    }
    [PropertyOrder(-1)]
    [Button(ButtonHeight = 50)]
    public void RedrawWindow()
    {
        Entities.Clear();

        if (!EntityManager.IsAlive)
            return;

        if (worldIndex >= EntityManager.Worlds.Length)
            return;

        if (EntityManager.Worlds[worldIndex] == null)
            return;

        EntitiesCount = EntityManager.Worlds[worldIndex].Entities.Length;

        var freeIndeces = ReflectionHelpers.GetPrivateFieldValue<Stack<int>>(EntityManager.Worlds[worldIndex], "freeIndicesForStandartEntities");

        FreeIndeces = freeIndeces.Count;

        foreach (var e in EntityManager.Worlds[worldIndex].Entities)
        {
            if (e == null || !e.IsAlive || !e.IsInited)
                continue;

            var drawEntity = new DrawEntity();

            drawEntity.ID = e.ID;
            drawEntity.Index = e.Index;
            drawEntity.Guid = e.GUID.ToString();
            drawEntity.ContainerID = e.ContainerID;
            drawEntity.IsAlive = e.IsAlive;
            drawEntity.IsPaused = e.IsPaused;

            foreach (var c in e.GetComponentsByType<IComponent>())
            {
                if (c == null)
                    continue;

                drawEntity.drawComponents.Add(new DrawComponent { Component = c, Name = c.GetType().Name });
            }

            foreach (var s in e.Systems)
            {
                drawEntity.drawSystems.Add(new DrawSystem { Name = s.GetType().Name });
            }

            Entities.Add(drawEntity);
        }
    }
}

[Serializable, HideLabel]
public class DrawEntity
{
    [FoldoutGroup("$ID", false), ReadOnly]
    public string ID;
       
    [FoldoutGroup("$ID", false), ReadOnly]
    public int Index;

    [FoldoutGroup("$ID"), ReadOnly]
    public string ContainerID;

    [FoldoutGroup("$ID"), ReadOnly]
    public string Guid;

    [FoldoutGroup("$ID"), ReadOnly]
    public bool IsAlive;

    [FoldoutGroup("$ID"), ReadOnly]
    public bool IsPaused;


    [ShowInInspector, FoldoutGroup("$ID"), ListDrawerSettings(ShowFoldout = false, IsReadOnly = true), LabelText("Components")]
    public List<DrawComponent> drawComponents = new List<DrawComponent>();

    [ShowInInspector, FoldoutGroup("$ID"), ListDrawerSettings(ShowFoldout = false, IsReadOnly = true), LabelText("Systems")]
    public List<DrawSystem> drawSystems = new List<DrawSystem>();
}

[Serializable]
public class DrawSystem
{
    [HideLabel, ReadOnly]
    public string Name;
}

[Serializable]
public class DrawComponent
{
    [ReadOnly, BoxGroup, HideLabel]
    public string Name;
    [ShowInInspector, HideLabel, HideReferenceObjectPicker, BoxGroup, ReadOnly] public IComponent Component;
}