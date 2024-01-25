using System;
using System.Collections.Generic;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class MultipleContainerWindow : OdinEditorWindow
{
    public List<EntityContainer> Containers;

    [ValueDropdown(nameof(GetComponents))]
    public Type ComponentType;

    [ValueDropdown(nameof(GetSystems))]
    public Type SystemType;

    public PresetContainer ApplyContainer;

    [Button]
    public void ApplyPreset()
    {
        foreach (var container in Containers) 
        {
            ApplyContainer.CopyOrReplaceComponents(container);
        }
    }

    [Button]
    public void AddComponent()
    {
        if (ComponentType == null)
            return;

        var bp = new BluePrintsProvider();

        var componentNode = new ComponentBluePrintNode(ComponentType.Name, bp.Components[ComponentType], Containers);
        componentNode.AddBluePrint();
    }

    [Button]
    public void AddSystem()
    {
        if (SystemType == null)
            return;

        var bp = new BluePrintsProvider();

        var componentNode = new SystemBluePrintNode(SystemType.Name, bp.Systems[SystemType], Containers);
        componentNode.AddBluePrint();
    }

    private IEnumerable<Type> GetComponents()
    {
        return new BluePrintsProvider().Components.Keys;
    }

    private IEnumerable<Type> GetSystems()
    {
        return new BluePrintsProvider().Systems.Keys;
    }

    [MenuItem("HECS Options/Helpers/MultipleContainerHelper")]
    public static void GetCreateUIHelperWindow()
    {
        GetWindow<MultipleContainerWindow>();
    }
}
