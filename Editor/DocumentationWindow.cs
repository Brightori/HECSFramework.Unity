using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HECSFramework.Core.Helpers;
using HECSFramework.Core.Generator;

public class DocumentationWindow : OdinEditorWindow
{
    private Vector2 scrollPosButtons;

    private HECSDocumentation documentation = new HECSDocumentation();

    private List<TagButton> buttons = new List<TagButton>(4);

    private Color defaultColor;

    [ShowInInspector, Space(10)][GUIColor(1f, 0.96f, 0.85f, 1)]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true, ShowPaging = false)]
    private List<DocumentationView> systems = new List<DocumentationView>();

    [ShowInInspector, Space(10)]
    [GUIColor(0.85f, 1f, 0.97f, 1)]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true, ShowPaging = false)]
    private List<DocumentationView> components = new List<DocumentationView>();
    
    [ShowInInspector, Space(10)]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true, ShowPaging = false)]
    private List<DocumentationView> common = new List<DocumentationView>();

    [MenuItem("HECS Options/Debug/Project Documentation")]
    public static void ShowDocumentationWindow()
    {
        GetWindow<DocumentationWindow>();
    }

    private Vector3 Gavno(Vector3 another)
    {
        return default;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        defaultColor = GUI.backgroundColor;

        foreach (var doc in documentation.Documentations)
        {
            foreach (var TagName in doc.SegmentTypes)
                buttons.Add(new TagButton { Name = TagName });
        }

        buttons = buttons.OrderBy(x=> x.Name).Distinct().ToList();
    }

    protected override void OnGUI()
    {
        GUILayout.BeginHorizontal();
        scrollPosButtons = EditorGUILayout.BeginScrollView(scrollPosButtons, GUILayout.MaxWidth(200f), GUILayout.Width(120f), GUILayout.MinWidth(60f));
        DrawButtons();
        GUILayout.EndScrollView();
        GUI.backgroundColor = defaultColor;
        base.OnGUI();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUI.backgroundColor = defaultColor;
        if (GUILayout.Button("Reset", GUILayout.Height(30f)))
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var data = buttons[i];
                data.IsActve = false;
                buttons[i] = data;
                RedrawData();
            }        
        }

        if (GUILayout.Button("Search", GUILayout.Height(30f)))
        {
            GetWindow<DocumentationSearchWinow>();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void RedrawData()
    {
        foreach (var view in systems)
            DestroyImmediate(view);        
        
        foreach (var view in components)
            DestroyImmediate(view);

        foreach (var view in common)
            DestroyImmediate(view);

        systems.Clear();
        components.Clear();
        common.Clear();


        var tags = buttons.Where(x => x.IsActve).ToArray();

        var neededDocs = new List<DocumentationRepresentation>(16);

        foreach (var d in documentation.Documentations)
        {
            foreach (var tag in tags)
            {
                if (d.SegmentTypes.Contains(tag.Name))
                {
                    neededDocs.AddOrRemoveElement(d, true);
                    continue;
                }
                else
                {
                    neededDocs.AddOrRemoveElement(d, false);
                    break;
                }
            }
        }

        foreach (var needed in neededDocs)
        {
            var view = CreateInstance<DocumentationView>().Init(needed);

            switch (needed.DocumentationType)
            {
                case DocumentationType.Common:
                    common.Add(view);
                    break;
                case DocumentationType.Component:
                    components.Add(view);
                    break;
                case DocumentationType.System:
                    systems.Add(view);
                    break;
            }
        }
    }

    private void DrawButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i].IsActve)
                GUI.backgroundColor = Color.cyan;
            else
                GUI.backgroundColor = defaultColor;
            
            if (GUILayout.Button(buttons[i].Name))
            {
                var data = buttons[i];
                data.IsActve = !buttons[i].IsActve;
                buttons[i] = data;
                RedrawData();
            }
        }
    }

    [HideLabel]
    public class DocumentationView : ScriptableObject
    {
        [CustomValueDrawer(nameof(DrawLabelAsBox))]
        public string Name;

        [TextArea(3, 5), ReadOnly]
        public string Comments;

        private string GroupName => Name.Replace("Component", "").Replace("BluePrint", "");

        public DocumentationView Init(DocumentationRepresentation documentationRepresentation)
        {
            Name = documentationRepresentation.DataType;
            
            foreach (var c in documentationRepresentation.Comments)
                Comments += c + CParse.Dot+"\n";
            
            return this;
        }

        private string DrawLabelAsBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(GroupName);
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
            return GroupName;
        }
    }

    public struct TagButton
    {
        public string Name;
        public bool IsActve;

        public override bool Equals(object obj)
        {
            return obj is TagButton button &&
                   Name == button.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
