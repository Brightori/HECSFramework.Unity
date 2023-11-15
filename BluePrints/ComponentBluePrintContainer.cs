using System;
using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class ComponentBluePrint : ScriptableObject, IComponentContainer
    {
        public abstract IComponent GetHECSComponent { get; }
        public virtual bool IsVisible { get; } = true;
        public virtual bool IsOverride { get; set; }
        public virtual bool IsColorNeeded { get; set; }

        [NonSerialized]
        private string data;

        [NonSerialized]
        private bool dataReady;

        public override bool Equals(object obj)
        {
            if (obj is ComponentBluePrint print) 
            {
                var fullname = GetHECSComponent.GetType().FullName;
                var fullnameObj = print.GetHECSComponent.GetType().FullName;
                return (string.Equals(fullname, fullnameObj));
            }

            return false;
        }

        public IComponent GetComponentInstance()
        {
            var t = Instantiate(this);
            return t.GetHECSComponent;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), GetHECSComponent);
        }

        public void LoadFromData(IComponent component)
        {
            if (dataReady)
                JsonUtility.FromJsonOverwrite(data, component);
            else
            {
                data = JsonUtility.ToJson(this.GetHECSComponent);
                dataReady = true;
                JsonUtility.FromJsonOverwrite(data, component);
            }
        }

        public abstract IComponent GetOrAddComponent(Entity entity); 
    }
    
    public class ComponentBluePrintContainer<T> : ComponentBluePrint, IEditorInit where T : class, IComponent, new()
    {
        #region Editor
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawLabelAsBox))]
        [ShowInInspector, HideIf(nameof(IsVisible)), HideLabel, ]
        private string GroupName => name.Replace("Component", "").Replace("BluePrint", "");

        private string DrawLabelAsBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(GroupName);
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
            return GroupName;
        }

        public override bool IsVisible
        {
            get
            {
                if (component == null) component = new T();
                
                return GetAllFields(component.GetType())
                    .Any(a => a.IsPublic || !Attribute.IsDefined(a, typeof(HideInInspector)) &&
                        (Attribute.IsDefined(a, typeof(SerializeField)) || Attribute.IsDefined(a, typeof(ShowInInspectorAttribute))));
            }
        }
        
        private static IEnumerable<System.Reflection.FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<System.Reflection.FieldInfo>();

            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic | 
                                                   System.Reflection.BindingFlags.Static | 
                                                   System.Reflection.BindingFlags.Instance | 
                                                   System.Reflection.BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }
        
    #if DeveloperMode
        [BoxGroup]
    #else
        [FoldoutGroup("$GroupName", VisibleIf = "$IsVisible")]
    #endif
#endif
        #endregion

        [SerializeField, HideLabel] 
        protected T component= new T();
        public override IComponent GetHECSComponent => component;

        void IEditorInit.Init()
            => component = new T();

        public override IComponent GetOrAddComponent(Entity entity)
        {
            if (component is IPoolableComponent)
                return entity.GetOrAddComponent<T>();

            return entity.AddComponent<T>();
        }
    }

    public interface IEditorInit
    {
        void Init();
    }
}