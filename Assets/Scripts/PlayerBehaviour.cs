using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int gridX;
    public int gridY;
    public int[,] maze;

    private bool isMoving = false;
    private Vector3 targetPosition;
    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        // move smoothly
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
        else if (Input.GetKeyDown(KeyCode.A))
        {
            x = -1;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            y = -1;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            x = 1;
        }

        // update position
        int newX = gridX + x;
        int newY = gridY + y;

        if (isValidMove(newX, newY))
        {
            gridX = newX;
            gridY = newY;
            targetPosition = new Vector3(gridX, 0, gridY);
            isMoving = true;
        }

    }   
    
    bool isValidMove(int x, int y)
    {
        // bounds check
        if (x < 0 || y < 0 || x >= maze.GetLength(0) || y >= maze.GetLength(1)) {
            return false;
        }

        // check if wall
        if (maze[x, y] == 1) {
            return false;
        }

        return true;
    }
}
