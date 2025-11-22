using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int gridX;
    public int gridY;
    public Room[,] rooms;

    public GenerateMaze mazegen;

    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (rooms == null) return;

        // smooth movement
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
            return;
        }

        // handle user input
        int x = 0;
        int y = 0;

        if (Input.GetKeyDown(KeyCode.W))
        {
            y = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            y = -1;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            x = -1;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            x = 1;
        }

        int newX = gridX + x;
        int newY = gridY + y;

        if (isValidMove(newX, newY, x, y))
        {
            gridX = newX;
            gridY = newY;
            targetPosition = rooms[gridX, gridY].transform.position;
            isMoving = true;
            UnityEngine.Object.FindFirstObjectByType<RLBehaviour>()?.NotifyPlayerMoved();
            UnityEngine.Object.FindFirstObjectByType<AIBehaviour>()?.NotifyPlayerMoved();
            if (mazegen != null)
                mazegen.OnPlayerMoved();
        }
    }

    bool isValidMove(int x, int y, int dx, int dy)
    {
        // bounds check
        if (x < 0 || y < 0 || x >= rooms.GetLength(0) || y >= rooms.GetLength(1))
        {
            return false;
        }

        // direction check (walls)
        Room.Directions dir = Room.Directions.NONE;

        if (dx == 1) dir = Room.Directions.RIGHT;
        else if (dx == -1) dir = Room.Directions.LEFT;
        else if (dy == 1) dir = Room.Directions.TOP;
        else if (dy == -1) dir = Room.Directions.BOTTOM;

        if (dir == Room.Directions.NONE)
        {
            return false;
        }

        // check if wall blocks movement
        if (rooms[gridX, gridY].dirflags[dir])
        {
            return false;
        }

        return true;
    }

    public void Init(Room[,] rooms, int startX, int startY)
    {
        this.rooms = rooms;
        this.gridX = startX;
        this.gridY = startY;
        targetPosition = transform.position;

        mazegen = FindFirstObjectByType<GenerateMaze>();
    }
}
