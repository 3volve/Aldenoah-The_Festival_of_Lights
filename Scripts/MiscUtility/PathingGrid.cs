using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingGrid : MonoBehaviour
{
    public readonly static int MAX_DISTANCE_TO_FIND_NODE = 30;

    public float nodeDensity = 0.5f;
    public int extraSizeOnEdges = 25;
    public int MaxSize { get; private set; }
    public int useableUpdateDelay = 10;

    public Vector3 GetPosition { get { return transform.position - new Vector3(extraSizeOnEdges, 0, extraSizeOnEdges);} }

    private new Collider collider;
    private Node[,] grid;
    private int gridWidth;
    private int gridHeight;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        if (collider == null) collider = GetComponent<TerrainCollider>();

        gridWidth = Mathf.RoundToInt(collider.bounds.size.x / nodeDensity);
        gridHeight = Mathf.RoundToInt(collider.bounds.size.z / nodeDensity);

        MaxSize = gridWidth >= gridHeight ? gridWidth : gridHeight;

        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 newNodePosition = TranslatedTerrainPosition(x * nodeDensity, z * nodeDensity);

                if (!Physics.CheckBox(newNodePosition + Vector3.up, new Vector3(0.49f, 1, 0.49f), Quaternion.identity, LayerMask.GetMask("BuildingsBlocking"), QueryTriggerInteraction.Ignore))
                    grid[x, z] = new Node(newNodePosition, x, z);
            }
        }
    }

    /***************** Private Helper Methods ***************************************************/
    Vector3 TranslatedTerrainPosition(float xPos, float zPos)
    {
        return new Vector3(
            GetPosition.x + xPos,
            0, //collider.bounds.ClosestPoint(new Vector3(xPos, 10, zPos)).y, this was from when I was thinking about trying to have vertical movement
            GetPosition.z + zPos
        );
    }

    /****************** Public Methods *********************************************************/
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - GetPosition.x) / nodeDensity);
        int z = Mathf.RoundToInt((worldPosition.z - GetPosition.z) / nodeDensity);

        x = Mathf.Clamp(x, 0, gridWidth - 1);
        z = Mathf.Clamp(z, 0, gridHeight - 1);

        if (grid == null) return null;
        if (grid[x, z] != null)
            return grid[x, z];
        else
        {
            Node nonNullResult = grid[x, z];

            for (int i = 1; i < MAX_DISTANCE_TO_FIND_NODE && nonNullResult == null; i++)
            {
                if (x + i < gridWidth)
                {
                    nonNullResult = grid[x + i, z];
                    if (nonNullResult != null) break;
                }

                if (x - i >= 0)
                {
                    nonNullResult = grid[x - i, z];
                    if (nonNullResult != null) break;
                }

                if (z + i < gridHeight)
                {
                    nonNullResult = grid[x, z + i];
                    if (nonNullResult != null) break;
                }

                if (z - i >= 0)
                {
                    nonNullResult = grid[x, z - i];
                    if (nonNullResult != null) break;
                }
            }

            if (nonNullResult != null)
                nonNullResult.updateCounter++;

            return nonNullResult;
        }
    }

    public int IsBlockedOrInvalidNode(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - GetPosition.x) / nodeDensity);
        int z = Mathf.RoundToInt((worldPosition.z - GetPosition.z) / nodeDensity);

        if (x >= gridWidth || x < 0 || z >= gridHeight || z < 0)
            return -1;
        else if (grid[x, z] == null)
            return 0;
        else return 1;

    }

    public List<Node> GetNeighbors(Node node, string askingObject, string targetObject)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
                if (x != 0 || z != 0)
                {
                    int checkX = node.gridX + x;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridWidth && checkZ >= 0 && checkZ < gridHeight && grid[checkX, checkZ] != null)
                    {
                        grid[checkX, checkZ].updateCounter++;

                        if (grid[checkX, checkZ].updateCounter > useableUpdateDelay)
                            UpdateNodeWalkable(grid[checkX, checkZ]);

                        Vector3 halfExtents = new Vector3(0.26f, 0.25f, 0.26f);
                        if (CheckIfWalkableNode(grid[checkX, checkZ], askingObject, targetObject))
                            neighbors.Add(grid[checkX, checkZ]);
                    }
                }

        return neighbors;
    }

    public bool UpdateNodeWalkable(Node node)
    {
        Vector3 halfExtents = new Vector3(0.26f, 0.25f, 0.26f);
        bool result = true;

        if (Physics.BoxCast(
                node.position + Vector3.down * 0.5f,
                halfExtents,
                Vector3.up,
                out RaycastHit hit,
                Quaternion.identity,
                2,
                LayerMask.GetMask("Player", "Character"),
                QueryTriggerInteraction.Ignore
            ))
        {
            result = false;
            node.walkable = hit.collider.name;
        }
        else node.walkable = "true";

        node.updateCounter = 0;
        return result;
    }

    public bool CheckIfWalkableNode(Node node, string askingObject, string targetObject)
        =>  node.walkable == askingObject ||
            node.walkable == targetObject ||
            node.walkable == "true";

    // I might save this method to use for other projects... It's really nice for visualizing this stuff!
    public void Debug_DrawBox(Vector3 position, Color color, float duration) =>
        Debug_DrawBox(position + Vector3.down * 0.15f, new Vector3(0.26f, 0.25f, 0.26f), 2, color, duration);

    void Debug_DrawBox(Vector3 position, Vector3 halfExtents, float distance, Color color, float duration)
    {
        Vector3 startingPoint = new Vector3(position.x - halfExtents.x, position.y, position.z - halfExtents.z);
        Vector3 direction = Vector3.forward * halfExtents.z;
        halfExtents *= 2;

        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.up * distance;
        Debug.DrawRay(startingPoint, direction, color, duration);

        direction = Vector3.right * halfExtents.x;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.up * distance;
        Debug.DrawRay(startingPoint, direction, color, duration);

        direction = Vector3.back * halfExtents.z;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.up * distance;
        Debug.DrawRay(startingPoint, direction, color, duration);

        direction = Vector3.left * halfExtents.x;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.up * distance;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.forward * halfExtents.z;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.right * halfExtents.x;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.back * halfExtents.z;
        Debug.DrawRay(startingPoint, direction, color, duration);

        startingPoint += direction;
        direction = Vector3.left * halfExtents.x;
        Debug.DrawRay(startingPoint, direction, color, duration);
    }
    //
}
