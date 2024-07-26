using System;
using System.Linq;
using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Feature(Doc.Quests)]
	[Serializable][Documentation(Doc.Quests, Doc.HECS, "QuestsSystem operates progress of quests and ")]
    public sealed class QuestsSystem : BaseSystem, IQuestSystem 
    {
        [Required]
        public QuestsHistoryComponent QuestsHistoryComponent;

        [Required]
        public QuestsStateComponent QuestsStateComponent;

        [Required]
        public QuestsHolderComponent QuestsHolderComponent;

        public void CommandGlobalReact(UpdateQuestGlobalCommand command)
        {
            foreach (var q in QuestsStateComponent.ActiveQuests)
            {
                q.Command(command);
            }
        }

        public void CommandGlobalReact(QuestCompleteGlobalCommand command)
        {
            throw new NotImplementedException();
        }

        public async void CommandGlobalReact(CheckQuestsGlobalCommand command)
        {
            var questsHolder = await QuestsHolderComponent.GetQuestsHolder();

            foreach (var s in questsHolder.QuestStages)
            {
                var stageIndex = s.QuestStageInfo.QuestStageIndex;

                if (QuestsHistoryComponent.CompletedStages.Contains(s.QuestStageInfo))
                    continue;

                if (!s.IsReady(Owner))
                    continue;

                QuestsStateComponent.ActiveStages.Add(s.QuestStageInfo);

                foreach (var g in s.QuestsGroups)
                {
                    if (QuestsHistoryComponent.CompletedGroups.Contains(g.GroupQuestInfo))
                        continue;

                    if (!QuestsStateComponent.ActiveGroups.Contains(g.GroupQuestInfo))
                    {
                        if (!s.IsReady(Owner))
                            continue;
                        else
                        {
                            QuestsStateComponent.ActiveGroups.Add(g.GroupQuestInfo);
                        }
                    }

                    foreach (var q in g.QuestDatas)
                    {
                        if (QuestsHistoryComponent.CompletedQuests.Contains(q.QuestDataInfo) || IsActiveQuest(q.QuestDataInfo))
                            continue;

                        var container = await q.GetContainer();
                        var quest = container.GetEntity();
                        quest.Init();
                        quest.Command(new StartQuestCommand());

                    }
                }
            }
        }

        private bool IsActiveQuest(QuestDataInfo questDataInfo)
        {
            return QuestsStateComponent.ActiveQuests.Any(x => x.GetComponent<ActorContainerID>().ContainerIndex == questDataInfo.QuestContainerIndex);
        }

        public override void InitSystem()
        {
        }
    }

    public interface IQuestSystem : ISystem, 
        IReactGlobalCommand<UpdateQuestGlobalCommand>, 
        IReactGlobalCommand<QuestCompleteGlobalCommand>,
        IReactGlobalCommand<CheckQuestsGlobalCommand>
    {
    }
}