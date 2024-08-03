using HECSFramework.Core;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace Components
{
    [CreateAssetMenu(menuName = "BluePrints/Quests/QuestsHolder", fileName = "QuestHolder")]
    [Documentation(Doc.Quests, Doc.HECS, "we isolate  quests in SO bcz we can update this SO through remote addressable, why can we have more then one questsholder? - mby we need ab scenarios (diff quests) for different players")]
    public partial class QuestsHolderBluePrint : ScriptableObject, IValidate
    {
        public int QuestsHolderIndex;
        public QuestStage[] QuestStages;

        public bool TryGetQuestData(QuestDataInfo questDataInfo, out QuestData questData)
        {
            questData = null;

            try
            {
                questData = QuestStages[questDataInfo.QuestStageIndex].
                    QuestsGroups[questDataInfo.QuestGroupIndex].
                        QuestDatas.FirstOrDefault(x => x.QuestDataInfo.Equals(questDataInfo));
                return questData != null;
            }
            catch
            {
                Debug.LogError("we dont have questData for " + questDataInfo.ToString());
            }
                
            return false;
        }

        #region Validation
        [Button]
        public bool IsValid()
        {
            var items = new SOProvider<QuestStage>().GetCollection().Where(x => x.QuestStageInfo.QuestsHolderIndex == QuestsHolderIndex).ToArray();

            QuestStages = new QuestStage[items.Length];

            foreach (var item in items)
            {
                if (QuestStages[item.QuestStageInfo.QuestStageIndex] == null)
                {
                    QuestStages[item.QuestStageInfo.QuestStageIndex] = item;
                }
                else
                {
                    Debug.LogWarning($"this slot is busy by {QuestStages[item.QuestStageInfo.QuestStageIndex].name} we try put here {item.QuestStageInfo.QuestStageIndex} {item.name}", item);
                    return false;
                }
            }

            if (QuestStages.Length == 0)
            {
                Debug.LogWarning($"we dont have any quests stages in {this.name}", this);
                return false;
            }

            foreach (var q in QuestStages)
            {
                if (q.QuestsGroups.Length == 0)
                {
                    Debug.LogWarning($"we dont have groups in {q.name}", q);
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}