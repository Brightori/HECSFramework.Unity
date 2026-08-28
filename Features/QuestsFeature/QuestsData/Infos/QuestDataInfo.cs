using System;
using Components;
using MessagePack;
using Sirenix.OdinInspector;

[Serializable]
[MessagePackObject]
#if JsonSerialize
[Newtonsoft.Json.JsonObject]
#endif
public struct QuestDataInfo : IEquatable<QuestDataInfo>, IEquatable<QuestGroupInfo>
{
    [Key(0)]
#if JsonSerialize
    [Newtonsoft.Json.JsonProperty("QuestsHolderIndex")]
#endif
    public int QuestsHolderIndex;

    [Key(1)]
#if JsonSerialize
    [Newtonsoft.Json.JsonProperty("QuestStageIndex")]
#endif
    public int QuestStageIndex;

    [Key(2)]
#if JsonSerialize
    [Newtonsoft.Json.JsonProperty("QuestGroupIndex")]
#endif
    public int QuestGroupIndex;

    [ReadOnly]
    [Key(3)]
#if JsonSerialize
    [Newtonsoft.Json.JsonProperty("QuestContainerIndex")]
#endif
    public int QuestContainerIndex;

    public static implicit operator QuestGroupInfo(QuestDataInfo info) => new QuestGroupInfo
    {
        QuestGroupIndex = info.QuestGroupIndex,
        QuestsHolderIndex = info.QuestsHolderIndex,
        QuestStageIndex = info.QuestStageIndex
    };

    public static implicit operator QuestStageInfo(QuestDataInfo info) => new QuestStageInfo
    {
        QuestsHolderIndex = info.QuestsHolderIndex,
        QuestStageIndex = info.QuestStageIndex
    };

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

    /// <summary>
    /// this method checks - group info include this quest data
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Equals(QuestGroupInfo info)
    {
        return QuestsHolderIndex == info.QuestsHolderIndex &&
              QuestStageIndex == info.QuestStageIndex &&
              QuestGroupIndex == info.QuestGroupIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(QuestsHolderIndex, QuestStageIndex, QuestGroupIndex, QuestContainerIndex);
    }

    public override string ToString()
    {
        return $"QuestsHolderIndex: {QuestsHolderIndex}, QuestStageIndex: {QuestStageIndex}, " +
            $"QuestGroupIndex {QuestGroupIndex}, QuestContainerIndex{QuestContainerIndex}";
    }
}