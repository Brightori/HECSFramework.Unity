using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.UI, "�������� ���� UI ����� ����������")]
	public struct HideAllUIExceptCommand : IGlobalCommand
	{
		public int UIActorType;
	}
}