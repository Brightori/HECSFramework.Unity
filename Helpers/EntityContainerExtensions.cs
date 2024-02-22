using System.Runtime.CompilerServices;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Helpers
{
    [Documentation(Doc.HECS, "extentions for entity containers, remove not containers components for example")]
    public static class EntityContainerExtensions
    {
        public static void RemoveNotBaseComponents(this EntityContainer entityContainer, Entity entity)
        {
            foreach (var c in entity.Components)
            {
                if (entityContainer.ContainsComponent(c))
                    continue;

                entity.World.Command(new RemoveHecsComponentWorldCommand { Component = entity.GetComponent(c) });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIdentifiers(this IdentifierContainer identifierContainer, IdentifierContainer other)
        {
            if (!identifierContainer && !other)
                return true;
            return identifierContainer && other && identifierContainer.Id == other.Id;
        }

        public static void RemoveNotBaseComponentsExcept<T>(this EntityContainer entityContainer, Entity entity)
        {
            foreach (var c in entity.Components)
            {
                if (entityContainer.ContainsComponent(c))
                    continue;

                if (entity.GetComponent(c) is T)
                    continue;

                entity.World.Command(new RemoveHecsComponentWorldCommand { Component = entity.GetComponent(c) });
            }
        }
    }
}
