using System;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class ValidateWindow : OdinEditorWindow
{
    [MenuItem("HECS Options/Helpers/Validate window")]
    public static void GetValidateWindow()
    {
        var allObjects = new SOProvider<ScriptableObject>();
        var list = allObjects.GetCollection().OfType<EntityContainer>().ToList();
        var validation = allObjects.GetCollection().Where(x => !list.Contains(x));

        foreach (var check in validation)
        {
            if (check is IValidate validate)
            {
                if (validate.IsValid())
                    continue;
                else
                {
                    Debug.LogError(check.name + " not valid");
                }
            }
        }

        foreach (var container in list) 
        {
            try
            {
                var holder = ReflectionHelpers.GetPrivateFieldValue<ComponentsSystemsHolder>(container, "holder");
                if (holder == null || holder.components == null || container.Components.Any(x => x == null))
                {
                    throw new Exception("we have null component in " + container.name);
                }

                if (container.Systems.Any(x => x == null))
                {
                    throw new Exception("we have null system in " + container.name);
                }
            }
            catch
            {
                Debug.LogError("we have null refs at " + container.name);
            }
        }

        foreach (var container in list)
        {
            if (container is PresetContainer)
                continue;

            if (!container.IsValid())
                Debug.LogWarning("we have problem with " + container.name);

            try
            {
                foreach (var c in container.GetComponents<IValidate>())
                {
                    try
                    {
                        if (!c.IsValid())
                            Debug.LogWarning($"we have problem with {c.GetType()} on {container.name}");
                    }
                    catch
                    {
                        Debug.LogError(container.name + " we have error here");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString() + $" {container.name}");
            }

            try
            {
                if (container.TryGetComponent(out ViewReferenceComponent viewReferenceComponent))
                {
                    if (viewReferenceComponent.ViewReference == null)
                    {
                        Debug.LogError("viewRef null on container " + container.name);
                    }

                    if (!AddressablesHelpers.IsAssetAddressable(viewReferenceComponent.ViewReference.AssetGUID))
                        Debug.LogError("viewRef not addressable at " + container.name);
                }
            }
            catch
            {
                Debug.LogError("we have problem with" + container.name);
            }

            EntityContainerEditorHelper.MarkDirtyAllInContainer(container);
        }

        Debug.Log("Validation Complete");
    }

    [MenuItem("HECS Options/Helpers/Save Histories")]
    public static void SaveHistories()
    {
        var entityContainers = new SOProvider<EntityContainer>();
        var list = entityContainers.GetCollection().ToList();

        foreach (var container in list)
        {
            if (container is PresetContainer)
                continue;

            container.AddToHistory();
        }
    }
}