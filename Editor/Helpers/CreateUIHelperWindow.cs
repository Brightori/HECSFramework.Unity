using System.Collections.Generic;
using System.IO;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Systems;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[Documentation(Doc.Editor, Doc.HECS, Doc.UI, "Its helper for create ui, this window create ui identifier, ui blueprint, and set ui actor to prfb if needed, after this ui blueprint and uiactor will be added to addressables")]
public class CreateUIHelperWindow : OdinEditorWindow
{
    //Addressables Group names
    private const string UIActors = "UI_Actors";
    private const string UIActorsContainers = "UI_ActorsContainers";
    private const string UIBluePrints = "UI_BluePrints";

    public string IdentifierName;

    [AssetsOnly]
    [OnValueChanged(nameof(SetNameOfIdentifierByPrfbName))]
    public GameObject UIprfb;

    public UIGroupIdentifier[] Groups;
    
    public bool IsNeedContainer = true;

    [HideIf(nameof(IsNeedContainer), true)]
    public ActorContainer Container;

    [MenuItem("HECS Options/Helpers/Create UI Window")]
    public static void GetCreateUIHelperWindow()
    {
        GetWindow<CreateUIHelperWindow>();
    }

    [Button]
    public void CreateUI()
    {
        //make path to needed folder, we take constants from InstallHECS
        var pathBluePrints = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.UIBluePrints;
        var pathUIIdentifiers = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers;

        //Here we check and create folders if that needed
        InstallHECS.CheckFolder(pathBluePrints);
        InstallHECS.CheckFolder(pathUIIdentifiers);

        //check all fields
        if (string.IsNullOrEmpty(IdentifierName))
        {
            this.ShowNotification(new GUIContent("Identifier name not setted properly"));
            return;
        }

        if (File.Exists(pathUIIdentifiers + $"{IdentifierName}.asset"))
        {
            this.ShowNotification(new GUIContent("We alrdy have identifier like this"));
            return;
        }

        if (UIprfb == null)
        {
            this.ShowNotification(new GUIContent("Set ui prfb in field"));
            return;
        }

        //Create objects of blueprints
        var uiidentifier = CreateInstance<UIIdentifier>();
        var uibluePrint = CreateInstance<UIBluePrint>();

        //Assign their names, names depends from fields
        uiidentifier.name = IdentifierName;
        uibluePrint.name = $"{UIprfb.name}_UIBluePrint";

        //save SO to project
        AssetDatabase.CreateAsset(uibluePrint, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.UIBluePrints + $"{uibluePrint.name}.asset");
        AssetDatabase.CreateAsset(uiidentifier, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers + $"{uiidentifier.name}.asset");

        //Try to add uiactor component to prfb and save it
        var actor = UIprfb.GetOrAddMonoComponent<UIActor>();
        var uiActorPath = AssetDatabase.GetAssetPath(UIprfb);

        //take guid from prfb and save prfb to addressables groupd
        var uiActorentry = AddressablesHelpers.SetAddressableGroup(UIprfb, UIActors);

        //take guid from bluePrint and save prfb to addressables groupd
        var uiBluePrintEntry = AddressablesHelpers.SetAddressableGroup(uibluePrint, UIBluePrints);
        uiBluePrintEntry.SetLabel(UISystem.UIBluePrints, true);

        //assign fields of blueprints
        uibluePrint.UIType = uiidentifier;
        uibluePrint.UIActor = new UIActorReference(uiActorentry.guid);
        var uiGroupsType = uibluePrint.Groups.GetType();

        var fields = uiGroupsType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        foreach (var f in fields)
        {
            if (f.Name == "Groups")
            {
                f.SetValue(uibluePrint.Groups, Groups);
                break;
            }
        }

        if (IsNeedContainer)
        {
            var newContainer = ScriptableObject.CreateInstance<ActorContainer>();
            newContainer.name = UIprfb.name + "Container";
            var path = AssetDatabase.GetAssetPath(UIprfb);

            var split = path.Split('/');
            path = path.Replace(split.Last(), "");
            path += newContainer.name + ".asset";

            AssetDatabase.CreateAsset(newContainer, path);

            var uiActor = UIprfb.GetOrAddMonoComponent<UIActor>();
            SetActorField(uiActor, newContainer);

            var entryOfContainer = AddAssetToGroup(newContainer, UIActorsContainers);
            uibluePrint.Container = new UnityEngine.AddressableAssets.AssetReference(entryOfContainer.guid);

            newContainer.GetOrAddComponent<UITagComponent>().ViewType = uiidentifier;
            newContainer.GetOrAddComponent<UnityTransformComponent>();
            newContainer.GetOrAddComponent<UnityRectTransformComponent>();
            ReflectionHelpers.SetPrivateFieldValue(newContainer.GetOrAddComponent<UIGroupTagComponent>(), "Groups", Groups);
            newContainer.AddSystemToContainer<HideUISystem>();
        }
        else if (Container != null)
        {
            var uiActor = UIprfb.GetOrAddMonoComponent<UIActor>();
            SetActorField(uiActor, Container);

            var entryOfContainer = AddAssetToGroup(Container, UIActorsContainers);
            uibluePrint.Container = new UnityEngine.AddressableAssets.AssetReference(entryOfContainer.guid);
        }

        EditorUtility.SetDirty(uibluePrint);
        EditorUtility.SetDirty(UIprfb);
        EditorUtility.SetDirty(uiidentifier);

        AssetDatabase.SaveAssets();
    }

    private void SetActorField(Actor actor, ActorContainer actorContainer)
    {
        var fieldsOfUIActor = typeof(Actor).GetFields(
                 System.Reflection.BindingFlags.NonPublic
               | System.Reflection.BindingFlags.Public 
               | System.Reflection.BindingFlags.Instance
               | System.Reflection.BindingFlags.Static
               | System.Reflection.BindingFlags.FlattenHierarchy);

        foreach (var f in fieldsOfUIActor)
        {
            if (f.Name == "actorContainer")
            {
                f.SetValue(actor, actorContainer);
                break;
            }
        }
    }

    public AddressableAssetEntry AddAssetToGroup(UnityEngine.Object asset, string group)
    {
        var addressablesSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        var neededGroup = addressablesSettings.groups.FirstOrDefault(x => x.name == group);

        var addressablesSchemas = CreateInstance<PlayerDataGroupSchema>();
        if (neededGroup == null)
        {
            neededGroup = addressablesSettings.CreateGroup(group, false, false, false,
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>()
                    { addressablesSchemas }, new System.Type[0]);
        }

        var path = AssetDatabase.GetAssetPath(asset);
        var prfbGuid = AssetDatabase.GUIDFromAssetPath(path);
        var entry = addressablesSettings.CreateOrMoveEntry(prfbGuid.ToString(), neededGroup);

        return entry;
    }

    private void SetNameOfIdentifierByPrfbName()
    {
        if (string.IsNullOrEmpty(IdentifierName) && UIprfb != null)
            IdentifierName = UIprfb.name + "_UIIdentifier";
    }
}