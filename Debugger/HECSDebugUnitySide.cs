using UnityEngine;

namespace HECSFramework.Unity
{
    public class HECSDebugUnitySide : IDebugDispatcher
    {
        public void LogDebug(string info)
        {
#if DEBUG_LOG
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}");
#endif
        }

        public void Log(string info)
        {
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}");
        }

        public void LogError(string info)
        {
            UnityEngine.Debug.LogError($"[{Time.frameCount}] {info}");
        }

        public void LogWarning(string info)
        {
            UnityEngine.Debug.LogWarning($"[{Time.frameCount}] {info}");
        }

        public void LogDebug(string info, object context)
        {
#if DEBUG_LOG
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}");
#endif
        }
    }
}