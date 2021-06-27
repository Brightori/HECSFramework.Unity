using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using Systems;

namespace Components
{
    [Serializable, BluePrint]
    [Documentation("Ability", "��� ������� ���, �� ��� ��������� � ������� ����������")]
    [Documentation("Tag")]
    public  class AbilityTagComponent : BaseComponent, IAbilityTagComponent
    {
    }

    public interface IAbilityTagComponent : IComponent
    {
    }
}