using System;
using UnityEngine;

namespace Components
{
    public class UIAccessPrefabs : MonoBehaviour
    {
        public UIAccessToPrefab[] UIAccessToPrefabs;

        public GameObject GetPrefab(int id)
        {

            for (int i = 0; i < UIAccessToPrefabs.Length; i++)
            {
                if (UIAccessToPrefabs[i].Identifier == id)
                    return UIAccessToPrefabs[i].Prfb;
            }

            Debug.LogError("we dont have such prfb for id " + id, gameObject);
            return null;
        }
    }

    [Serializable]
    public struct UIAccessToPrefab
    {
        public UIAccessIdentifier Identifier;
        public GameObject Prfb;
    }
}