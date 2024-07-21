using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable]
    [Feature(Doc.Quests)]
    [Documentation(Doc.Quests, Doc.DailyQuests, Doc.HECS, "this is main component, holder for all quests data")]
    public sealed class DailyQuestsHolderComponent : BaseComponent, IValidate
    {
        [SerializeField]
        private AssetReference QuestsHolderBluePrintReference;

        private QuestsHolderBluePrint QuestsHolderBluePrint;

        public async UniTask<QuestsHolderBluePrint> GetQuestsHolder()
        {
            if (QuestsHolderBluePrint)
                return QuestsHolderBluePrint;

            QuestsHolderBluePrint = await Addressables.LoadAssetAsync<QuestsHolderBluePrint>(QuestsHolderBluePrintReference);
            return QuestsHolderBluePrint;
        }

        public void ChangeQuests(QuestsHolderBluePrint questsHolderBluePrint)
        {
            QuestsHolderBluePrint = questsHolderBluePrint;
        }

        [Button]
        public bool IsValid()
        {
            if (QuestsHolderBluePrintReference == null)
            {
                Debug.LogWarning($"we dont have reference on {nameof(DailyQuestsHolderComponent)}");
                return false;
            }

            if (string.IsNullOrEmpty(QuestsHolderBluePrintReference.AssetGUID))
            {
                Debug.LogWarning($"we dont have valid asset reference on {nameof(DailyQuestsHolderComponent)}");
                return false;
            }
            return true;
        }
    }
}