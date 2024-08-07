using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    [Feature("Quest")]
    [Serializable][Documentation(Doc.Quests, "we indicate here position of quests in overall quest hierarchy")]
    public sealed class QuestInfoComponent : BaseComponent, IValidate
    {
        public QuestDataInfo QuestDataInfo;
        public RequiredQuest[] RequiredQuestsForStart = Array.Empty<RequiredQuest>();

        public bool IsValid()
        {
            foreach (var item in RequiredQuestsForStart)
            {
                if (!item.IsValid())
                {
                    Debug.LogError("we have invalid id " + item.QuestContainerIndex);
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public struct RequiredQuest : IValidate
    {
        [HideInInspector]
        public QuestDataInfo QuestDataInfo;

        [OnValueChanged(nameof(FillQuestData))]
        [EntityContainerIDDropDown(nameof(QuestTagComponent))]
        public int QuestContainerIndex;

        public bool IsValid()
        {
            return EntityContainersMap.EntityContainersIDtoString.ContainsKey(QuestContainerIndex);
        }

        private void FillQuestData()
        {
            var index = QuestContainerIndex;
            var container = new SOProvider<EntityContainer>().GetCollection().FirstOrDefault(x => x.ContainerIndex == index);

            if (container.TryGetComponent(out QuestInfoComponent questInfoComponent))
            {
                QuestDataInfo = questInfoComponent.QuestDataInfo;
            }
        }
    }
}