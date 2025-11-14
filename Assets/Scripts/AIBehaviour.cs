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
        if (index < path.Count){
            targetPosition = path[index].transform.position;
            isMoving = true;


        }

    }

    public void Init(Room[,] rooms, int startX, int startY){
        this.rooms = rooms;
        path = AStar.FindPath(rooms, new Vector2Int(startX, startY), new Vector2Int(rooms.GetLength(0) - 1, rooms.GetLength(1) - 1));

        // a star returned something
        if (path!= null){
            index =0;
            // start at beginning of path
            transform.position = path[0]. transform.position;
            targetPosition = path[index].transform.position;

            isMoving = true;



        }
        else {
            Debug.LogWarning("No path found for AI");
        }
    }

}
