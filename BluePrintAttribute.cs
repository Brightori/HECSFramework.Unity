using System;

namespace HECSFramework.Unity
{
    public class BluePrintAttribute : Attribute
    {
        public readonly string GetCustomName;

        public BluePrintAttribute() { }

        public BluePrintAttribute(string name)
        {
            GetCustomName = name;
        }
    }
}