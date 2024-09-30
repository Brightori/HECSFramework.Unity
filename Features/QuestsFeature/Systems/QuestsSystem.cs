using System;
using System.Linq;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;

namespace Systems
{
    [Feature(Doc.Quests)]
    [Serializable]
    [Documentation(Doc.Quests, Doc.HECS, "QuestsSystem operates progress of quests")]
    public sealed class QuestsSystem : BaseSystem, IQuestSystem
    {
        [Required]
        public QuestsHistoryComponent QuestsHistoryComponent;

        [Required]
        public QuestsStateComponent QuestsStateComponent;

        [Required]
        public QuestsHolderComponent QuestsHolderComponent;

        public override void InitSystem()
        {
        }

        public void CommandGlobalReact(UpdateQuestGlobalCommand command)
        {
            foreach (var q in QuestsStateComponent.ActiveQuests)
            {
                q.Command(command);
            }
        }

        public async void CommandGlobalReact(QuestCompleteGlobalCommand command)
        {
            this.QuestsStateComponent.ActiveQuests.Remove(command.Quest);

            var questDataInfo = command.Quest.GetComponent<QuestInfoComponent>().QuestDataInfo;
            QuestsHistoryComponent.CompletedQuests.Add(questDataInfo);
            QuestsStateComponent.ActiveQuests.Remove(command.Quest);

            var holder = await QuestsHolderComponent.GetQuestsHolder();

            var group = holder.QuestStages[questDataInfo.QuestStageIndex].QuestsGroups[questDataInfo.QuestGroupIndex];

            if (group.QuestDatas.All(x => QuestsHistoryComponent.CompletedQuests.Contains(x.QuestDataInfo)))
            {
                QuestsHistoryComponent.CompletedGroups.Add(group.GroupQuestInfo);
                QuestsStateComponent.ActiveGroups.Remove(group.GroupQuestInfo);
                RemoveQuestsOfCompletedGroup(group);
            }

            CompleteStage(holder.QuestStages[questDataInfo.QuestStageIndex]);
            command.Quest.HECSDestroyEndOfFrame();
        }

        private void CompleteStage(QuestStage questStage)
        {
            if (questStage.QuestsGroups.All(x => QuestsHistoryComponent.CompletedGroups.Contains(x.GroupQuestInfo)))
            {
                foreach (var questGroup in questStage.QuestsGroups)
                {
                    QuestsHistoryComponent.CompletedGroups.Remove(questGroup.GroupQuestInfo);
                }

                QuestsHistoryComponent.CompletedStages.Add(questStage.QuestStageInfo);
                QuestsStateComponent.ActiveStages.Remove(questStage.QuestStageInfo);
            }
        }

        private void RemoveQuestsOfCompletedGroup(QuestGroup group)
        {
            using var pooled = HECSPooledArray<QuestDataInfo>.GetArray(QuestsHistoryComponent.CompletedQuests.Count);

            foreach (var q in QuestsHistoryComponent.CompletedQuests)
            {
                if (q.Equals(group.GroupQuestInfo))
                {
                    pooled.Add(q);
                }
            }

            for (int i = 0; i < pooled.Count; i++)
            {
                QuestsHistoryComponent.CompletedQuests.Remove(pooled.Items[i]);
            }
        }

        public async void CommandGlobalReact(CheckQuestsGlobalCommand command)
        {
            var questsHolder = await QuestsHolderComponent.GetQuestsHolder();

            foreach (var s in questsHolder.QuestStages)
            {
                var stageIndex = s.QuestStageInfo.QuestStageIndex;

                if (QuestsHistoryComponent.CompletedStages.Contains(s.QuestStageInfo))
                    continue;

                if (!QuestsStateComponent.ActiveStages.Contains(s.QuestStageInfo))
                {
                    if (!s.IsReady(Owner))
                        continue;

                    QuestsStateComponent.ActiveStages.Add(s.QuestStageInfo);
                }

                foreach (var g in s.QuestsGroups)
                {
                    if (QuestsHistoryComponent.CompletedGroups.Contains(g.GroupQuestInfo))
                        continue;

                    if (!QuestsStateComponent.ActiveGroups.Contains(g.GroupQuestInfo))
                    {
                        if (!s.IsReady(Owner))
                            continue;

                        QuestsStateComponent.ActiveGroups.Add(g.GroupQuestInfo);
                    }

                    foreach (var q in g.QuestDatas)
                    {
                        if (q.IsManualyStarted)
                            continue;

                        if (QuestsHistoryComponent.CompletedQuests.Contains(q.QuestDataInfo) || IsActiveQuest(q.QuestDataInfo))
                            continue;

                        if (!q.IsRequiredCompleted(Owner))
                            continue;

                        StartQuest(q);
                    }
                }
            }
        }

        private async void StartQuest(QuestData questData, StartQuestCommand startQuestCommand = default, bool addCompleteInfo = false)
        {
            var container = await questData.GetContainer();
            var quest = container.GetEntity().Init();
            quest.Command(startQuestCommand);
            QuestsStateComponent.ActiveQuests.Add(quest);

            if (addCompleteInfo)
            {
                var info = quest.GetComponent<QuestInfoComponent>().QuestDataInfo;
                QuestsStateComponent.ActiveStages.Add(new QuestStageInfo
                {
                    QuestsHolderIndex = info.QuestsHolderIndex,
                    QuestStageIndex = info.QuestStageIndex
                });
                
                QuestsStateComponent.ActiveGroups.Add(new QuestGroupInfo
                {
                    QuestsHolderIndex = info.QuestsHolderIndex,
                    QuestStageIndex = info.QuestStageIndex,
                    QuestGroupIndex = info.QuestGroupIndex
                });
            }
        }

        private bool IsActiveQuest(QuestDataInfo questDataInfo)
        {
            return QuestsStateComponent.IsActiveQuest(questDataInfo);
        }

        public void CommandGlobalReact(ForceCompleteQuestCommand command)
        {
            foreach (var activeQuest in QuestsStateComponent.ActiveQuests)
            {
                if (activeQuest.GetComponent<QuestInfoComponent>().QuestDataInfo.Equals(command.QuestDataInfo))
                {
                    activeQuest.Command(command);
                    return;
                }
            }
        }

        public async void CommandGlobalReact(ForceStartQuestCommand command)
        {
            var questHolder = await QuestsHolderComponent.GetQuestsHolder();
            if (questHolder.TryGetQuestData(command.QuestDataInfo, out var questData))
            {
                if (IsActiveQuest(questData.QuestDataInfo))
                    return;
                
                if (QuestsHistoryComponent.IsCompletedQuest(questData.QuestDataInfo))
                    return;

                StartQuest(questData, new StartQuestCommand { From = command.From, To = command.To }, true);
            }
        }
    }

    public interface IQuestSystem : ISystem,
        IReactGlobalCommand<UpdateQuestGlobalCommand>,
        IReactGlobalCommand<QuestCompleteGlobalCommand>,
        IReactGlobalCommand<CheckQuestsGlobalCommand>,
        IReactGlobalCommand<ForceCompleteQuestCommand>,
        IReactGlobalCommand<ForceStartQuestCommand>
    {
    }
}