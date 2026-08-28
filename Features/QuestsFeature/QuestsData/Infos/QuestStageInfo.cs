using MessagePack;
using System;

namespace Components
{
    [Serializable]
    [MessagePackObject]
#if JsonSerialize
    [Newtonsoft.Json.JsonObject]
#endif
    public struct QuestStageInfo : IEquatable<QuestStageInfo>, IEquatable<QuestGroupInfo>
    {
        [Key(0)]
#if JsonSerialize
        [Newtonsoft.Json.JsonProperty("QuestStageIndex")]
#endif
        public int QuestStageIndex;
        
        
        [Key(1)]
#if JsonSerialize
        [Newtonsoft.Json.JsonProperty("QuestsHolderIndex")]
#endif
        public int QuestsHolderIndex;

        public override bool Equals(object obj)
        {
            return obj is QuestStageInfo info &&
                    QuestStageIndex == info.QuestStageIndex &&
                    QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public bool Equals(QuestStageInfo info)
        {
            return QuestStageIndex == info.QuestStageIndex &&
                   QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public bool Equals(QuestGroupInfo info)
        {
            return QuestStageIndex == info.QuestStageIndex &&
                   QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QuestStageIndex, QuestsHolderIndex);
        }
    }
}