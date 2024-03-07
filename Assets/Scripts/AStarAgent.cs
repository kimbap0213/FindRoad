using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAgent
{
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
    
    private PriorityQueue<AStarNode> _queue;
    
    Dictionary<Vector2Int, float> _dp;

    public void FindPath(Vector2 target)
    {
        _dp = new Dictionary<Vector2Int, float>();
        _queue = new PriorityQueue<AStarNode>(Comparer<AStarNode>.Create((x, y) =>
        {
            return -x.Sum.CompareTo(y.Sum);
        }));
        
        AStarNode start = new AStarNode(Vector2Int.zero, 0);
        _queue.Enqueue(start);
        _dp.Add(start.PositionInt, 0);
        
        CheckNode(target);
    }
    
    private Vector2Int[] _directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };
    
    private void CheckNode(Vector2 target)
    {
        AStarNode node = _queue.Dequeue();

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
            
            if (_dp.ContainsKey(position))
            {
                if(_dp[position] > newNode.Sum)
                {
                    _dp[position] = newNode.Sum;
                    _queue.Enqueue(newNode);
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
                _dp.Add(position, newNode.Sum);
                _queue.Enqueue(newNode);
            }
        }
        
        if (_queue.Count > 0)
            CheckNode(target);
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
