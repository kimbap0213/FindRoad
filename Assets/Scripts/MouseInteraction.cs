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
        
        PriorityQueue<int> queue = new (Comparer<int>.Create((x, y) =>
        {
            return x.CompareTo(y);
        }));
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
            Debug.DrawLine(node.Position, node.Parent.Position, Color.red, 2);
            Draw(node.Parent, dest);
        }
    }
}

public class PriorityQueue<T>
{
    private List<T> _data;
    private IComparer<T> _comparer;
    public T Peek() => _data[0];
    public int Count => _data.Count;
    
    public PriorityQueue(IComparer<T> comparer)
    {
        _data = new List<T>();
        _comparer = comparer;
    }
    
    public void Enqueue(T item)
    {
        _data.Add(item);
        EnqueueSort(_data.Count);
    }
    
    private void EnqueueSort(int idx)
    {
        if (idx <= 1)
            return;
        
        int compare = _comparer.Compare(GetValueAt(idx), GetValueAt(idx / 2));

        if (compare <= 0)
            return;
        
        Swap(idx, idx / 2);
        
        EnqueueSort(idx / 2);
    }
    
    public T Dequeue()
    {
        T item = _data[0];
        
        _data[0] = _data[_data.Count - 1];
        
        _data.RemoveAt(_data.Count - 1);
        
        DequeueSort(1);
        
        return item;
    }
    
    private void DequeueSort(int idx)
    {
        if (idx * 2 > _data.Count)
            return;
        
        if(idx * 2 + 1 > _data.Count)
        {
            int compare = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2));
            if (compare < 0)
                Swap(idx, idx * 2);
            return;
        }
        
        int compare1 = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2));
        int compare2 = _comparer.Compare(GetValueAt(idx), GetValueAt(idx * 2 + 1));
        
        if (compare1 > 0 && compare2 > 0)
            return;
        
        int swapCompare = _comparer.Compare(GetValueAt(idx * 2), GetValueAt(idx * 2 + 1));
        
        int swapIdx = swapCompare > 0 ? idx * 2 : idx * 2 + 1;
        
        Swap(idx, swapIdx);
        
        DequeueSort(swapIdx);
    }
    
    private void Swap(int idx1, int idx2)
    {
        (_data[idx1 - 1], _data[idx2 - 1]) = (_data[idx2 - 1], _data[idx1 - 1]);
    }
    
    private T GetValueAt(int idx) => _data[idx - 1];
    
}