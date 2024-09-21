using System;
using Components;
using HECSFramework.Core;

[Serializable]
public class SetAnimationBoolean : IAction
{
    public AnimatorParameterIdentifier TriggerParameter;
    public bool Value;

    public void Action(Entity entity)
    {
        if (entity.ContainsMask<AbilityTagComponent>())
        {
            entity.GetComponent<AbilityOwnerComponent>().AbilityOwner.Command(new Commands.BoolAnimationCommand { Index = TriggerParameter.Id, Value = this.Value });
        }
        else
        {
            entity.Command(new Commands.BoolAnimationCommand { Index = TriggerParameter.Id, Value = this.Value });
        }
    }
}
