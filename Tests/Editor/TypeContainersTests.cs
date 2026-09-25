using System;
using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using NUnit.Framework;

public class TypeContainersTests
{
    [Test]
    public void EveryComponentHasContainer()
    {
        AssertRegistered(typeof(IComponent), TypesMap.ComponentTypes);
    }

    [Test]
    public void EverySystemHasContainer()
    {
        AssertRegistered(typeof(ISystem), TypesMap.SystemTypes);
    }

    private static void AssertRegistered(Type contract, IReadOnlyCollection<Type> registered)
    {
        var known = new HashSet<Type>(registered);

        var missing = typeof(TypesMap).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && contract.IsAssignableFrom(t) && !known.Contains(t))
            .Select(t => t.FullName)
            .OrderBy(name => name)
            .ToList();

        Assert.IsEmpty(missing, $"No type container for: {string.Join(", ", missing)}. Run codegen.");
    }
}
