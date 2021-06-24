using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(5000)]
    public class LateStartProvider : MonoBehaviour
    {
        void Start()
        {
            GetComponent<BaseGameController>().LateStart();
        }
    }
}