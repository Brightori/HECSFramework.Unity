using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
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
        public int QuestStageIndex;
        public int QuestsHolderIndex;
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
                x.GroupQuestInfo.QuestStageIndex == QuestStageIndex && QuestsHolderIndex == 
                x.GroupQuestInfo.QuestsHolderIndex).ToArray();

            QuestsGroups = new QuestGroup[questsOnRow.Length];

            foreach (var qg in questsOnRow)
            {
                if (QuestsGroups[qg.GroupQuestInfo.QuestStageIndex] == null)
                {
                    QuestsGroups[qg.GroupQuestInfo.QuestStageIndex] = qg;
                }
                else
                {
                    Debug.LogWarning($"stage: this slot is busy by {QuestsGroups[qg.GroupQuestInfo.QuestStageIndex].name} " +
                        $"we try put here {qg.GroupQuestInfo.QuestStageIndex} {qg.name}", qg);
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
}