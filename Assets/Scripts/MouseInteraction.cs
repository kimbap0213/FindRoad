using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInteraction : MonoBehaviour
{
    private static MouseInteraction _instance;
    
    public static MouseInteraction Instance => _instance;
    
    private Camera _camera;
    
    class AStarNode
    {
        public Vector2 Position;
        public float Sum;
        public AStarNode Parent = null;
        
        public AStarNode(Vector2 position, float sum)
        {
            Position = position;
            Sum = sum;
        }
        
        public AStarNode(Vector2 position, float sum, AStarNode parent)
        {
            Position = position;
            Sum = sum;
            Parent = parent;
        }
    }
    
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        GameObject go = new GameObject("MouseInteraction");
        _instance = go.AddComponent<MouseInteraction>();
        _instance._camera = Camera.main;
    }
    
    void Update()
    {
        Vector2 vector2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            List<AStarNode> nodes = new List<AStarNode>();
            nodes.Add(new AStarNode(Vector2.zero, 0));
            AnalyseRay(ref nodes, vector2);
            nodes.Clear();
        }
    }
    
    private void AnalyseRay(ref List<AStarNode> nodes, Vector2 dest)
    {
        AStarNode minNode = null;
        float minSum = float.MaxValue;
        
        foreach (var node in nodes)
        {
            float sum = Vector2.Distance(node.Position, dest) + node.Sum;
            if (sum < minSum)
            {
                minSum = sum;
                minNode = node;
            }
        }
        
        if (minNode == null)
            return;

        if (minNode.Sum > 10)
        {
            Debug.Log("Sum > 10");
            Draw(minNode, dest);
            return;
        }
        
        nodes.Remove(minNode);
        
        RaycastHit2D hit = Physics2D.Raycast(minNode.Position, dest - minNode.Position);
        
        if (hit.collider == null)
        {
            AStarNode node = new AStarNode(dest, minNode.Sum + Vector2.Distance(minNode.Position, dest), minNode);
            Draw(node, dest);
            return;
        }
        
        SpreadNode(ref nodes, ref minNode);
        
        AnalyseRay(ref nodes, dest);
    }

    void SpreadNode(ref List<AStarNode> nodes, ref AStarNode node)
    {
        AddNode(ref nodes, ref node, node.Position + Vector2.up * 0.2f, node.Sum + 0.2f);
        AddNode(ref nodes, ref node, node.Position + Vector2.down * 0.2f, node.Sum + 0.2f);
        AddNode(ref nodes, ref node, node.Position + Vector2.right * 0.2f, node.Sum + 0.2f);
        AddNode(ref nodes, ref node, node.Position + Vector2.left * 0.2f, node.Sum + 0.2f);
    }
    
    void AddNode(ref List<AStarNode> nodes, ref AStarNode node, Vector2 newVector, float sum)
    {
        if (Physics2D.Raycast(node.Position, newVector - node.Position).collider == null)
        {
            AStarNode newNode = new AStarNode(newVector, sum, node);
            CalcCost(ref newNode);
            nodes.Add(newNode);
            
        }
    }
    
    void CalcCost(ref AStarNode node)
    {
        if (node.Parent != null)
        {
            if(node.Parent.Parent == null)
                return;
            
            if (Physics2D.Raycast(node.Position, node.Parent.Parent.Position - node.Position).collider == null)
            {
                node.Sum = Vector2.Distance(node.Position, node.Parent.Position) + node.Parent.Parent.Sum;
                node.Parent = node.Parent.Parent;
            }
        }
    }

    void Draw(AStarNode node, Vector2 dest)
    {
        Debug.Log("Draw");
        if (node.Parent != null)
        {
            Debug.Log(node.Position + " " + node.Parent.Position + " " + dest);
            //Debug.DrawRay(node.Position, node.Position - node.Parent.Position, Color.green, 2);
            Debug.DrawLine(node.Position, node.Parent.Position, Color.red, 2);
            Draw(node.Parent, dest);
        }
    }
}
