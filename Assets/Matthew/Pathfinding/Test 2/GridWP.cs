using System.Collections.Generic;
using UnityEngine;

public class GridWP : MonoBehaviour
{
    public class Node
    {
        private int depth;
        private bool walkable;

        private GameObject waypoint = new GameObject();
        private List<Node> neighbours = new List<Node>();
        public int Depth { get => depth; set => depth = value; }
        public bool Walkable { get => walkable; set => walkable = value; }

        public GameObject Waypoint { get => waypoint; set => waypoint = value; }
        public List<Node> Neighbours { get => neighbours; set => neighbours = value; }

        public Node()
        {
            this.depth = -1;
            this.walkable = true;
        }

        public Node(bool walkable)
        {
            this.depth = -1;
            this.walkable = walkable;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null) return false;

            Node n = obj as Node;

            if ((System.Object)n == null)
            {
                return false;
            }

            if (this.waypoint.transform.position.x == n.Waypoint.transform.position.x &&
                this.waypoint.transform.position.z == n.Waypoint.transform.position.z)
            {
                return true;
            }

            return false;
        }
    }

    public Node[,] grid;
    List<Node> path = new List<Node>();
    int curNode = 0;

    public GameObject prefabWaypoint;
    public Material goalMat;
    public Material wallMat;

    Vector3 goal;
    float speed = 4.0f;
    float accuracy = 0.5f;
    float rotSpeed = 4f;

    int spacing = 5;

    Node startNode;
    Node endNode;

    List<Node> getAdjacentNodes(Node[,] m, int i, int j)
    {
        List<Node> list = new List<Node>();

        // Node Up
        if (i - 1 >= 0)
        {
            if (m[i - 1, j].Walkable)
                list.Add(m[i - 1, j]);
        }

        // Node Down
        if (i + 1 < m.GetLength(0))
        {
            if (m[i + 1, j].Walkable)
                list.Add(m[i + 1, j]);
        }

        // Node Left
        if (j - 1 >= 0)
        {
            if (m[i, j - 1].Walkable)
                list.Add(m[i, j - 1]);
        }

        // Node Right
        if (j + 1 < m.GetLength(1))
        {
            if (m[i, j + 1].Walkable)
                list.Add(m[i, j + 1]);
        }

        return list;
    }

    List<Node> BFS(Node start, Node end)
    {
        Queue<Node> toVisit = new Queue<Node>();
        List<Node> visited = new List<Node>();

        Node currentNode = start;
        currentNode.Depth = 0;
        toVisit.Enqueue(currentNode);

        List<Node> finalPath = new List<Node>();

        while (toVisit.Count > 0)
        {
            currentNode = toVisit.Dequeue();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            if (currentNode.Equals(end))
            {
                while (currentNode.Depth != 0)
                {
                    foreach (Node n in currentNode.Neighbours)
                    {
                        if (n.Depth == currentNode.Depth - 1)
                        {
                            finalPath.Add(currentNode);
                            currentNode = n;
                            break;
                        }
                    }
                }
                finalPath.Reverse();
                break;
            }
            else
            {
                foreach (Node n in currentNode.Neighbours)
                {
                    if (!visited.Contains(n) && n.Walkable)
                    {
                        n.Depth = currentNode.Depth + 1;
                        toVisit.Enqueue(n);
                    }
                }
            }
        }
        return finalPath;
    }

    void Start()
    {
        // Create grid
        grid = new Node[,]
        {
           { new Node(), new Node(false), new Node(), new Node(), new Node(), new Node() },
           { new Node(), new Node(false), new Node(), new Node(false), new Node(false), new Node() },
           { new Node(), new Node(false), new Node(), new Node(), new Node(), new Node() },
           { new Node(), new Node(false), new Node(), new Node(false), new Node(), new Node() },
           { new Node(), new Node(false), new Node(), new Node(false), new Node(false), new Node() },
           { new Node(), new Node(false), new Node(), new Node(false), new Node(), new Node() },
           { new Node(), new Node(), new Node(), new Node(false), new Node(), new Node() }
        };

        // Initialise grid points
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j].Waypoint = Instantiate(prefabWaypoint, new Vector3(i * spacing, this.transform.position.y, j * spacing), Quaternion.identity);

                if (!grid[i, j].Walkable)
                    grid[i, j].Waypoint.GetComponent<Renderer>().material = wallMat;
                else
                    grid[i, j].Neighbours = getAdjacentNodes(grid, i, j);
            }
        }

        startNode = grid[0, 0];
        endNode = grid[6, 5];
        startNode.Walkable = true;
        endNode.Walkable = true;
        endNode.Waypoint.GetComponent<Renderer>().material = goalMat;

        this.transform.position = new Vector3(startNode.Waypoint.transform.position.x, this.transform.position.y, startNode.Waypoint.transform.position.z);
    }

    void LateUpdate()
    {
        // Calculate shortest path when the return key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            this.transform.position = new Vector3(startNode.Waypoint.transform.position.x, this.transform.position.y, startNode.Waypoint.transform.position.z);
            curNode = 0;
            path = BFS(startNode, endNode);
        }
        // If there's no path
        if (path.Count == 0) return;

        // set the goal position 
        goal = new Vector3(path[curNode].Waypoint.transform.position.x, this.transform.position.y, path[curNode].Waypoint.transform.position.z);

        // set the direction
        Vector3 direction = goal - this.transform.position;

        // Move towards the goal or increase the counter to set another goal in the next iteration.
        if (direction.magnitude > accuracy)
        {
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
            this.transform.Translate(0, 0, speed * Time.deltaTime);
        }
        else
        {
            if (curNode < path.Count - 1)
                curNode++;
        }
    }
}
