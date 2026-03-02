using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[Documentation(Doc.Editor, Doc.Containers, Doc.Config, "this window show and edit containers by rules (tags and masks for needed components) ")]
public class ContainerCustomEditorWindow : OdinMenuEditorWindow
{
    private static SOProvider<EntityContainer> sOProvider = new();
    private static ContainersFinderConfig containersConfig;

    static ContainerCustomEditorWindow()
    {
        ContainersFinderConfig.OpenEditor += OpenWindow;
        ContainersFinderConfigDrawler.OpenEditor += OpenWindow;
    }

    private static void OpenWindow(ContainersFinderConfig config)
    {
        containersConfig = config;
        GetWindow<ContainerCustomEditorWindow>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var collection = sOProvider.GetCollection()
        .Where(x => x is not PresetContainer && containersConfig.ContainersFilter.All(p => x.ContainsComponent(p))
            && !x.ContainsComponent(ComponentProvider<IgnoreReferenceContainerTagComponent>.TypeIndex, true))
        .ToList();

        var tree = new OdinMenuTree();
        tree.Config.DrawSearchToolbar = true;

        foreach(var ec in collection)
        {
            tree.Add(ec.name, new EntityContainerDrawler(ec, containersConfig));
        }

        return tree;
    }
}

[Serializable]
public class EntityContainerDrawler
{
    [HorizontalGroup("Header")]
    [ShowInInspector, HideLabel]
    public EntityContainer Container;

    [ShowInInspector, ListDrawerSettings(ShowFoldout = false, DefaultExpandedState = true, HideAddButton = true, HideRemoveButton = true)]
    public List<ComponentDrawler> components;

    public EntityContainerDrawler(EntityContainer container, ContainersFinderConfig config)
    {
        Container = container;
        var containerComponents = container.GetComponents<IComponent>().ToList();
        components = new List<ComponentDrawler>(config.ShowingComponents.Length);

        foreach (var c in containerComponents)
        {
            if (c == null)
                continue;

            for (int i = 0; i < config.ShowingComponents.Length; i++)
            {
                if (c.GetTypeHashCode != config.ShowingComponents[i])
                    continue;

                components.Add(new ComponentDrawler { ComponentID = c.GetTypeHashCode, Component = c, Name = c.GetType().Name });
                break;
            }
        }
    }

    [HorizontalGroup("Header", Width = 80)]
    [Button("Save", ButtonHeight = 25)]
    private void SaveChanges()
    {
        var containerComponents = Container.GetComponents<IComponent>().ToList();

        for(int i = 0; i < containerComponents.Count; i++)
        {
            for(int j = 0; j < components.Count; j++)
            {
                if (components[j].ComponentID == containerComponents[i].GetTypeHashCode)
                {
                    containerComponents[i] = components[j].Component;
                    break;
                }
            }
        }

        EditorUtility.SetDirty(Container);
        AssetDatabase.SaveAssets();
    }
}

[Serializable]
public class ComponentDrawler
{
    [HideInInspector]
    public int ComponentID;
    [BoxGroup, HideLabel]
    public string Name;
    [ShowInInspector, HideLabel, HideReferenceObjectPicker, BoxGroup] public IComponent Component;
}
