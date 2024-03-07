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

    private Vector2 _a = Vector2.zero;
    private Vector2 _b = Vector2.zero;
    
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
            _a = _camera.ScreenToWorldPoint(Input.mousePosition);
            CheckPath();
        }
        if(Input.GetMouseButtonDown(1))
        {
            _b = _camera.ScreenToWorldPoint(Input.mousePosition);
            CheckPath();
        }
    }
    
    void CheckPath()
    {
        if (_a != Vector2.zero && _b != Vector2.zero)
        {
            PathMapAgent.Instance.FindPath(_a, _b);
            Debug.Log("Path Found");
        }
    }
}