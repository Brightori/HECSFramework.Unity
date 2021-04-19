using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DocumentationWindow : OdinEditorWindow
{
    private Vector2 scrollPosButtons;


    private HECSDocumentation documentation = new HECSDocumentation();

    private List<TagButton> buttons = new List<TagButton>(4);

    private Color defaultColor;

    [BoxGroup("TagInfos")]
    private List<DocumentationView> systems = new List<DocumentationView>();

    [MenuItem("HECS Options/Debug/Project Documentation")]
    public static void ShowDocumentationWindow()
    {
        GetWindow<DocumentationWindow>();
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

        buttons = buttons.Distinct().ToList();
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

        GUI.backgroundColor = defaultColor;
        if (GUILayout.Button("Reset", GUILayout.Height(30f)))
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var data = buttons[i];
                data.IsActve = false;
                buttons[i] = data;
            }        
        }

        GUILayout.EndHorizontal();
    }

    private void RedrawData()
    {

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
            }
        }
    }

    public class DocumentationView
    {

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
