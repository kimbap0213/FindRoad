using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PathMapAgent))]
public class PathMapAgentEditor : Editor
{
    public VisualTreeAsset visualTreeAsset;
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        visualTreeAsset.CloneTree(root);
        
        root.Query<Button>("GenerateNode").First().clicked += () => ((PathMapAgent)target).GenerateNode();
        root.Query<Button>("ResetNode").First().clicked += () => ((PathMapAgent)target).ResetNode();
        root.Query<Button>("CalculatePath").First().clicked += () => ((PathMapAgent)target).CalculatePath();
        return root;
    }
}
