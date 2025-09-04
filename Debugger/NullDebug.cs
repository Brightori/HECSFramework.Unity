using UnityEngine;

public partial class NullDebug : IDebugDispatcher
{
    public void LogDebug(string info, Object context)
    {
        Debug.Log(info, context);
    }
}