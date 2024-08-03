using Sirenix.OdinInspector;

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

                return "";
            }
        }
    }
}