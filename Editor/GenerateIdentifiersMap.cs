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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial class GenerateIdentifiersMap : UnityEditor.Editor
    {
        private const string LegacyMapFile = "IdentifiersMaps.cs";
        private const string IdentifiersFolder = "Identifiers/";

        private const string IdentifierToStringMapClass = "IdentifierToStringMap";
        private const string EntityContainersMapClass = "EntityContainersMap";
        private const string AbilitiesMapClass = "AbilitiesMap";
        private const string NetworkEntityContainersMapClass = "NetworkEntityContainersMap";
        private const string StrategiesMapClass = "StrategiesMap";

        [MenuItem("HECS Options/Generate Identifiers Map")]
        public static void GenerateIdentifiers()
        {
            var identifiersContainers = AssetDatabase.FindAssets("t:IdentifierContainer")
              .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
              .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<IdentifierContainer>(x)).ToList();

            var entityContainers = AssetDatabase.FindAssets($"t:{nameof(EntityContainer)}")
             .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
             .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<EntityContainer>(x)).ToList();

            var strategies = new SOProvider<Strategy>().GetCollection().ToList();

            var files = new Dictionary<string, GeneratedFile>(256);

            foreach (var identifier in identifiersContainers)
                AddIdentifier(files, identifier);

            foreach (var container in entityContainers)
                AddEntityContainer(files, container);

            foreach (var strategy in strategies)
                AddStrategy(files, strategy);

            SaveToFiles(files);
        }

        private static void AddIdentifier(Dictionary<string, GeneratedFile> files, IdentifierContainer identifier)
        {
            var map = identifier.GetType().Name + "Map";
            var name = GetIdentifierValidName(identifier.name);
            var file = GetFile(files, $"{map}_{name}");

            file.Add(map, $"public const int {name} = {identifier.Id};");
            file.Add(map, $"public const string {name}_string = {Quoted(name)};");
            file.Add(IdentifierToStringMapClass, $"private static readonly bool {file.Name} = Register({IndexGenerator.GetIndexForType(name)}, {Quoted(name)});");
        }

        private static void AddEntityContainer(Dictionary<string, GeneratedFile> files, EntityContainer container)
        {
            var name = ToValidIdentifier(container.name);
            var file = GetFile(files, $"{EntityContainersMapClass}_{name}");
            var index = container.ContainerIndex;
            var rawName = Quoted(container.name);

            file.Add(EntityContainersMapClass, $"public const int _{name} = {index};");
            file.Add(EntityContainersMapClass, $"public const string _{name}_string = {rawName};");
            file.Add(EntityContainersMapClass, $"private static readonly bool {file.Name} = Register({index}, {rawName});");

            if (container.IsHaveComponent<AbilityTagComponent>())
            {
                file.Add(AbilitiesMapClass, $"public const int {name} = {index};");
                file.Add(AbilitiesMapClass, $"public const string {name}_string = {rawName};");
                file.Add(AbilitiesMapClass, $"private static readonly bool {file.Name} = Register({rawName}, {index});");
            }

            if (container.IsHaveComponent<NetworkEntityTagComponent>())
            {
                file.Add(NetworkEntityContainersMapClass, $"public const int {name} = {index};");
                file.Add(NetworkEntityContainersMapClass, $"public const string {name}_string = {rawName};");
            }
        }

        private static void AddStrategy(Dictionary<string, GeneratedFile> files, Strategy strategy)
        {
            var name = ToValidIdentifier(strategy.name);
            var file = GetFile(files, $"{StrategiesMapClass}_{name}");
            var rawName = Quoted(strategy.name);

            file.Add(StrategiesMapClass, $"public const int {name} = {strategy.StrategyIndex};");
            file.Add(StrategiesMapClass, $"public const string {name}_string = {rawName};");
            file.Add(StrategiesMapClass, $"private static readonly bool {file.Name} = Register({strategy.StrategyIndex}, {rawName});");
        }

        public static string GetIdentifierValidName(string identifier)
        {
            return ToValidIdentifier(identifier.Replace("Container", ""));
        }

        private static string ToValidIdentifier(string name)
        {
            var result = new StringBuilder(name.Length + 1);

            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    result.Append(c);
                else if (c == '-')
                    result.Append('_');
            }

            if (result.Length > 0 && char.IsDigit(result[0]))
                result.Insert(0, '_');

            return result.ToString();
        }

        private static string Quoted(string value)
        {
            return CParse.Quote + value + CParse.Quote;
        }

        private static GeneratedFile GetFile(Dictionary<string, GeneratedFile> files, string name)
        {
            if (!files.TryGetValue(name, out var file))
            {
                file = new GeneratedFile(name);
                files.Add(name, file);
            }

            return file;
        }

        private static void SaveToFiles(Dictionary<string, GeneratedFile> files)
        {
            var directory = (InstallHECS.ScriptPath + InstallHECS.HECSGenerated + IdentifiersFolder).Replace("//", "/");
            Directory.CreateDirectory(directory);

            var written = 0;
            var deleted = 0;
            var pending = files.Values.ToArray();

            Parallel.For(0, pending.Length, i =>
            {
                var path = directory + pending[i].Name + ".cs";
                var data = pending[i].ToString();

                if (File.Exists(path) && File.ReadAllText(path) == data)
                    return;

                File.WriteAllText(path, data);
                Interlocked.Increment(ref written);
            });

            var generated = new HashSet<string>(files.Keys, StringComparer.OrdinalIgnoreCase);

            foreach (var path in Directory.GetFiles(directory, "*.cs"))
            {
                if (generated.Contains(Path.GetFileNameWithoutExtension(path)))
                    continue;

                DeleteWithMeta(path);
                deleted++;
            }

            foreach (var legacy in Directory.GetFiles(Application.dataPath, LegacyMapFile, SearchOption.AllDirectories))
            {
                DeleteWithMeta(legacy);
                deleted++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[HECS] Identifiers maps: {files.Count} files, {written} written, {deleted} deleted.");
        }

        private static void DeleteWithMeta(string path)
        {
            File.Delete(path);

            var meta = path + ".meta";

            if (File.Exists(meta))
                File.Delete(meta);
        }

        private sealed class GeneratedFile
        {
            public readonly string Name;

            private readonly List<string> classes = new List<string>(2);
            private readonly Dictionary<string, List<string>> members = new Dictionary<string, List<string>>(2);

            public GeneratedFile(string name)
            {
                Name = name;
            }

            public void Add(string className, string member)
            {
                if (!members.TryGetValue(className, out var classMembers))
                {
                    classMembers = new List<string>(4);
                    members.Add(className, classMembers);
                    classes.Add(className);
                }

                if (!classMembers.Contains(member))
                    classMembers.Add(member);
            }

            public override string ToString()
            {
                var tree = new TreeSyntaxNode();

                for (int i = 0; i < classes.Count; i++)
                {
                    if (i > 0)
                        tree.Add(new ParagraphSyntax());

                    tree.Add(new SimpleSyntax($"public static partial class {classes[i]}" + CParse.Paragraph));
                    tree.Add(new LeftScopeSyntax());

                    foreach (var member in members[classes[i]])
                        tree.Add(new TabSimpleSyntax(1, member));

                    tree.Add(new RightScopeSyntax());
                }

                return tree.ToString();
            }
        }
    }
}
