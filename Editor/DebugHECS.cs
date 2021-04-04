using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebugHECS : OdinEditorWindow
{
    [UnityEngine.SerializeField, ShowInInspector, Range(0, 99)] private int worldIndex = 0;

    [ShowInInspector, Searchable]
    [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 100)]
    public List<DrawEntity> Entities = new List<DrawEntity>(16);

    [MenuItem("HECS Options/Debug HECS", priority = 0)]
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

        if (worldIndex < EntityManager.Worlds.Length && Entities.Count != EntityManager.Worlds[worldIndex].EntitiesCount)
            RedrawWindow();
        Repaint();
    }

    [Button]
    public void RedrawWindow()
    {
        Entities.Clear();

        if (!EntityManager.IsAlive)
            return;

        if (worldIndex >= EntityManager.Worlds.Length)
            return;

        foreach (var e in EntityManager.Worlds[worldIndex].Entities)
        {
            if (e == null)
                continue;

            var drawEntity = new DrawEntity();

            drawEntity.ID = e.ID;
            drawEntity.Guid = e.GUID.ToString();
            drawEntity.ContainerID = e.ContainerID;

            foreach (var c in e.GetAllComponents)
            {
                if (c == null)
                    continue;

                drawEntity.drawComponents.Add(new DrawComponent { Component = c, Name = c.GetType().Name });
            }

            foreach (var s in e.GetAllSystems)
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

    [FoldoutGroup("$ID"), ReadOnly]
    public string ContainerID;

    [FoldoutGroup("$ID"), ReadOnly]
    public string Guid;

    [ShowInInspector, FoldoutGroup("$ID"), ListDrawerSettings(Expanded = false, IsReadOnly = true), LabelText("Components")]
    public List<DrawComponent> drawComponents = new List<DrawComponent>();

    [ShowInInspector, FoldoutGroup("$ID"), ListDrawerSettings(Expanded = false, IsReadOnly = true), LabelText("Systems")]
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