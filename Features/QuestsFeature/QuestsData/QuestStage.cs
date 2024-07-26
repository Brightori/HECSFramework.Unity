using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using MessagePack;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace Components
{
    [Serializable]
    [CreateAssetMenu(menuName = "BluePrints/Quests/QuestStage", fileName = "QuestStage")]
    [Documentation(Doc.HECS, Doc.Quests, "its main unit of quests, its holds quests groups and predicates, main purpose of this - have high lvl checking or executing of quests branch, we look after  ")]
    public partial class QuestStage : ScriptableObject, IValidate
    {
        public QuestStageInfo QuestStageInfo;
        public PredicateBluePrint[] Predicates;
        public QuestGroup[] QuestsGroups;

        public bool IsReady(Entity target, Entity owner = null)
        {
            foreach (var p in Predicates)
            {
                if (!p.GetPredicate.IsReady(target, owner))
                    return false;
            }

            return true;
        }

        #region Validation
        [Button]
        public bool IsValid()
        {
            var questsOnRow = new SOProvider<QuestGroup>().GetCollection().Where(x =>
                x.GroupQuestInfo.QuestStageIndex == QuestStageInfo.QuestStageIndex && QuestStageInfo.QuestsHolderIndex ==
                x.GroupQuestInfo.QuestsHolderIndex).ToArray();

            if (questsOnRow.Length == 0 )
            {
                Debug.LogWarning($"stage: we dont have any group in {this.name}", this);
                return false;
            }

            QuestsGroups = new QuestGroup[questsOnRow.Length];

            foreach (var questGroup in questsOnRow)
            {
                if (QuestsGroups.Length < questGroup.GroupQuestInfo.QuestGroupIndex+1)
                {
                    Array.Resize(ref QuestsGroups, questGroup.GroupQuestInfo.QuestGroupIndex + 1);
                }

                if (QuestsGroups[questGroup.GroupQuestInfo.QuestGroupIndex] == null)
                {
                    QuestsGroups[questGroup.GroupQuestInfo.QuestGroupIndex] = questGroup;
                }
                else
                {
                    Debug.LogError($"stage: this slot is busy by {QuestsGroups[questGroup.GroupQuestInfo.QuestGroupIndex].name} " +
                        $"we try put here {questGroup.GroupQuestInfo.QuestGroupIndex} {questGroup.name}", questGroup);
                    return false;
                }
            }

            if (QuestsGroups.Length == 0)
            {
                Debug.LogWarning($"stage: we dont have any group in {this.name}", this);
                return false;
            }

            foreach (var q in QuestsGroups)
            {
                if (q == null)
                {
                    Debug.LogError($"stage: we have null group at here ", this);
                    return false;
                }

                if (q.QuestDatas.Length == 0)
                {
                    Debug.LogWarning($"stage: we dont have quests in group in {q.name}", q);
                    return false;
                }
            }

            return true;
        }
        #endregion
    }

    [Serializable]
    [MessagePackObject]
    public struct QuestStageInfo : IEquatable<QuestStageInfo>
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
            return  QuestStageIndex == info.QuestStageIndex &&
                    QuestsHolderIndex == info.QuestsHolderIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QuestStageIndex, QuestsHolderIndex);
        }
    }
}