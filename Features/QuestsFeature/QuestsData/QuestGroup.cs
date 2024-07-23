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

    [Serializable]
    public struct QuestGroupInfo : IEquatable<QuestGroupInfo>, IEquatable<QuestDataInfo>
    {
        public int QuestGroupIndex;
        public int QuestStageIndex;
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