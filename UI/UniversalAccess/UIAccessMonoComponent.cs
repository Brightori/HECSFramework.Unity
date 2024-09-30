using System;
using Assets.Scripts.HECSFramework.HECS.Unity.UI.UniversalAccess;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components
{
    [Documentation(Doc.HECS, Doc.UI, "this component provides access to ui parts, by adding them to collections")]
    public partial class UIAccessMonoComponent : MonoBehaviour
    {
        public AccessToIdentifier<Actor>[] Actors = new AccessToIdentifier<Actor>[0];
        public AccessToIdentifier<Image>[] Images = new AccessToIdentifier<Image>[0];
        public AccessToIdentifier<Button>[] Buttons = new AccessToIdentifier<Button>[0];
        public AccessToIdentifier<CanvasGroup>[] CanvasGroups = new AccessToIdentifier<CanvasGroup>[0];
        public AccessToIdentifier<RectTransform>[] RectTransforms = new AccessToIdentifier<RectTransform>[0];
        public AccessToIdentifier<TextMeshProUGUI>[] TextMeshProUGUIs = new AccessToIdentifier<TextMeshProUGUI>[0];
        public AccessToIdentifier<MonoBehaviour>[] GenericAccess = new AccessToIdentifier<MonoBehaviour>[0];
        public AccessToIdentifier<UIAccessMonoComponent>[] UIAccessMonoComponents = new AccessToIdentifier<UIAccessMonoComponent>[0];

        public Image GetImage(int id) => Get(id, Images);
        public Actor GetActor(int id) => Get(id, Actors);
        public Button GetButton(int id) => Get(id, Buttons);
        public CanvasGroup GetCanvasGroup(int id) => Get(id, CanvasGroups);
        public RectTransform GetRectTransform(int id) => Get(id, RectTransforms);
        public TextMeshProUGUI GetTextMeshProUGUI(int id) => Get(id, TextMeshProUGUIs);
        public UIAccessMonoComponent GetUIAccess(int id) => Get(id, UIAccessMonoComponents);

        public HECSPooledArray<RectTransform> GetRectTransforms(int id)
        {
            return GetArray(id, RectTransforms);
        }

        public HECSPooledArray<Actor> GetActors(int id)
        {
            return GetArray(id, Actors);
        }

        public HECSPooledArray<UIAccessMonoComponent> GetUIAccessElements(int id)
        {
            return GetArray(id, UIAccessMonoComponents);
        }

        public HECSPooledArray<T> GetGenericComponents<T>(int id) where T : Component
        {
            var needed = HECSPooledArray<T>.GetArray(GenericAccess.Length);

            for (int i = 0; i < GenericAccess.Length; i++)
            {
                if (GenericAccess[i].UIAccessIdentifier == id)
                {
                    var lookingFor = GenericAccess[i].Value.GetComponent<T>();

                    if (lookingFor)
                        needed.Add(lookingFor);
                }
            }

            return needed;
        }

        private HECSPooledArray<T> GetArray<T>(int id, AccessToIdentifier<T>[] array) where T : Component
        {
            var get = HECSPooledArray<T>.GetArray(array.Length);

            for (int i = 0; i < RectTransforms.Length; i++)
            {
                if (array[i].UIAccessIdentifier == id)
                {
                    get.Add(array[i].Value);
                }
            }

            return get;
        }

        public T GetGenericComponent<T>(int id) where T : Component
        {
            for (int i = 0; i < GenericAccess.Length; i++)
            {
                if (GenericAccess[i].UIAccessIdentifier == id)
                    return GenericAccess[i].Value.GetComponent<T>();
            }

            return null;
        }

        private T Get<T>(int id, AccessToIdentifier<T>[] accessToIdentifiers) where T : Component
        {
            for (int i = 0; i < accessToIdentifiers.Length; i++)
            {
                if (accessToIdentifiers[i].UIAccessIdentifier == id)
                    return accessToIdentifiers[i].Value;
            }

            return null;
        }
    }
}

namespace Assets.Scripts.HECSFramework.HECS.Unity.UI.UniversalAccess
{
    [Serializable]
    public struct AccessToIdentifier<T> where T : Component
    {
        public UIAccessIdentifier UIAccessIdentifier;
        public T Value;
    }
}