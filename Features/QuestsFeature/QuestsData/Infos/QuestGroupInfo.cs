using MessagePack;
using System;

namespace Components
{
    [Serializable]
    [MessagePackObject]
    public struct QuestGroupInfo : IEquatable<QuestGroupInfo>, IEquatable<QuestDataInfo>
    {
        [Key(0)]
        public int QuestGroupIndex;
        [Key(1)]
        public int QuestStageIndex;
        [Key(2)]
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