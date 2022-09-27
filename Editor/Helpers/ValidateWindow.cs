using System.Linq;
using Components;
using HECSFramework.Core;
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
        var entityContainers = new SOProvider<EntityContainer>();
        var list = entityContainers.GetCollection().ToList();

        foreach (var container in list)
        {
            if (container is PresetContainer)
                continue;

            if (!container.IsValid())
                Debug.LogWarning("we have problem with " + container.name);

            foreach (var c in container.GetComponents<IValidate>())
            {
                if (!c.IsValid())
                    Debug.LogWarning($"we have problem with {c.GetType()} on {container.name}");
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