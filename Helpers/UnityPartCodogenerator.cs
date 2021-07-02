using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core.Generator
{
    public partial class CodeGenerator
    {
        public const string BluePrint = "BluePrint";

        #region BluePrintsProvider
        public string GetBluePrintsProvider()
        {
            var tree = new TreeSyntaxNode();
            var constructor = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("Components"));
            tree.Add(new UsingSyntax("System"));
            tree.Add(new UsingSyntax("Systems"));
            tree.Add(new UsingSyntax("System.Collections.Generic",1));
            tree.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public partial class BluePrintsProvider"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new TabSimpleSyntax(2, "public BluePrintsProvider()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(constructor);
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            constructor.Add(GetComponentsBluePrintsDictionary());
            constructor.Add(GetSystemsBluePrintsDictionary());

            return tree.ToString();
        }

        private ISyntax GetComponentsBluePrintsDictionary()
        {
            var tree = new TreeSyntaxNode();
            var dictionaryBody = new TreeSyntaxNode();
            
            tree.Add(new TabSimpleSyntax(2, "Components = new Dictionary<Type, Type>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dictionaryBody);
            tree.Add(new RightScopeSyntax(2, true));

            foreach(var c in componentTypes)
            {
                dictionaryBody.Add(new TabSimpleSyntax(3, $" {CParse.LeftScope} typeof({c.Name}), typeof({c.Name}{BluePrint}) {CParse.RightScope},"));
            }

            return tree;     
        } 
        
        private ISyntax GetSystemsBluePrintsDictionary()
        {
            var tree = new TreeSyntaxNode();
            var dictionaryBody = new TreeSyntaxNode();
            
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "Systems = new Dictionary<Type, Type>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dictionaryBody);
            tree.Add(new RightScopeSyntax(2, true));

            foreach(var s in systems)
            {
                dictionaryBody.Add(new TabSimpleSyntax(3, $" {CParse.LeftScope} typeof({s.Name}), typeof({s.Name}{BluePrint}) {CParse.RightScope},"));
            }

            return tree;     
        }
        #endregion

        #region GenerateSystemsBluePrints
        public List<(string name, string classBody)> GenerateSystemsBluePrints()
        {
            var list = new List<(string name, string classBody)>();

            foreach (var c in systems)
                list.Add((c.Name + BluePrint + ".cs", GetSystemBluePrint(c)));

            return list;
        }

        private string GetSystemBluePrint(Type type)
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new UsingSyntax("Systems", 1));

            tree.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, $"public class {type.Name}{BluePrint} : SystemBluePrint<{type.Name}>"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            return tree.ToString();
        }

        #endregion

        #region GenerateComponentsBluePrints  
        public List<(string name, string classBody)> GenerateComponentsBluePrints()
        {
            var list = new List<(string name, string classBody)>();

            foreach (var c in componentTypes)
                list.Add((c.Name+BluePrint+".cs", GetComponentBluePrint(c)));

            return list;
        }

        private string GetComponentBluePrint(Type type)
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new UsingSyntax("Components"));
            tree.Add(new UsingSyntax("System"));
            tree.Add(new UsingSyntax("System.Collections.Generic",1));

            tree.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, $"public class {type.Name}{BluePrint} : ComponentBluePrintContainer<{type.Name}>"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            return tree.ToString();
        }
        #endregion

        #region Predicates
        public List<(string name, string data)> GetPredicatesBluePrints()
        {
            var list = new List<(string name, string data)>();
            var predicate = typeof(IPredicate);
            var neededClasses = Assembly.Where(p => predicate.IsAssignableFrom(p) && !p.IsGenericType && !p.IsAbstract && !p.IsInterface).ToList();

            foreach (var p in neededClasses)
            {
                list.Add(GetPredicatesBluePrint(p));
            }

            return list;
        }

        private (string name, string data) GetPredicatesBluePrint(Type type)
        {
            var name = type.Name;

            var tree = new TreeSyntaxNode();
            tree.Add(new UsingSyntax("HECSFramework.Unity"));
            tree.Add(new UsingSyntax("Predicates"));
            tree.Add(new UsingSyntax("UnityEngine",1));

            tree.Add(new SimpleSyntax($"[CreateAssetMenu(fileName = "+
                $"{CParse.Quote}{name}BluePrint{CParse.Quote}, " +
                $"menuName = {CParse.Quote}BluePrints/Predicates/{type.Name}{CParse.Quote})]"));

            tree.Add(new SimpleSyntax($"public class {name}BluePrint : PredicateBluePrintContainer<{name}>"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new RightScopeSyntax());
            return (type.Name, tree.ToString());
        }

        #endregion
    }
}