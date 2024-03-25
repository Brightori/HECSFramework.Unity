using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.UI, Doc.HECS, Doc.Provider, "provider for ui access component")]
    public sealed class UIAccessProviderComponent : BaseProviderComponent<UIAccessMonoComponent>
    {
    }
}