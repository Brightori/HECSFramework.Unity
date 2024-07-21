using System.IO;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Documentation(Doc.HECS, Doc.Quests, Doc.Editor, "this ")]
public class QuestsHelper : OdinEditorWindow
{
    private const string Quests = "Quests";
    private const string QuestsStages = "QuestsStages";
    private const string QuestsGroups = "QuestsGroups";
    private const string QuestsDatas = "QuestsDatas";
    private const string Datas = "Datas";

    private static string QuestStagesPath => InstallHECS.DataPath + InstallHECS.BluePrints + $"/{Quests}/" + $"/{Datas}/" + $"/{QuestsStages}/";
    private static string QuestGroupsPath => InstallHECS.DataPath + InstallHECS.BluePrints + $"/{Quests}/" + $"/{Datas}/" + $"/{QuestsGroups}/";
    private static string QuestDatasPath => InstallHECS.DataPath + InstallHECS.BluePrints + $"/{Quests}/" + $"/{Datas}/" + $"/{QuestsDatas}/";

    [MenuItem("HECS Options/Helpers/Quests/ProcessQuests", priority = 11)]
    public static void GenerateQuestData()
    {
        var quests = new SOProvider<EntityContainer>().GetCollection().Where(x => x.IsHaveComponent<QuestTagComponent>());

        InstallHECS.CheckFolder(QuestStagesPath);
        InstallHECS.CheckFolder(QuestGroupsPath);
        InstallHECS.CheckFolder(QuestDatasPath);

        foreach (var quest in quests)
        {
            ProcessQuest(quest);
            ProccessGroups(quest);
        }

        new SOProvider<QuestData>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestGroup>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestStage>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestsHolderBluePrint>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        AssetDatabase.SaveAssets();
    }




    private static void ProcessQuest(EntityContainer quest)
    {
        if (quest.TryGetComponent(out QuestInfoComponent questInfo))
        {
            ProcessQuestData(quest, questInfo);
        }
    }

    private static void ProcessQuestData(EntityContainer entityContainer, QuestInfoComponent questInfoComponent)
    {
        var fileName = $"{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_" +
            $"{questInfoComponent.QuestDataInfo.QuestStageIndex}_" +
            $"{questInfoComponent.QuestDataInfo.QuestGroupIndex}_" +
            $"{entityContainer.name}_QuestData.asset";


        var assetDBPath = (QuestDatasPath + fileName).Replace(InstallHECS.DataPath, "Assets/").Replace("//", "/");

        if (File.Exists(QuestDatasPath + fileName))
        {
            var data = AssetDatabase.LoadAssetAtPath<QuestData>(assetDBPath);
            AddressablesHelpers.SetAddressableGroup(entityContainer, $"Quests_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");
            AddressablesHelpers.SetAddressableGroup(data, $"QuestsDatas_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");

            data.Predicates = ReflectionHelpers.GetPrivateFieldValue<PredicateBluePrint[]>(entityContainer.GetComponent<PredicatesComponent>(), "predicatesBP");
            questInfoComponent.QuestDataInfo.QuestContainerIndex = entityContainer.ContainerIndex;

            EditorUtility.SetDirty(entityContainer.GetComponentBluePrint(IndexGenerator.GetIndexForType(typeof(QuestInfoComponent))));
            EditorUtility.SetDirty(entityContainer);
            EditorUtility.SetDirty(data);
            return;
        }

        var newData = ScriptableObject.CreateInstance<QuestData>();
        newData.QuestDataInfo = questInfoComponent.QuestDataInfo;
        newData.QuestDataInfo.QuestContainerIndex = entityContainer.ContainerIndex;

        AddressablesHelpers.SetAddressableGroup(entityContainer, $"Quests_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");

        newData.QuestContainer = new UnityEngine.AddressableAssets.AssetReference(AddressablesHelpers.GetGuidOfObject(entityContainer));
        newData.Predicates = ReflectionHelpers.GetPrivateFieldValue<PredicateBluePrint[]>(entityContainer.GetComponent<PredicatesComponent>(), "predicatesBP");

        questInfoComponent.QuestDataInfo.QuestContainerIndex = entityContainer.ContainerIndex;
        EditorUtility.SetDirty(entityContainer.GetComponentBluePrint(IndexGenerator.GetIndexForType(typeof(QuestInfoComponent))));

        AssetDatabase.CreateAsset(newData, assetDBPath);
        AddressablesHelpers.SetAddressableGroup(newData, $"QuestsDatas_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");
    }

    private static void ProccessGroups(EntityContainer container)
    {
        if (!container.TryGetBaseComponent(out QuestInfoComponent questInfoComponent))
            return;

        var fileName = $"{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_" +
          $"{questInfoComponent.QuestDataInfo.QuestStageIndex}_" +
          $"{questInfoComponent.QuestDataInfo.QuestGroupIndex}_" +
          $"QuestGroup.asset";

        if (File.Exists(QuestGroupsPath + fileName))
            return;


        var assetDBPath = (QuestGroupsPath + fileName).Replace(InstallHECS.DataPath, "Assets/").Replace("//", "/");
        var newData = ScriptableObject.CreateInstance<QuestGroup>();
        
        newData.GroupQuestInfo.QuestsHolderIndex = questInfoComponent.QuestDataInfo.QuestsHolderIndex;
        newData.GroupQuestInfo.QuestStageIndex = questInfoComponent.QuestDataInfo.QuestStageIndex;
        newData.GroupQuestInfo.QuestGroupIndex = questInfoComponent.QuestDataInfo.QuestGroupIndex;
        
        AssetDatabase.CreateAsset(newData, assetDBPath);
        AddressablesHelpers.SetAddressableGroup(newData, $"QuestsGroups_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");
    }
}