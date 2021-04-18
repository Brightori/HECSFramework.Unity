using HECSFramework.Core;
using UnityEngine;

namespace Commands
{
    public struct TriggerEnterCommand : ICommand
	{
		public Collider Collider;
	}
	public struct TriggerExitCommand : ICommand
	{
		public Collider Collider;
	}
}