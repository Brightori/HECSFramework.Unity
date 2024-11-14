using System;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using Sirenix.OdinInspector;
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
        public AccessToIdentifier<Transform>[] GenericAccess = new AccessToIdentifier<Transform>[0];
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


#if UNITY_EDITOR
        [Button]
        public void ProcessTags()
        {
            var allTags = GetComponentsInChildren<UIAccessGenericTagMonoComponent>();

            foreach (var tag in allTags)
            {
                if (tag.GetComponentInParent<UIAccessMonoComponent>() == this)
                {
                    switch (tag.UIAccessType)
                    {
                        case UIAccessType.Image:
                            var image = tag.GetComponent<Image>();
                            var addImage = Images.ToHashSet();
                            addImage.Add(new AccessToIdentifier<Image> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = image });
                            Images = addImage.ToArray();
                            break;
                        case UIAccessType.Button:
                            var button = tag.GetComponent<Button>();
                            var addButton = Buttons.ToHashSet();
                            addButton.Add(new AccessToIdentifier<Button> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = button });
                            Buttons = addButton.ToArray();
                            break;
                        case UIAccessType.Text:
                            var text = tag.GetComponent<TextMeshProUGUI>();
                            var addtext = TextMeshProUGUIs.ToHashSet();
                            addtext.Add(new AccessToIdentifier<TextMeshProUGUI> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = text });
                            TextMeshProUGUIs = addtext.ToArray();
                            break;
                        case UIAccessType.UIAccess:
                            var uiAccessMonoComponent = tag.GetComponent<UIAccessMonoComponent>();
                            var addAccess = UIAccessMonoComponents.ToHashSet();
                            addAccess.Add(new AccessToIdentifier<UIAccessMonoComponent> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = uiAccessMonoComponent });
                            UIAccessMonoComponents = addAccess.ToArray();
                            break;
                        case UIAccessType.CanvasGroup:
                            var canvasGroup = tag.GetComponent<CanvasGroup>();
                            var addCanvasGroup = CanvasGroups.ToHashSet();
                            addCanvasGroup.Add(new AccessToIdentifier<CanvasGroup> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = canvasGroup });
                            CanvasGroups = addCanvasGroup.ToArray();
                            break;
                        case UIAccessType.RectTransform:
                            var rectTransform = tag.GetComponent<RectTransform>();
                            var addRectTransform = RectTransforms.ToHashSet();
                            addRectTransform.Add(new AccessToIdentifier<RectTransform> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = rectTransform });
                            RectTransforms = addRectTransform.ToArray();
                            break;
                        case UIAccessType.Generic:
                            var obj = tag.GetComponent<Transform>();
                            var generics = GenericAccess.ToHashSet();
                            generics.Add(new AccessToIdentifier<Transform> { UIAccessIdentifier = tag.UIAccessIdentifier, Value = obj });
                            GenericAccess = generics.ToArray();
                            break;
                    }

                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }

    [Serializable]
    public struct AccessToIdentifier<T> : IEquatable<AccessToIdentifier<T>> where T : Component
    {
        public UIAccessIdentifier UIAccessIdentifier;
        public T Value;

        public bool Equals(AccessToIdentifier<T> other)
        {
            return Value.GetInstanceID() == other.Value.GetInstanceID();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}