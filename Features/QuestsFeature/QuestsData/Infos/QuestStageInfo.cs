using MessagePack;
using System;

namespace Components
{
    [Serializable]
    [MessagePackObject]
    public struct QuestStageInfo : IEquatable<QuestStageInfo>, IEquatable<QuestGroupInfo>
    {
        [Key(0)]
        public int QuestStageIndex;
        [Key(1)]
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