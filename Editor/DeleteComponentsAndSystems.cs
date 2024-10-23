using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[Documentation(Doc.Editor, Doc.HECS, "this window helps delete script files and remove blueprints from containers")]
public class DeleteComponentsAndSystems : OdinEditorWindow
{
    [ValueDropdown(nameof(GetComponents))]
    public List<Type> Components = new List<Type>();

    [ValueDropdown(nameof(GetSystems))]
    public List<Type> Systems = new List<Type>();

    [MenuItem("HECS Options/Helpers/Delete systems and components")]
    public static void ShowDeleteComponentsAndSystems()
    {
        GetWindow<DeleteComponentsAndSystems>();
    }

    private IEnumerable<Type> GetComponents()
    {
        var components = new BluePrintsProvider().Components.Keys.ToList();
        return components;
    }

    private IEnumerable<Type> GetSystems()
    {
        var components = new BluePrintsProvider().Systems.Keys.ToList();
        return components;
    }

    [Button]
    public void Delete()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            var database = new BluePrintsProvider();
            var containers = new SOProvider<EntityContainer>().GetCollection().ToArray();
            DirectoryInfo lookingFor = new DirectoryInfo(Application.dataPath);

            foreach (var component in Components)
            {
                foreach (var c in containers)
                {
                    foreach (var componentBP in c.Components)
                    {
                        if (componentBP.GetHECSComponent.GetType() == component)
                        {
                            c.Components.Remove(componentBP);
                            c.ClearDeletedBluePrints();
                            EditorUtility.SetDirty(c);
                            break;
                        }
                    }
                }

                var find = lookingFor.GetFiles($"*{component.Name}*.*", SearchOption.AllDirectories);

                foreach (var file in find)
                {
                    file.Delete();
                }
            }

            foreach (var sys in Systems)
            {
                foreach (var c in containers)
                {
                    foreach (var systemBP in c.Systems)
                    {
                        if (systemBP.GetSystem.GetType() == sys)
                        {
                            c.Systems.Remove(systemBP);
                            c.ClearDeletedBluePrints();
                            EditorUtility.SetDirty(c);
                            break;
                        }
                    }
                }

                var find = lookingFor.GetFiles($"*{sys.Name}*.*", SearchOption.AllDirectories);

                foreach (var file in find)
                {
                    file.Delete();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    [Button]
    public void CleanNullRefs()
    {
        var soProvider = new SOProvider<EntityContainer>().GetCollection().ToArray();

        foreach (var c in soProvider)
        {
            c.ClearDeletedBluePrints();
        }
    }
}