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
    
    private Vector2Int[] _directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };
    
    class AStarNode
    {
        public Vector2 Position;
        public Vector2Int PositionInt;
        public float Sum;
        public AStarNode Parent = null;
        
        public AStarNode(Vector2Int position, float sum)
        {
            PositionInt = position;
            Sum = sum;
            Position = new Vector2(position.x / 10f, position.y / 10f);
        }
        
        public AStarNode(Vector2Int position, float sum, AStarNode parent)
        {
            PositionInt = position;
            Sum = sum;
            Position = new Vector2(position.x / 10f, position.y / 10f);
            Parent = parent;
            
            OptimizeParent();
        }

        public AStarNode(Vector2 position, float sum, AStarNode parent)
        {
            PositionInt = new Vector2Int((int)position.x * 10, (int)position.y * 10);
            Sum = sum;
            Position = position;
            Parent = parent;
            
            OptimizeParent();
        }

        private void OptimizeParent()
        {
            if(Parent == null)
                return;

            if (Parent.Parent == null)
                return;
            
            float distance = Vector2.Distance(Position, Parent.Parent.Position);
            
            RaycastHit2D hit = Physics2D.Raycast(Position, Parent.Parent.Position - Position, distance);
            
            if (hit.collider != null)
                return;
            
            this.Sum -= Vector2.Distance(Position, Parent.Position);
            this.Sum += Vector2.Distance(Position, Parent.Parent.Position);
            
            Parent = Parent.Parent;
            
            OptimizeParent();
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 vector2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            PriorityQueue<AStarNode> queue = new (Comparer<AStarNode>.Create((x, y) =>
            {
                return -x.Sum.CompareTo(y.Sum);
            }));
            
            Dictionary<Vector2Int, float> dp = new();
            AStarNode start = new AStarNode(Vector2Int.zero, 0);
            
            queue.Enqueue(start);
            dp.Add(start.PositionInt, 0);
            
            CheckNode(ref queue, ref dp, vector2);
        }
    }

    private void CheckNode(ref PriorityQueue<AStarNode> queue, ref Dictionary<Vector2Int, float> dp, Vector2 target)
    {
        AStarNode node = queue.Dequeue();

        foreach (var vector in _directions)
        {
            Vector2Int position = node.PositionInt + vector;
            
            Vector2 floatPosition = new Vector2(position.x / 10f, position.y / 10f);

            AStarNode newNode = new AStarNode(position, node.Sum + 0.1f + GetSimpleSum(floatPosition, target), node);

            float preAbleToRay = GetAbleToRay(ref node, newNode.Position);
            
            if (preAbleToRay < 0)
            {
                continue;
            }
            
            if (dp.ContainsKey(position))
            {
                if(dp[position] > newNode.Sum)
                {
                    dp[position] = newNode.Sum;
                    queue.Enqueue(newNode);
                }
            }
            else
            {
                float ableToRay = GetAbleToRay(ref newNode, target);
                if (ableToRay > 0 && ableToRay < 1f)
                {
                    AStarNode endNode = new AStarNode(target, newNode.Sum + ableToRay, newNode);
                    DrawRay(endNode);
                    return;
                }
                dp.Add(position, newNode.Sum);
                queue.Enqueue(newNode);
            }
        }
        
        if (queue.Count > 0)
            CheckNode(ref queue, ref dp, target);
    }
    
    private void DrawRay(AStarNode node)
    {
        if (node.Parent == null)
            return;
        
        Debug.DrawRay(node.Position, node.Parent.Position - node.Position, Color.red, 1f);
        DrawRay(node.Parent);
    }
    
    private float GetSum(ref AStarNode node, Vector2 position)
    {
        return node.Sum + Vector2.Distance(node.Position, position);
    }
    
    private float GetSimpleSum(Vector2 position, Vector2 target)
    {
        return Vector2.Distance(position, target);
    }
    
    private float GetAbleToRay(ref AStarNode node, Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(node.Position, position - node.Position);
        
        if (hit.collider != null)
            return -1f;
        
        return Vector2.Distance(node.Position, position);
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