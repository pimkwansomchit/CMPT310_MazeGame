using UnityEngine;
using System.Collections.Generic;


public class AIBehaviour : MonoBehaviour
{

    public float moveSpeed = 5f;
    public int gridX;
    public int gridY;
    public Room[,] rooms;
    private List<Room> path;
    private int index =0;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool playerMoved = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPosition = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {

        if (path == null){
            return;
        }
        if (index >= path.Count){
            return;
        }

        // if (!playerMoved) return;
        // playerMoved = false;
        float distance = Vector2.Distance(transform.position, targetPosition);
        
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                index++;
                
                if (index < path.Count){
                    gridX = path[index].Index.x;
                    gridY = path[index].Index.y;
                }
                isMoving = false;
            }
            return;
        }

        if (!playerMoved) return;
        playerMoved = false;
        if (index < path.Count){
            targetPosition = path[index].transform.position;
            isMoving = true;
           // playerMoved = true;


        }

    }

    public void NotifyPlayerMoved()
    {
        playerMoved = true;
    }

    public void Init(Room[,] rooms, int startX, int startY){
        this.rooms = rooms;
        path = AStar.FindPath(rooms, new Vector2Int(startX, startY), new Vector2Int(rooms.GetLength(0) - 1, rooms.GetLength(1) - 1));

        // a star returned something
        if (path!= null){
            index =0;
            // start at beginning of path
            transform.position = path[0]. transform.position;
            gridX = path[0].Index.x;
            gridY = path[0].Index.y;
            targetPosition = path[index].transform.position;

            isMoving = false;



        }
        else {
            Debug.LogWarning("No path found for AI");
        }
    }

}
