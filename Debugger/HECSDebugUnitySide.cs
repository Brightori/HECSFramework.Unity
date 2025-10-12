using UnityEngine;

namespace HECSFramework.Unity
{
    public class HECSDebugUnitySide : IDebugDispatcher
    {
        public void LogDebug(string info)
        {
#if DEBUG_LOG && DEBUG_FRAMECOUNT
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}");
#elif DEBUG_LOG
            UnityEngine.Debug.Log(info);
#endif
        }

        public void Log(string info)
        {
#if DEBUG_FRAMECOUNT
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}");
#else
            UnityEngine.Debug.Log(info);
#endif
        }

        public void LogError(string info)
        {
#if DEBUG_FRAMECOUNT
            UnityEngine.Debug.LogError($"[{Time.frameCount}] {info}");
#else
            UnityEngine.Debug.LogError(info);
#endif
        }

        public void LogWarning(string info)
        {
#if DEBUG_FRAMECOUNT
            UnityEngine.Debug.LogWarning($"[{Time.frameCount}] {info}");
#else
            UnityEngine.Debug.LogWarning(info);
#endif
        }

        public void LogDebug(string info, Object context)
        {
#if DEBUG_LOG && DEBUG_FRAMECOUNT
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info}", context);
#elif DEBUG_LOG
            UnityEngine.Debug.Log(info, context);
#endif
        }

        public void LogDebug(string info, object context)
        {
#if DEBUG_LOG && DEBUG_FRAMECOUNT
            UnityEngine.Debug.Log($"[{Time.frameCount}] {info} {context.ToString()}");
#elif DEBUG_LOG
            UnityEngine.Debug.Log($"{info} {context.ToString()}");
#endif
        }
    }
}

public partial interface IDebugDispatcher
{
    void LogDebug(string info, UnityEngine.Object context);
}