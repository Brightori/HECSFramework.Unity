using UnityEngine;

namespace HECSFramework.Unity
{
    [DefaultExecutionOrder(5000)]
    public class LateStartProvider : MonoBehaviour
    {
        void Start()
        {
            GetComponent<GameController>().LateStart();
        }
    }
}