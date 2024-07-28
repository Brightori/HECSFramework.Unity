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
    [CreateAssetMenu(menuName = "BluePrints/Quests/QuestGroup", fileName = "QuestGroup")]
    [Documentation(Doc.HECS, Doc.Quests, "group of quests middle lvl unit for isolate thematic quests and exclude them from checking after completing group")]
    public class QuestGroup : ScriptableObject, IValidate
    {
        public QuestGroupInfo GroupQuestInfo;
        public PredicateBluePrint[] Predicates;
        public QuestData[] QuestDatas;

        public bool IsReady(Entity target, Entity owner = null)
        {
            foreach (var p in Predicates)
            {
                if (!p.GetPredicate.IsReady(target, owner))
                    return false;
            }

            return true;
        }

        public QuestData GetDataByContainerIndex(int containerIndex)
        {
            return QuestDatas.FirstOrDefault(x=> x.QuestDataInfo.QuestContainerIndex == containerIndex);
        }

        [Button]
        public bool IsValid()
        {
            QuestDatas = new SOProvider<QuestData>().GetCollection()
                .Where(x => GroupQuestInfo.Equals(x.QuestDataInfo)).ToArray();

            if (QuestDatas.Length == 0)
            {
                Debug.LogWarning($"we dont have any quest datas in {this.name}", this);
                return false;
            }

            if (QuestDatas.Any(x => x == null))
            {
                Debug.LogWarning($"we have null in {this.name}", this);
                return false;
            }

            return true;
        }
    }
}