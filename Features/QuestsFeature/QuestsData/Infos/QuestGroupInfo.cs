using MessagePack;
using System;

namespace Components
{
    [Serializable]
    [MessagePackObject]
#if JsonSerialize
    [Newtonsoft.Json.JsonObject]
#endif
    public struct QuestGroupInfo : IEquatable<QuestGroupInfo>, IEquatable<QuestDataInfo>
    {
        [Key(0)]
#if JsonSerialize
        [Newtonsoft.Json.JsonProperty("QuestGroupIndex")]
#endif
        public int QuestGroupIndex;

        [Key(1)]
#if JsonSerialize
        [Newtonsoft.Json.JsonProperty("QuestStageIndex")]
#endif
        public int QuestStageIndex;

        [Key(2)]
#if JsonSerialize
        [Newtonsoft.Json.JsonProperty("QuestsHolderIndex")]
#endif
        public int QuestsHolderIndex;

        public override bool Equals(object obj)
        {
            return obj is QuestGroupInfo info &&
                   QuestGroupIndex == info.QuestGroupIndex &&
                   QuestStageIndex == info.QuestStageIndex &&
                   QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public bool Equals(QuestGroupInfo info)
        {
            return QuestGroupIndex == info.QuestGroupIndex &&
                   QuestStageIndex == info.QuestStageIndex &&
                   QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public bool Equals(QuestDataInfo other)
        {
            return other.QuestGroupIndex == QuestGroupIndex &&
                   other.QuestsHolderIndex == QuestsHolderIndex &&
                   other.QuestStageIndex == QuestStageIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QuestGroupIndex, QuestStageIndex, QuestsHolderIndex);
        }
    }
}