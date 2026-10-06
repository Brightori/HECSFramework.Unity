using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
#if JsonSerialize
    [HECSFramework.Serialize.JSONHECSSerialize]
#endif
    [Documentation(Doc.Quests, "here we hold active quests and active hierarchy of quests")]
    public sealed partial class QuestsStateComponent : BaseComponent, IBeforeSerializationComponent, IAfterSerializationComponent, ISavebleComponent, IDirty, IWorldSingleComponent
    {
        [Field(0)]
        public HashSet<QuestStageInfo> ActiveStages = new HashSet<QuestStageInfo>();
        [Field(1)]
        public HashSet<QuestGroupInfo> ActiveGroups = new HashSet<QuestGroupInfo>();

        public HashSet<Entity> ActiveQuests = new HashSet<Entity>();

        public HashSet<int> StartingQuests = new HashSet<int>();

        [Field(2)]
        private List<QuestDataInfo> QuestIndeces = new List<QuestDataInfo>();

        private readonly List<QuestDataInfo> restoringQuests = new List<QuestDataInfo>();

        public bool IsDirty { get; set; }

        public async void AfterSync()
        {
            ActiveQuests.Clear();

            foreach (var q in QuestIndeces)
            {
                if (StartingQuests.Add(q.QuestContainerIndex))
                    restoringQuests.Add(q);
            }

            var toRestore = restoringQuests.ToArray();

            try
            {
                var holder = await Owner.GetComponent<QuestsHolderComponent>().GetQuestsHolder();

                foreach (var q in toRestore)
                {
                    var questData = holder.QuestStages[q.QuestStageIndex]
                        .QuestsGroups[q.QuestGroupIndex].GetDataByContainerIndex(q.QuestContainerIndex);

                    if (questData == null)
                    {
                        Debug.LogError("we dont have quest container for index " + q.QuestContainerIndex);
                        ReleaseRestoring(q);
                        continue;
                    }

                    var neededContainer = await questData.GetContainer();

                    var quest = neededContainer.GetEntity().Init();
                    ActiveQuests.Add(quest);
                    ReleaseRestoring(q);
                }
            }
            finally
            {
                foreach (var q in toRestore)
                    ReleaseRestoring(q);
            }
        }

        private void ReleaseRestoring(QuestDataInfo questDataInfo)
        {
            if (restoringQuests.Remove(questDataInfo))
                StartingQuests.Remove(questDataInfo.QuestContainerIndex);
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

            QuestIndeces.AddRange(restoringQuests);
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