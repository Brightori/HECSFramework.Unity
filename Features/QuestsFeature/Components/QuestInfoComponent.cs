using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    [Feature("Quest")]
    [Serializable]
    [Documentation(Doc.Quests, "we indicate here position of quests in overall quest hierarchy")]
    public sealed class QuestInfoComponent : BaseComponent, IValidate
    {
        public QuestDataInfo QuestDataInfo;
        public RequiredQuest[] RequiredQuestsForStart = Array.Empty<RequiredQuest>();

        public bool IsValid()
        {
            for (int i = 0; i < RequiredQuestsForStart.Length; i++)
            {
                ref var value = ref RequiredQuestsForStart[i];

                if (!value.IsValid())
                {
                    Debug.LogError("we have invalid id " + RequiredQuestsForStart[i].QuestContainerIndex);
                    return false;
                }
            }

            return true;
        }
    }

    public static class RequiredQuestHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsQuestsReady(this RequiredQuest[] requiredQuests, Entity questManager)
        {
            if (requiredQuests == null)
                return false;

            var history = questManager.GetComponent<QuestsHistoryComponent>();

            for (int i = 0; i < requiredQuests.Length; i++)
            {
                ref RequiredQuest required = ref requiredQuests[i];
                
                if (!history.IsCompletedQuest(required.QuestDataInfo))
                    return false;
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
            FillQuestData();
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