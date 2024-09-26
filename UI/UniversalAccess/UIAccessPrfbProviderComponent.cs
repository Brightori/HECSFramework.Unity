using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.UI, Doc.HECS, Doc.Provider, "provider for ui access component with prfbs needed for ui")]
    public sealed class UIAccessPrfbProviderComponent : BaseProviderComponent<UIAccessPrefabs>
    {
    }
}