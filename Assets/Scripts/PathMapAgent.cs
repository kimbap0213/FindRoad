using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EventSystems;
using UnityEngine;

[Serializable]
public class PathLink
{
    public float Cost;
    public int LinkIndex;
        
    public PathLink()
    {
        Cost = 0;
        LinkIndex = -1;
    }
        
    public PathLink(int link, float cost)
    {
        Cost = cost;
        LinkIndex = link;
    }
}

[Serializable]
public class PathNode
{
    public Vector2 Position;
    
    [HideInInspector]
    public PathLink[] Links;
        
    public PathNode()
    {
        Position = Vector2.zero;
    }
        
    public PathNode(Vector2 position)
    {
        Position = position;
    }

    public void SetSize(int size)
    {
        Links = new PathLink[size];
    }
}

public class PathMapAgent : MonoBehaviour
{
    private static PathMapAgent _instance;
    public static PathMapAgent Instance => _instance;
    
    private bool _isRunning = false;
    
    public List<PathNode> PathNodes = new List<PathNode>();
    public Vector2 position;
    public Vector2 nodeSize;
    public Vector2Int nodeCount;

    private void Awake()
    {
        _instance = this;
        _isRunning = true;
    }

    public void ResetNode()
    {
        PathNodes.Clear();
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
    
    public void GenerateNode()
    {
        PathNodes.Clear();
        
        for (int x = 0; x < nodeCount.x; x++)
        {
            for (int y = 0; y < nodeCount.y; y++)
            {
                Vector2 nodePosition = position + new Vector2(nodeSize.x * x, nodeSize.y * y);
                RaycastHit2D hit = Physics2D.Raycast(nodePosition, Vector2.zero);
                if (hit.collider == null)
                {
                    PathNode node = new PathNode(nodePosition);
                    PathNodes.Add(node);
                }
            }
        }

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    public void CalculatePath()
    {
        foreach (var node in PathNodes)
        {
            node.SetSize(PathNodes.Count);
        }
        
        for (int i = 0; i < PathNodes.Count; i++)
        {
            for (int j = 0; j < PathNodes.Count; j++)
            {
                if (i == j)
                    continue;
                
                float distance = Vector2.Distance(PathNodes[i].Position, PathNodes[j].Position);
                
                RaycastHit2D hit = Physics2D.Raycast(PathNodes[i].Position, PathNodes[j].Position - PathNodes[i].Position, distance);
                
                if (hit.collider == null)
                {
                    PathNodes[i].Links[j] = new PathLink(j, distance);
                }
            }
        }
        
        FloydWarshall();
        
        Debug.Log("Calculating Path");
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private void FloydWarshall()
    {
        int n = PathNodes.Count;
        for (int v = 0; v < n; v++)
        {
            for (int a = 0; a < n; a++)
            {
                for (int b = 0; b < n; b++)
                {
                    if (a == b || a == v || b == v)
                        continue;
                    
                    if (PathNodes[a].Links[b] == null)
                    {
                        if(PathNodes[a].Links[v] == null || PathNodes[v].Links[b] == null)
                            continue;
                        
                        float distance = PathNodes[a].Links[v].Cost + PathNodes[v].Links[b].Cost;
                        
                        PathNodes[a].Links[b] = new PathLink(PathNodes[a].Links[v].LinkIndex, distance);
                    }
                    else
                    {
                        if(PathNodes[a].Links[v] == null || PathNodes[v].Links[b] == null)
                            continue;
                        
                        float distance = PathNodes[a].Links[v].Cost + PathNodes[v].Links[b].Cost;
                        
                        if (distance < PathNodes[a].Links[b].Cost)
                        {
                            PathNodes[a].Links[b] = new PathLink(PathNodes[a].Links[v].LinkIndex, distance);
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_isRunning)
            return;
        
        Gizmos.color = Color.green;

        if (PathNodes.Count == 0)
        {
            DrawLayout();
        }
        else
        {
            DrawNode();
        }
    }

    private void DrawNode()
    {
        List<int> visited = new List<int>();
        
        int n = PathNodes.Count;

        for (int i = 0; i < n; i++)
        {
            PathNode node = PathNodes[i];
            
            Gizmos.DrawWireSphere(node.Position, 0.075f);
            
            if(node.Links == null)
                continue;
            
            foreach (var neighbor in node.Links)
            {
                if(neighbor == null)
                    continue;
                
                if(visited.Contains(neighbor.LinkIndex) || neighbor.LinkIndex == -1)
                    continue;
                
                Gizmos.DrawLine(node.Position, PathNodes[neighbor.LinkIndex].Position);
            }
            
            visited.Add(i);
        }
    }

    private void DrawLayout()
    {
        if(nodeSize.x <= 0 || nodeSize.y <= 0)
            return;
        
        if(nodeCount.x <= 0 || nodeCount.y <= 0)
            return;
        
        for (int x = 0; x < nodeCount.x; x++)
        {
            Vector2 pos = position + new Vector2(nodeSize.x * x, 0);
            Gizmos.DrawLine(pos, pos + new Vector2(0, nodeSize.y * (nodeCount.y - 1)));
        }

        for (int y = 0; y < nodeCount.y; y++)
        {
            Vector2 pos = position + new Vector2(0, nodeSize.y * y);
            Gizmos.DrawLine(pos, pos + new Vector2(nodeSize.x * (nodeCount.x - 1), 0));
        }
    }

    public void FindPath(Vector2 start, Vector2 end)
    {
        if(StartCheck(start, end))
            return;
        
        int n = PathNodes.Count;
        
        Vector2Int minPath = Vector2Int.zero;
        float minDistance = float.MaxValue;
        
        for(int i = 0; i < n; i++)
        {
            float distanceStart = Vector2.Distance(start, PathNodes[i].Position);
            
            RaycastHit2D hit = Physics2D.Raycast(start, PathNodes[i].Position - start, distanceStart);
            
            if (hit.collider != null)
                continue;
            
            for(int j = 0; j < n; j++)
            {
                if(PathNodes[i].Links[j] == null)
                    continue;
                
                float distanceEnd = Vector2.Distance(end, PathNodes[j].Position);
                
                RaycastHit2D hit2 = Physics2D.Raycast(PathNodes[j].Position, end - PathNodes[j].Position, distanceEnd);
                
                if (hit2.collider != null)
                    continue;
                
                float distance = distanceStart + PathNodes[i].Links[j].Cost + distanceEnd;
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPath = new Vector2Int(i, j);
                }
            }
        }
        
        Debug.DrawRay(PathNodes[minPath.x].Position, start - PathNodes[minPath.x].Position, Color.red, 1f);
        
        DrawPath(minPath.x, minPath.y, end);
    }

    private bool StartCheck(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        
        RaycastHit2D hit = Physics2D.Raycast(start, end - start, distance);

        if(hit.collider != null)
            return false;
        
        Debug.DrawRay(start, end - start, Color.red, 1f);
        
        return true;
    }

    private void DrawPath(int x, int y, Vector2 end)
    {
        if (x == y)
        {
            Debug.DrawRay(end, PathNodes[y].Position - end, Color.red, 1f);
            return;
        }
        
        int linkedNode = PathNodes[x].Links[y].LinkIndex;

        if (linkedNode == -1)
        {
            Debug.DrawRay(end, PathNodes[y].Position - end, Color.red, 1f);
            return;
        }
        
        Debug.DrawRay(PathNodes[x].Position, PathNodes[linkedNode].Position - PathNodes[x].Position, Color.red, 1f);
        
        DrawPath(linkedNode, y, end);
    }
}
