using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingObject : MonoBehaviour
{
    public PathingGrid grid;

    Transform target;

    private List<Vector3> path = new List<Vector3>();

    List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos, string targetName)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        Node currentNode = startNode;

        if (targetNode == null) return null;

        targetNode.updateCounter++;

        if (targetNode.updateCounter >= grid.useableUpdateDelay || !grid.CheckIfWalkableNode(targetNode, name, targetName))
            grid.UpdateNodeWalkable(targetNode);

        if (!grid.CheckIfWalkableNode(targetNode, name, targetName))
        {
            if (targetNode.walkable != targetName)
            {
                foreach (Node neighbor in grid.GetNeighbors(currentNode, name, targetName))
                {
                    if (neighbor.walkable == targetName)
                    {
                        targetNode = neighbor;
                        break;
                    }
                }
            }

            return null;
        }

        Heap<Node> open = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closed = new HashSet<Node>();
        int count = 0;

        open.Add(startNode);
        startNode.distanceFromOrig = 0;

        while (open.Count > 0 && count++ < grid.MaxSize * 3)
        {
            currentNode = open.RemoveFirst();
            closed.Add(currentNode);

            if (targetNode == currentNode)
                return RetracePath(startNode, targetNode);
            
            foreach (Node neighbor in grid.GetNeighbors(currentNode, name, targetName))
            {
                if (!closed.Contains(neighbor))
                {
                    int newMovementCost = currentNode.distanceFromOrig + GetDistance(currentNode, neighbor);
                    bool openContainsNeighbor = open.Contains(neighbor);

                    if ((openContainsNeighbor && newMovementCost < neighbor.distanceFromOrig) || !openContainsNeighbor)
                    {
                        neighbor.distanceFromOrig = newMovementCost;
                        neighbor.distanceFromDest = GetDistance(neighbor, targetNode);
                        neighbor.previous = currentNode;
                    }

                    if (!openContainsNeighbor) open.Add(neighbor);
                }
            }
        }

        return RetracePath(startNode, currentNode);
    }

    /********************* Public Interaction Methods ********************/
    public Vector3 GetNextPathPosition(Transform t)
    {
        target = t ?? throw new System.Exception("no target for pathing was provided.");
        return GetNextPathPosition(t.position, t.name, 0);
    }
    
    public Vector3 GetNextPathPosition(Vector3 nextPosition, int stepsAhead)
        => GetNextPathPosition(nextPosition, "", stepsAhead);

    public Vector3 GetNextPathPosition(Vector3 nextPosition, string targetName, int stepsAhead)
    {
        path = FindPath(transform.position, nextPosition, targetName);

        if (path == null || path?.Count <= stepsAhead) return transform.position;

        Vector3 nextPathPosition = path[stepsAhead];

        if (Vector3.Distance(transform.position, nextPathPosition) <= (grid.nodeDensity * stepsAhead + grid.nodeDensity / 2) && path.Count > stepsAhead + 1)
            nextPathPosition = path[stepsAhead + 1];
        
        return nextPathPosition;
    }

    public bool IsPositionWalkable(Vector3 targetPosition) => grid.CheckIfWalkableNode(grid.NodeFromWorldPoint(targetPosition), name, "");

    public bool UpdateNextPositionWalkable()
    {
        bool result = IsPositionWalkable(path[0]);
        grid.UpdateNodeWalkable(grid.NodeFromWorldPoint(path[0]));

        return result;
    }

    public void Debug_CheckPathForWalkable(Vector3 startPosition)
    {
        if (path != null)
        {
            grid.Debug_DrawBox(grid.NodeFromWorldPoint(startPosition).position, Color.cyan, 1f);

            foreach (Vector3 pos in path)
            {
                if (grid.UpdateNodeWalkable(grid.NodeFromWorldPoint(pos)))
                    grid.Debug_DrawBox(pos, Color.green, 1f);
                else grid.Debug_DrawBox(pos, Color.red, 1f);
            }
        }
    }

    public Vector3 Debug_ResultOfNodeFromPoint(Vector3 worldPoint) => grid.NodeFromWorldPoint(worldPoint).position;

    /********************* Private Helper Methods ************************/
    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            //Debug.DrawRay(currentNode.position, Vector3.up, Color.green, 1f);
            currentNode = currentNode.previous;
        }

        path.Reverse();

        return path;
    }

    static int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstZ = Mathf.Abs(a.gridZ - b.gridZ);

        return dstX > dstZ ?
            14 * dstZ + 10 * (dstX - dstZ):
            14 * dstX + 10 * (dstZ - dstX);
    }
}
