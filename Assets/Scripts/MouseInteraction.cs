using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    private static MouseInteraction _instance;
    public static MouseInteraction Instance => _instance;
    
    private Camera _camera;

    private AStarAgent _aStarAgent = new();
    
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        GameObject go = new GameObject("MouseInteraction");
        _instance = go.AddComponent<MouseInteraction>();
        _instance._camera = Camera.main;
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 vector2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            _aStarAgent.FindPath(vector2);
        }
    }
}