using System;
using System.Collections.Generic;
using UnityEditorInternal.VR;

namespace HECSFramework.Core.Generator
{
    public partial class CodeGenerator
    {
        public const string BluePrint = "BluePrint";

        #region BluePrintsProvider
        public string GetComponentsBluePrintsProvider()
        {
            var tree = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("Components"));
            tree.Add(new UsingSyntax("System"));
            tree.Add(new UsingSyntax("System.Collections.Generic",1));
            tree.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public class BluePrintsProvider"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetComponentsBluePrintsDictionary());
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            return tree.ToString();
        }

        private ISyntax GetComponentsBluePrintsDictionary()
        {
            var tree = new TreeSyntaxNode();
            var dictionaryBody = new TreeSyntaxNode();
            
            tree.Add(new TabSimpleSyntax(2, "public Dictionary<Type, Type> Components = new Dictionary<Type, Type>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(dictionaryBody);
            tree.Add(new RightScopeSyntax(2, true));

            foreach(var c in componentTypes)
            {
                dictionaryBody.Add(new TabSimpleSyntax(3, $" {CParse.LeftScope} typeof({c.Name}), {c.Name}{BluePrint}) {CParse.RightScope},"));
            }

            return tree;     
        }
        #endregion

        #region GenerateComponentsBluePrints  
        public List<(string name, string classBody)> GenerateComponentsBluePrints()
        {
            var list = new List<(string name, string classBody)>();

            foreach (var c in componentTypes)
                list.Add((c.Name, GetComponentBluePrint(c)));

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
    }
}