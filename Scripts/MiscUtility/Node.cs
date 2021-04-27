using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector3 position;
    public int gridX;
    public int gridZ;

    public int distanceFromDest = 0;
    public int distanceFromOrig = int.MaxValue;
    public int TotalDistanceDiff { get { return distanceFromOrig + distanceFromDest; } }

    public Node previous = null;
    public string walkable = "true";
    public int HeapIndex { get; set; }
    public int updateCounter = 0;

    public Node(Vector3 pos, int x, int z)
    {
        position = pos;
        gridX = x;
        gridZ = z;
    }

    public int CompareTo(Node n)
    {
        int compare = n.TotalDistanceDiff.CompareTo(TotalDistanceDiff);
        if (compare == 0) compare = n.distanceFromDest.CompareTo(distanceFromDest);
        return compare;
    }

    public override string ToString()
        => "position: (" + position.x + ", " + position.z + "), diff: " + TotalDistanceDiff + ", dest: " + distanceFromDest;
}
