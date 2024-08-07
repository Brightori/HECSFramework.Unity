using System;
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
using UnityEditor.ShaderGraph;
using UnityEngine;

[Documentation(Doc.HECS, Doc.Quests, Doc.Editor, "this window process and create all blueprints around quests")]
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
            ProcessStages(quest);
        }

        new SOProvider<QuestData>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestGroup>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestStage>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        new SOProvider<QuestsHolderBluePrint>().GetCollection().ForEach((x) => { x.IsValid(); EditorUtility.SetDirty(x); });
        AssetDatabase.SaveAssets();
    }

    [MenuItem("HECS Options/Helpers/Quests/CleanQuestDatas", priority = 11)]
    public static void CleanQuestData()
    {
        var quests = new SOProvider<QuestData>().GetCollection().ToArray();

        foreach (var quest in quests)
        {
            var path = AssetDatabase.GUIDToAssetPath(quest.QuestContainer.AssetGUID);
            var container = AssetDatabase.LoadAssetAtPath<EntityContainer>(path);

            if (container.TryGetComponent(out QuestInfoComponent questInfoComponent))
            {
                if (quest.QuestDataInfo.Equals(questInfoComponent.QuestDataInfo))
                {
                    continue;
                }

                var pathQuest = AssetDatabase.GetAssetPath(quest);
                AssetDatabase.DeleteAsset(pathQuest);
            }
        }

        var groups = new SOProvider<QuestGroup>().GetCollection().ToArray();

        foreach (var group in groups)
        {
            if (group.QuestDatas.Length == 0 && group.Predicates.Length == 0)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(group));
            }

            group.IsValid();
        }

        AssetDatabase.SaveAssets();
    }


    private static void ProcessStages(EntityContainer container)
    {
        if (!container.TryGetBaseComponent(out QuestInfoComponent questInfoComponent))
            return;

        var fileName = $"{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_" +
          $"{questInfoComponent.QuestDataInfo.QuestStageIndex}_" +
          $"QuestStage.asset";

        if (File.Exists(QuestStagesPath + fileName))
            return;

        var assetDBPath = (QuestStagesPath + fileName).Replace(InstallHECS.DataPath, "Assets/").Replace("//", "/");
        var newData = ScriptableObject.CreateInstance<QuestStage>();

        newData.QuestStageInfo.QuestsHolderIndex = questInfoComponent.QuestDataInfo.QuestsHolderIndex;
        newData.QuestStageInfo.QuestStageIndex = questInfoComponent.QuestDataInfo.QuestStageIndex;

        AssetDatabase.CreateAsset(newData, assetDBPath);
        AddressablesHelpers.SetAddressableGroup(newData, "QuestsStages");
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
            SetGroup(entityContainer, questInfoComponent);
            SetGroup(data, questInfoComponent);

            data.IsManualyStarted = entityContainer.IsHaveComponent<QuestManualStartTagComponent>();
            data.QuestDataInfo = questInfoComponent.QuestDataInfo;
            data.RequiredQuestsForStart = questInfoComponent.RequiredQuestsForStart;
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
        newData.IsManualyStarted = entityContainer.IsHaveComponent<QuestManualStartTagComponent>();

        SetGroup(entityContainer, questInfoComponent);

        newData.QuestContainer = new UnityEngine.AddressableAssets.AssetReference(AddressablesHelpers.GetGuidOfObject(entityContainer));
        newData.Predicates = ReflectionHelpers.GetPrivateFieldValue<PredicateBluePrint[]>(entityContainer.GetComponent<PredicatesComponent>(), "predicatesBP");
        newData.RequiredQuestsForStart = questInfoComponent.RequiredQuestsForStart;

        questInfoComponent.QuestDataInfo.QuestContainerIndex = entityContainer.ContainerIndex;
        EditorUtility.SetDirty(entityContainer.GetComponentBluePrint(IndexGenerator.GetIndexForType(typeof(QuestInfoComponent))));

        AssetDatabase.CreateAsset(newData, assetDBPath);
        SetGroup(newData, questInfoComponent);
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
        SetGroup(newData, questInfoComponent);
    }

    private static void SetGroup(UnityEngine.Object newData, QuestInfoComponent questInfoComponent)
    {
        AddressablesHelpers.SetAddressableGroup(newData, $"QuestsGroups_{questInfoComponent.QuestDataInfo.QuestsHolderIndex}_{questInfoComponent.QuestDataInfo.QuestStageIndex}");
    }
}