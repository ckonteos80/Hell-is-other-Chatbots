using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class WaypointController : MonoBehaviour
{
    public MovementController myMovementController;
    public GameObject hitObject;

    void Start()
    {
        myMovementController = GetComponent<MovementController>();
    }

    public void SetTarget(Vector2 targetDest)
    {
        Vector2 startPoint = transform.position;
        Vector2 endPoint = targetDest;

        LayerMask mask = LayerMask.GetMask("Physical");
        RaycastHit2D hit = Physics2D.Linecast(startPoint, endPoint, mask);

        Debug.DrawLine(startPoint, endPoint, Color.red, 2f);

        if (hit.collider != null)
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);

            ObjectBlocking objectBlock = hit.collider.GetComponent<ObjectBlocking>();
            if (objectBlock != null)
            {
                hitObject = hit.collider.gameObject;
                Debug.Log("Hit an object with ObjectBlocking script: " + hit.collider.name);

                // Collect waypoints around the obstacle
                List<Vector2> TravelPoints = new List<Vector2>();
                TravelPoints.Add(startPoint); // Add start point first

                // Add all horizontal and vertical waypoints
                foreach (Transform t in objectBlock.waypointsHorizontal)
                {
                    TravelPoints.Add(t.position);
                }
                foreach (Transform t in objectBlock.waypointsVertical)
                {
                    TravelPoints.Add(t.position);
                }

                TravelPoints.Add(endPoint); // Add end point

                // Use A* pathfinding to find the optimal path
                List<Vector2> optimalPath = AStarPathfinding(startPoint, endPoint, TravelPoints);

                // Send the optimized path to the movement controller
                myMovementController.MoveToPos.Clear();
                myMovementController.MoveToPos.AddRange(optimalPath);
            }
            else
            {
                myMovementController.MoveToPos.Clear();
                myMovementController.MoveToPos.Add(targetDest); // Direct path if no obstacles
                hitObject = null;
            }
        }
        else
        {
            myMovementController.MoveToPos.Clear();
            myMovementController.MoveToPos.Add(targetDest); // Direct path if no obstacles
            hitObject = null;
        }
    }

    // A* Pathfinding algorithm to find the optimal path
    private List<Vector2> AStarPathfinding(Vector2 start, Vector2 end, List<Vector2> waypoints)
    {
        // Open and closed lists for A* algorithm
        List<Vector2> openList = new List<Vector2>();
        HashSet<Vector2> closedList = new HashSet<Vector2>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();

        // g-cost and h-cost for each waypoint
        Dictionary<Vector2, float> gCost = new Dictionary<Vector2, float>();
        Dictionary<Vector2, float> hCost = new Dictionary<Vector2, float>();
        Dictionary<Vector2, float> fCost = new Dictionary<Vector2, float>();

        openList.Add(start);
        gCost[start] = 0;
        hCost[start] = Vector2.Distance(start, end);
        fCost[start] = gCost[start] + hCost[start];

        while (openList.Count > 0)
        {
            // Get the node with the lowest f-cost
            Vector2 current = GetLowestFcostNode(openList, fCost);

            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            // Check neighbors
            foreach (Vector2 neighbor in waypoints)
            {
                if (closedList.Contains(neighbor) || IsBlocked(current, neighbor))
                    continue;

                float tentativeGCost = gCost[current] + Vector2.Distance(current, neighbor);

                if (!openList.Contains(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    hCost[neighbor] = Vector2.Distance(neighbor, end);
                    fCost[neighbor] = gCost[neighbor] + hCost[neighbor];

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2>(); // Return an empty path if no path is found
    }

    // Get the node with the lowest f-cost from the open list
    private Vector2 GetLowestFcostNode(List<Vector2> openList, Dictionary<Vector2, float> fCost)
    {
        Vector2 lowestFcostNode = openList[0];
        float lowestFcost = fCost[lowestFcostNode];

        foreach (Vector2 node in openList)
        {
            if (fCost[node] < lowestFcost)
            {
                lowestFcostNode = node;
                lowestFcost = fCost[node];
            }
        }

        return lowestFcostNode;
    }

    // Reconstruct the path from the cameFrom dictionary
    private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    // Check if the path between two points is blocked
    private bool IsBlocked(Vector2 startPoint, Vector2 endPoint)
    {
        LayerMask mask = LayerMask.GetMask("Physical");
        RaycastHit2D hit = Physics2D.Linecast(startPoint, endPoint, mask);

        if (hit.collider != null)
        {
            Debug.Log($"Blocked path at: {hit.collider.gameObject.name}");
            return true;
        }

        return false;
    }


}
