using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class ActionBluePrint : ScriptableObject
    {
        public abstract IAction GetAction();
    }

    public class ActionBluePrint<T> : ActionBluePrint where T : IAction, new()
    {
        [SerializeField] private T action = new T();

        public override IAction GetAction()
        {
            return action;
        }
    }

    public class AsyncActionBluePrint<T> : AsyncActionBluePrint where T : IAsyncAction, new()
    {
        [SerializeField] private T action = new T();

        public override IAsyncAction GetAction()
        {
            return action;
        }
    }
}
