using Components;
using HECSFramework.Core;
using HECSFramework.Core.Generator;
using HECSFramework.Unity.Editor;
using HECSFramework.Unity.Helpers;
using Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial class GenerateIdentifiersMap : UnityEditor.Editor
    {
        [MenuItem("HECS Options/Generate Identifiers Map")]
        public static void GenerateIdentifiers()
        {
            var identifiersContainers = AssetDatabase.FindAssets("t:IdentifierContainer")
              .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
              .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<IdentifierContainer>(x)).ToList();

            var entityContainers = AssetDatabase.FindAssets($"t:{nameof(EntityContainer)}")
             .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
             .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<EntityContainer>(x)).ToList();

            var sort = new Dictionary<Type, HashSet<IdentifierContainer>>(64);

            var strategies = new SOProvider<Strategy>().GetCollection().ToList();

            foreach (var identifier in identifiersContainers)
                AddToDictionary(sort, identifier);

            var composite = new TreeSyntaxNode();
            string maps = string.Empty;

            maps += new UsingSyntax("System.Collections.Generic", 1).ToString();

            foreach (var sorted in sort)
                maps += GetIdentifiersMap(sorted.Key, sorted.Value);

            var abilities = entityContainers.Where(x => x.IsHaveComponent<AbilityTagComponent>()).ToList();
            var names = identifiersContainers.Select(x => x.name).ToHashSet();

            maps += GetContainersMap(entityContainers);
            maps += GetAbilitiesMap(abilities);
            maps += GetNetworkContainersMap(entityContainers);
            maps += GetIdentifierStringMap(names);
            maps += GetStrategiesMap(strategies);

            SaveToFile(maps);
        }

        private static void AddToDictionary(Dictionary<Type, HashSet<IdentifierContainer>> dict, IdentifierContainer container)
        {
            var type = container.GetType();

            if (dict.ContainsKey(type))
                dict[type].Add(container);
            else
            {
                dict.Add(type, new HashSet<IdentifierContainer>());
                dict[type].Add(container);
            }
        }

        private static string GetStrategiesMap(List<Strategy> strategies)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new SimpleSyntax($"public static partial class StrategiesMap" + CParse.Paragraph));

            tree.Add(new LeftScopeSyntax());

            tree.Add(new TabSimpleSyntax(1, "static StrategiesMap()"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetIntToStringDictionaryCLear("StrategiesIDtoString", out var dicBody));
            tree.Add(new RightScopeSyntax(1));

            tree.Add(body);
            tree.Add(new RightScopeSyntax());

            foreach (var e in strategies)
            {
                dicBody.AddUnique(new TabSimpleSyntax(3, $"{CParse.LeftScope} {e.StrategyIndex}, {CParse.Quote}{e.name}{CParse.Quote} {CParse.RightScope},"));
                body.AddUnique(new TabSimpleSyntax(1, $"public const int {e.name} = {e.StrategyIndex};"));
                body.AddUnique(new TabSimpleSyntax(1, $"public const string {e.name}_string = {CParse.Quote}{e.name}{CParse.Quote};"));
            }

            return tree.ToString();
        }

        private static string GetContainersMap(List<EntityContainer> entityContainers)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new SimpleSyntax($"public static partial class EntityContainersMap" + CParse.Paragraph));

            tree.Add(new LeftScopeSyntax());

            tree.Add(new TabSimpleSyntax(1, "static EntityContainersMap()"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetIntToStringDictionary("EntityContainersIDtoString", entityContainers, out var dicBody));
            tree.Add(new RightScopeSyntax(1));

            tree.Add(body);
            tree.Add(new RightScopeSyntax());

            foreach (var e in entityContainers)
            {
                var fieldName = e.name.Replace("-", "").Replace(" ", "").Insert(0,"_");

                dicBody.AddUnique(new TabSimpleSyntax(3, $"{CParse.LeftScope} {e.ContainerIndex}, {CParse.Quote}{e.name}{CParse.Quote} {CParse.RightScope},"));
                body.AddUnique(new TabSimpleSyntax(1, $"public const int {fieldName} = {e.ContainerIndex};"));
                body.AddUnique(new TabSimpleSyntax(1, $"public const string {fieldName}_string = {CParse.Quote}{e.name}{CParse.Quote};"));
            }

            return tree.ToString();
        }

        private static string GetAbilitiesMap(List<EntityContainer> entityContainers)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new SimpleSyntax($"public static partial class AbilitiesMap" + CParse.Paragraph));

            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "static AbilitiesMap()"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetDictionary("AbilitiesToIdentifiersMap", entityContainers));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(body);
            tree.Add(new RightScopeSyntax());

            foreach (var e in entityContainers)
            {
                body.Add(new TabSimpleSyntax(1, $"public const int {e.name} = {e.ContainerIndex};"));
                body.Add(new TabSimpleSyntax(1, $"public const string {e.name}_string = {CParse.Quote}{e.name}{CParse.Quote};"));
            }

            return tree.ToString();
        }

        private static ISyntax GetIntToStringDictionary(string name, List<EntityContainer> containers, out ISyntax body)
        {
            var tree = new TreeSyntaxNode();
            var dicBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(2, $"{name} = new Dictionary<int, string>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(2, true));

            body = dicBody;
            return tree;
        }

        private static ISyntax GetIntToStringDictionaryCLear(string name, out ISyntax body)
        {
            var tree = new TreeSyntaxNode();
            var dicBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(2, $"{name} = new Dictionary<int, string>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(2, true));

            body = dicBody;
            return tree;
        }

        private static ISyntax GetDictionary(string name, List<EntityContainer> containers)
        {
            var tree = new TreeSyntaxNode();
            var dicBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(2, $"{name} = new Dictionary<string, int>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(2, true));

            foreach (var c in containers)
            {
                dicBody.Add(new TabSimpleSyntax(3, $"{CParse.LeftScope} {CParse.Quote}{c.name}{CParse.Quote}, {c.ContainerIndex} {CParse.RightScope},"));
            }

            return tree;
        }

        private static string GetNetworkContainersMap(List<EntityContainer> entityContainers)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new SimpleSyntax($"public static class NetworkEntityContainersMap" + CParse.Paragraph));

            tree.Add(new LeftScopeSyntax());
            tree.Add(body);
            tree.Add(new RightScopeSyntax());

            foreach (var e in entityContainers)
            {
                if (!e.IsHaveComponent<NetworkEntityTagComponent>())
                    continue;

                body.Add(new TabSimpleSyntax(1, $"public const int {e.name} = {e.ContainerIndex};"));
                body.Add(new TabSimpleSyntax(1, $"public const string {e.name}_string = {CParse.Quote}{e.name}{CParse.Quote};"));
            }

            return tree.ToString();
        }

        private static string GetIdentifierStringMap(HashSet<string> identifierNames)
        {
            var stringIdentifiersMap = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();
            var dictionaryIntToString = new TreeSyntaxNode();

            stringIdentifiersMap.Add(new SimpleSyntax($"public static class IdentifierToStringMap" + CParse.Paragraph));

            stringIdentifiersMap.Add(new LeftScopeSyntax());
            stringIdentifiersMap.Add(body);
            stringIdentifiersMap.Add(new RightScopeSyntax());

            //dictionary
            body.Add(new TabSimpleSyntax(1, "public static readonly Dictionary<int, string> IntToString = new Dictionary<int, string>"));
            body.Add(new LeftScopeSyntax(1));
            body.Add(dictionaryIntToString);
            body.Add(new RightScopeSyntax(1, true));

            foreach (var identifier in identifierNames)
            {
                var name = identifier.Replace("Container", "");
                body.Add(new TabSimpleSyntax(1, $"public const string {name} = {CParse.Quote}{name}{CParse.Quote};"));
                dictionaryIntToString.Add(new TabSimpleSyntax(2, $"{CParse.LeftScope} {IndexGenerator.GetIndexForType(name)}, {CParse.Quote}{name}{CParse.Quote}{CParse.RightScope},"));
            }

            return stringIdentifiersMap.ToString();
        }

        private static string GetIdentifiersMap(Type type, HashSet<IdentifierContainer> identifierContainers)
        {
            var composeIdentifiersMap = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            composeIdentifiersMap.Add(new SimpleSyntax($"public static class {type.Name}Map" + CParse.Paragraph));

            composeIdentifiersMap.Add(new LeftScopeSyntax());
            composeIdentifiersMap.Add(body);
            composeIdentifiersMap.Add(new RightScopeSyntax());

            foreach (var identifier in identifierContainers)
            {
                var name = identifier.name.Replace("Container", "");
                body.Add(new TabSimpleSyntax(1, $"public const int {name} = {identifier.Id};"));
            }

            return composeIdentifiersMap.ToString();
        }

        private static void SaveToFile(string data)
        {
            var find = Directory.GetFiles(Application.dataPath, "IdentifiersMaps.cs", SearchOption.AllDirectories);

            var pathToDirectory = InstallHECS.ScriptPath + InstallHECS.HECSGenerated;
            var path = pathToDirectory + "IdentifiersMaps.cs";

            if (find != null && find.Length > 0 && !string.IsNullOrEmpty(find[0]))
            {
                path = find[0];
            }

            try
            {
                if (!Directory.Exists(pathToDirectory))
                    Directory.CreateDirectory(pathToDirectory);

                File.WriteAllText(path, data);
                var sourceFile2 = path.Replace(Application.dataPath, "Assets");
                AssetDatabase.ImportAsset(sourceFile2);
            }
            catch
            {
                Debug.LogError("не смогли ослить " + pathToDirectory);
            }
        }
    }
}
