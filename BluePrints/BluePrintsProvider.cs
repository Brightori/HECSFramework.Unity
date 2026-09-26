using System;
using System.Collections.Generic;

namespace HECSFramework.Unity
{
    public partial class BluePrintsProvider
    {
        public Dictionary<Type, Type> Components = new(sortedComponents);
        public Dictionary<Type, Type> Systems = new(sortedSystems);

        private static readonly Dictionary<Type, Type> sortedComponents;
        private static readonly Dictionary<Type, Type> sortedSystems;

        private static List<KeyValuePair<Type, Type>> collectedComponents;
        private static List<KeyValuePair<Type, Type>> collectedSystems;

        static BluePrintsProvider()
        {
            sortedComponents = Sort(collectedComponents);
            sortedSystems = Sort(collectedSystems);
            collectedComponents = null;
            collectedSystems = null;
        }

        private static bool RegisterComponent(Type component, Type bluePrint)
        {
            (collectedComponents ??= new List<KeyValuePair<Type, Type>>(256)).Add(new KeyValuePair<Type, Type>(component, bluePrint));
            return true;
        }

        private static bool RegisterSystem(Type system, Type bluePrint)
        {
            (collectedSystems ??= new List<KeyValuePair<Type, Type>>(128)).Add(new KeyValuePair<Type, Type>(system, bluePrint));
            return true;
        }

        private static Dictionary<Type, Type> Sort(List<KeyValuePair<Type, Type>> collected)
        {
            if (collected == null)
                return new Dictionary<Type, Type>();

            collected.Sort((a, b) => string.CompareOrdinal(a.Key.Name, b.Key.Name));

            var sorted = new Dictionary<Type, Type>(collected.Count);

            foreach (var pair in collected)
                sorted.Add(pair.Key, pair.Value);

            return sorted;
        }
    }
}
