using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DocumentationWindow;

public class DocumentationSearchWinow : OdinEditorWindow
{
    [ShowInInspector, OnValueChanged("Search")]
    public string SearchWord = string.Empty;

    [ShowInInspector]
    [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, IsReadOnly = true, Expanded = true, ShowPaging = false)]
    [HideIf("@Documentation.Count == 0")]
    private List<DocumentationView> Documentation = new List<DocumentationView>(128);
    private List<DocumentationRepresentation> documentation;

    protected override void OnEnable()
    {
        base.OnEnable();
        documentation = new HECSDocumentation().Documentations;


    }

    [Button]
    public void Search()
    {
        Documentation.Clear();
        var word = SearchWord.ToLower();

        foreach (var d in documentation)
        {
            if (d.DataType.ToLower().Contains(word)
                || d.Comments.Any(x => x.ToLower().Contains(word))
                || d.SegmentTypes.Any(x => x.Contains(word)))
            {
                var view = CreateInstance<DocumentationView>().Init(d);
                Documentation.Add(view);
            }
        }
    }
}