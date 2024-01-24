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
#if UNITY_EDITOR

                if (Application.isPlaying)
                {
#if IdentifiersGenerated
                    if (IdentifierToStringMap.IntToString.ContainsKey(CurrentState))
                    {
                        return IdentifierToStringMap.IntToString[CurrentState];
                    }
                    else
                    {
                        Debug.LogError("we dont have state like this " + CurrentState.ToString());
                        return "Wrong State";
                    }
#else

                    return "";
#endif
                }
                else
                    return "";


#else
                    return "";
#endif
            }
        }
    }
}