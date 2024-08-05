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
#if IdentifiersGenerated
                if (IdentifierToStringMap.IntToString.ContainsKey(CurrentState))
                {
                    return IdentifierToStringMap.IntToString[CurrentState];
                }
#endif

                return "";
            }
        }
    }
}