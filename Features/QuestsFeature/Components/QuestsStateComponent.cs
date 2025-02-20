using System;
using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, "here we hold active quests and active hierarchy of quests")]
    public sealed partial class QuestsStateComponent : BaseComponent, IBeforeSerializationComponent, IAfterSerializationComponent, ISavebleComponent, IDirty, IWorldSingleComponent
    {
        [Field(0)]
        public HashSet<QuestStageInfo> ActiveStages = new HashSet<QuestStageInfo>();
        [Field(1)]
        public HashSet<QuestGroupInfo> ActiveGroups = new HashSet<QuestGroupInfo>();

        public HashSet<Entity> ActiveQuests = new HashSet<Entity>();

        [Field(2)]
        private List<QuestDataInfo> QuestIndeces = new List<QuestDataInfo>();

        public bool IsDirty { get; set; }

        public async void AfterSync()
        {
            ActiveQuests.Clear();

            var holder = await Owner.GetComponent<QuestsHolderComponent>().GetQuestsHolder();

            foreach (var q in QuestIndeces)
            {
                var questData = holder.QuestStages[q.QuestStageIndex]
                    .QuestsGroups[q.QuestGroupIndex].GetDataByContainerIndex(q.QuestContainerIndex);

                if (questData == null)
                {
                    Debug.LogError("we dont have quest container for index " + q.QuestContainerIndex);
                    continue;
                }

                var neededContainer = await questData.GetContainer();

                var quest = neededContainer.GetEntity().Init();
                ActiveQuests.Add(quest);
            }
        }

        public void BeforeSync()
        {
            QuestIndeces.Clear();

            foreach (var q in ActiveQuests)
            {
                if (q.ContainsMask<RunTimeQuestTagComponent>())
                    continue;

                QuestIndeces.Add(q.GetComponent<QuestInfoComponent>().QuestDataInfo);
            }
        }

        public bool IsActiveQuest(QuestDataInfo questDataInfo)
        {
            return IsActiveQuest(questDataInfo, out _);
        }

        public bool IsActiveQuest(QuestDataInfo questDataInfo, out Entity activeQuest)
        {
            foreach (var q in ActiveQuests)
            {
                if (q.GetContainerIndex() == questDataInfo.QuestContainerIndex)
                {
                    activeQuest = q;
                    return true;
                }
            }

            activeQuest = null;
            return false;
        }

        public bool IsActiveAndReadyForManualCompleteQuest(QuestDataInfo questDataInfo)
        {
            foreach (var q in ActiveQuests)
            {
                if (q.GetContainerIndex() == questDataInfo.QuestContainerIndex &&
                        q.ContainsMask<QuestCompletedTagComponent>())
                    return true;
            }

            return false;
        }
    }
}