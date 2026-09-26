using System.Collections.Generic;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using NUnit.Framework;
using Strategies;

public class IdentifiersMapsTests
{
    [Test]
    public void EveryIdentifierIsMapped()
    {
        var missing = new SOProvider<IdentifierContainer>().GetCollection()
            .Where(x => x != null)
            .Select(x => GenerateIdentifiersMap.GetIdentifierValidName(x.name))
            .Where(name => !IdentifierToStringMap.IntToString.ContainsKey(IndexGenerator.GetIndexForType(name)));

        AssertMapped(missing, nameof(IdentifierToStringMap));
    }

    [Test]
    public void EveryEntityContainerIsMapped()
    {
        var missing = new SOProvider<EntityContainer>().GetCollection()
            .Where(x => x != null && !EntityContainersMap.EntityContainersIDtoString.ContainsKey(x.ContainerIndex))
            .Select(x => x.name);

        AssertMapped(missing, nameof(EntityContainersMap));
    }

    [Test]
    public void EveryAbilityIsMapped()
    {
        var missing = new SOProvider<EntityContainer>().GetCollection()
            .Where(x => x != null && x.IsHaveComponent<AbilityTagComponent>() && !AbilitiesMap.AbilitiesToIdentifiersMap.ContainsKey(x.name))
            .Select(x => x.name);

        AssertMapped(missing, nameof(AbilitiesMap));
    }

    [Test]
    public void EveryStrategyIsMapped()
    {
        var missing = new SOProvider<Strategy>().GetCollection()
            .Where(x => x != null && !StrategiesMap.StrategiesIDtoString.ContainsKey(x.StrategyIndex))
            .Select(x => x.name);

        AssertMapped(missing, nameof(StrategiesMap));
    }

    private static void AssertMapped(IEnumerable<string> missing, string map)
    {
        var names = missing.OrderBy(name => name).ToList();
        Assert.IsEmpty(names, $"Not in {map}: {string.Join(", ", names)}. Run HECS Options/Generate Identifiers Map.");
    }
}
