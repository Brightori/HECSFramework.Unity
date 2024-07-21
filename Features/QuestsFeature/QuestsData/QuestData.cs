using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "BluePrints/Quests/QuestData", fileName = "QuestData")]
[Documentation(Doc.Quests, Doc.HECS, "this is config whom hold base context for quests, ids of stage and group, predicates for starting quest and reference to quest entity")]
public class QuestData : ScriptableObject, IValidate
{
    public QuestDataInfo QuestDataInfo;

    public PredicateBluePrint[] Predicates = new PredicateBluePrint[0];
    public AssetReference QuestContainer;

    public bool IsValid()
    {
        if (QuestContainer == null)
        {
            Debug.LogWarning($"we dont have assetreference to quest container {this.name}", this);
            return false;
        }

        if (string.IsNullOrEmpty(QuestContainer.AssetGUID))
        {
            Debug.LogWarning($"we dont have valid quest container {this.name}", this);
            return false;
        }

#if UNITY_EDITOR
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(QuestContainer.AssetGUID);
        var container = UnityEditor.AssetDatabase.LoadAssetAtPath<EntityContainer>(path);
        QuestDataInfo.QuestContainerIndex = container.ContainerIndex;
        
        if (container.TryGetComponent(out PredicatesComponent predicatesComponent))
        {
            Predicates = ReflectionHelpers.GetPrivateFieldValue<PredicateBluePrint[]>(predicatesComponent, "predicatesBP");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        return true;
    }
}

[Serializable]
public struct QuestDataInfo : IEquatable<QuestDataInfo>
{
    public int QuestsHolderIndex;
    public int QuestStageIndex;
    public int QuestGroupIndex;

    [ReadOnly]
    public int QuestContainerIndex;

    public override bool Equals(object obj)
    {
        return obj is QuestDataInfo info &&
               QuestsHolderIndex == info.QuestsHolderIndex &&
               QuestStageIndex == info.QuestStageIndex &&
               QuestGroupIndex == info.QuestGroupIndex &&
               QuestContainerIndex == info.QuestContainerIndex;
    }

    public bool Equals(QuestDataInfo info)
    {
        return QuestsHolderIndex == info.QuestsHolderIndex &&
               QuestStageIndex == info.QuestStageIndex &&
               QuestGroupIndex == info.QuestGroupIndex &&
               QuestContainerIndex == info.QuestContainerIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(QuestsHolderIndex, QuestStageIndex, QuestGroupIndex, QuestContainerIndex);
    }
}
