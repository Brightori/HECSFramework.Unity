using System;
using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[Serializable]
[Documentation(Doc.Debug, Doc.HECS, "RuntimeFilterWindow")]
public sealed class RuntimeFilterWindow : OdinEditorWindow
{
    [ValueDropdown(nameof(GetAllowedTypes))]
    public Type[] With = new Type[0];

    [ValueDropdown(nameof(GetAllowedTypes))]
    public Type[] Without = new Type[0];

    public int WorldIndex = 0;

    public List<Entity> entities = new List<Entity>(6);

    [MenuItem("HECS Options/Runtime Filter", priority = 2)]
    public static void ShowRuntimeFilterWindow()
    {
        GetWindow<RuntimeFilterWindow>();
    }

    [Button]
    public void ShowEntities()
    {
        entities.Clear();

        var filterWith = new Filter();
        var filterWithout = new Filter();

        foreach (var t in With) 
        {
            filterWith.Add(IndexGenerator.GetIndexForType(t));
        }

        foreach (var t in Without)
        {
            filterWithout.Add(IndexGenerator.GetIndexForType(t));
        }

        var world = EntityManager.Worlds[WorldIndex];

        var filter = world.GetFilter(filterWith, filterWithout);

        filter.ForceUpdateFilter();

        foreach (var e in filter)
        {
            entities.Add(e);
        }
    }

    private static IEnumerable<Type> GetAllowedTypes()
    {
        return typeof(IComponent).Assembly
            .GetTypes()
            .Where(t => typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract);
    }
}