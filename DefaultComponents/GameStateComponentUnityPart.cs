using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    public partial class GameStateComponent
    {
        [ShowInInspector]
        public string GameStateDebugString
        {
            get
            {
                if (IdentifierToStringMap.IntToString.ContainsKey(CurrentState))
                {
                    return IdentifierToStringMap.IntToString[CurrentState];
                }
                else
                {
                    Debug.LogError("we dont have state like this " + CurrentState.ToString());
                    return "Wrong State";
                }
            }
        }
    }
}