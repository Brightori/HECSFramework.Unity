using System;
using Components;
using HECSFramework.Core;

[Serializable]
public class SetAnimationTrigger : IAction
{
    public AnimatorParameterIdentifier TriggerParameter;

    public void Action(Entity entity)
    {
        if (entity.ContainsMask<AbilityTagComponent>())
        {
            entity.GetComponent<AbilityOwnerComponent>().AbilityOwner.Command(new Commands.TriggerAnimationCommand { Index = TriggerParameter.Id });
        }
        else
        {
            entity.Command(new Commands.TriggerAnimationCommand { Index = TriggerParameter.Id });
        }
    }
}
