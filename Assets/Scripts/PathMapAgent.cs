using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EventSystems;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    public void ResetLink()
    {
        Links = null;
    }
}

public class PathMapAgent : MonoBehaviour
{
    private static PathMapAgent _instance;
    public static PathMapAgent Instance => _instance;
    
    private bool _isRunning = false;
    
    [SerializeField] private List<PathNode> pathNodes = new List<PathNode>();
    [SerializeField] private Vector2 position;
    [SerializeField] private Vector2 nodeSize;
    [SerializeField] private Vector2Int nodeCount;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private bool shouldDrawGrid;
    [SerializeField] private bool shouldDrawNode;
    [SerializeField] private bool shouldDrawLine;
    [SerializeField] private bool shouldDrawAtRuntime;

    private void Awake()
    {
        _instance = this;
        _isRunning = true;
    }

    public void ResetNode()
    {
        pathNodes.Clear();
        
        Debug.Log("Reset Node");
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
    
    public void GenerateNode()
    {
        pathNodes.Clear();
        
        for (int x = 0; x < nodeCount.x; x++)
        {
            for (int y = 0; y < nodeCount.y; y++)
            {
                Vector2 nodePosition = position + new Vector2(nodeSize.x * x, nodeSize.y * y);
                RaycastHit2D hit = Physics2D.Raycast(nodePosition, Vector2.zero, 1, layerMask);
                if (hit.collider == null)
                {
                    PathNode node = new PathNode(nodePosition);
                    pathNodes.Add(node);
                }
            }
        }
        
        Debug.Log("Node Generated");

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    public void ResetLine()
    {
        foreach (var node in pathNodes)
        {
            node.ResetLink();
        }
        
        Debug.Log("Reset Node");
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    public void CalculateLine()
    {
        foreach (var node in pathNodes)
        {
            node.SetSize(pathNodes.Count);
        }
        
        for (int i = 0; i < pathNodes.Count; i++)
        {
            for (int j = 0; j < pathNodes.Count; j++)
            {
                if (i == j)
                    continue;
                
                float distance = Vector2.Distance(pathNodes[i].Position, pathNodes[j].Position);
                
                RaycastHit2D hit = Physics2D.Raycast(pathNodes[i].Position, pathNodes[j].Position - pathNodes[i].Position, distance, layerMask);
                
                if (hit.collider == null)
                {
                    pathNodes[i].Links[j] = new PathLink(j, distance);
                }
            }
        }
        
        FloydWarshall();
        
        Debug.Log("Path Calculated");
        
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private void FloydWarshall()
    {
        int n = pathNodes.Count;
        for (int v = 0; v < n; v++)
        {
            for (int a = 0; a < n; a++)
            {
                for (int b = 0; b < n; b++)
                {
                    if (a == b || a == v || b == v)
                        continue;
                    
                    if (pathNodes[a].Links[b] == null)
                    {
                        if(pathNodes[a].Links[v] == null || pathNodes[v].Links[b] == null)
                            continue;
                        
                        float distance = pathNodes[a].Links[v].Cost + pathNodes[v].Links[b].Cost;
                        
                        pathNodes[a].Links[b] = new PathLink(pathNodes[a].Links[v].LinkIndex, distance);
                    }
                    else
                    {
                        if(pathNodes[a].Links[v] == null || pathNodes[v].Links[b] == null)
                            continue;
                        
                        float distance = pathNodes[a].Links[v].Cost + pathNodes[v].Links[b].Cost;
                        
                        if (distance < pathNodes[a].Links[b].Cost)
                        {
                            pathNodes[a].Links[b] = new PathLink(pathNodes[a].Links[v].LinkIndex, distance);
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_isRunning && !shouldDrawAtRuntime)
            return;
        
        Gizmos.color = Color.green;

        if (pathNodes.Count == 0 || (shouldDrawGrid && !shouldDrawNode && !shouldDrawLine))
        {
            DrawGrid();
        }
        else
        {
            DrawNode();
        }
    }

    private void DrawNode()
    {
        if(!shouldDrawNode && !shouldDrawLine)
            return;
        
        List<int> visited = new List<int>();
        
        int n = pathNodes.Count;

        for (int i = 0; i < n; i++)
        {
            PathNode node = pathNodes[i];
            
            if(shouldDrawNode)
                Gizmos.DrawWireSphere(node.Position, 0.075f);
            
            if(!shouldDrawLine)
                continue;
            
            if(node.Links == null)
                continue;
            
            foreach (var neighbor in node.Links)
            {
                if(neighbor == null)
                    continue;
                
                if(visited.Contains(neighbor.LinkIndex) || neighbor.LinkIndex == -1)
                    continue;
                
                Gizmos.DrawLine(node.Position, pathNodes[neighbor.LinkIndex].Position);
            }
            
            visited.Add(i);
        }
    }

    private void DrawGrid()
    {
        if (!shouldDrawGrid)
            return;
        
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
        
        int n = pathNodes.Count;
        
        Vector2Int minPath = Vector2Int.zero;
        float minDistance = float.MaxValue;
        
        for(int i = 0; i < n; i++)
        {
            float distanceStart = Vector2.Distance(start, pathNodes[i].Position);
            
            RaycastHit2D hit = Physics2D.Raycast(start, pathNodes[i].Position - start, distanceStart, layerMask);
            
            if (hit.collider != null)
                continue;
            
            for(int j = 0; j < n; j++)
            {
                if(pathNodes[i].Links[j] == null)
                    continue;
                
                float distanceEnd = Vector2.Distance(end, pathNodes[j].Position);
                
                RaycastHit2D hit2 = Physics2D.Raycast(pathNodes[j].Position, end - pathNodes[j].Position, distanceEnd, layerMask);
                
                if (hit2.collider != null)
                    continue;
                
                float distance = distanceStart + pathNodes[i].Links[j].Cost + distanceEnd;
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPath = new Vector2Int(i, j);
                }
            }
        }
        
        Debug.DrawRay(pathNodes[minPath.x].Position, start - pathNodes[minPath.x].Position, Color.red, 1f);
        
        DrawPath(minPath.x, minPath.y, end);
    }

    private bool StartCheck(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        
        RaycastHit2D hit = Physics2D.Raycast(start, end - start, distance, layerMask);

        if(hit.collider != null)
            return false;
        
        Debug.DrawRay(start, end - start, Color.red, 1f);
        
        return true;
    }

    private void DrawPath(int x, int y, Vector2 end)
    {
        if (x == y)
        {
            Debug.DrawRay(end, pathNodes[y].Position - end, Color.red, 1f);
            return;
        }
        
        int linkedNode = pathNodes[x].Links[y].LinkIndex;

        if (linkedNode == -1)
        {
            Debug.DrawRay(end, pathNodes[y].Position - end, Color.red, 1f);
            return;
        }
        
        Debug.DrawRay(pathNodes[x].Position, pathNodes[linkedNode].Position - pathNodes[x].Position, Color.red, 1f);
        
        DrawPath(linkedNode, y, end);
    }
}
