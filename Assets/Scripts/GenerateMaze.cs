using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    GameObject AIPrefab;

    [SerializeField]
    GameObject RLPrefab;

    private GameObject aiObject;
    private AIBehaviour ai;
    private GameObject rlObject;

    private PlayerBehaviour player;
    private RLBehaviour rl;


    // The grid.
    Room[,] rooms = null;

    [SerializeField]
    int numX = 10;
    [SerializeField]
    int numY = 10;

    // The room width and height.
    float roomWidth;
    float roomHeight;

    // The stack for backtracking.
    Stack<Room> stack = new Stack<Room>();

    bool generating = false;

    //track if game running
    private bool gameActive = false;

    // Score tracking
    public int playerScore = 0;
    public int aiScore = 0;

    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers =
          roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(
              minBounds,
              ren.bounds.min);

            maxBounds = Vector3.Max(
              maxBounds,
              ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }


    private void SetCamera()
    {
        Camera.main.transform.position = new Vector3(
          numX * (roomWidth - 1) / 2,
          numY * (roomHeight - 1) / 2,
          -100.0f);

        float min_value = Mathf.Min(numX * (roomWidth - 1), numY * (roomHeight - 1));
        Camera.main.orthographicSize = min_value * 0.75f;
    }

    private void Start()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        GetRoomSize();

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                GameObject room = Instantiate(roomPrefab,
                  new Vector3(i * roomWidth, j * roomHeight, 0.0f),
                  Quaternion.identity);

                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }

        SetCamera();

        // Automatically generate maze on start after rooms are initialized
        StartCoroutine(InitializeMaze());
    }

    IEnumerator InitializeMaze()
    {
        // Wait one frame to ensure all Room components are initialized
        yield return null;
        CreateMaze();
    }

    private void RemoveRoomWall(
      int x,
      int y,
      Room.Directions dir)
    {
        if (dir != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(dir, false);
        }

        Room.Directions opp = Room.Directions.NONE;
        switch (dir)
        {
            case Room.Directions.TOP:
                if (y < numY - 1)
                {
                    opp = Room.Directions.BOTTOM;
                    ++y;
                }
                break;
            case Room.Directions.RIGHT:
                if (x < numX - 1)
                {
                    opp = Room.Directions.LEFT;
                    ++x;
                }
                break;
            case Room.Directions.BOTTOM:
                if (y > 0)
                {
                    opp = Room.Directions.TOP;
                    --y;
                }
                break;
            case Room.Directions.LEFT:
                if (x > 0)
                {
                    opp = Room.Directions.RIGHT;
                    --x;
                }
                break;
        }
        if (opp != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(opp, false);
        }
    }

    public List<Tuple<Room.Directions, Room>> GetNeighboursNotVisited(
      int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbours =
          new List<Tuple<Room.Directions, Room>>();

        foreach (Room.Directions dir in Enum.GetValues(
          typeof(Room.Directions)))
        {
            int x = cx;
            int y = cy;

            switch (dir)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                    {
                        ++y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(
                              Room.Directions.TOP,
                              rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                    {
                        ++x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(
                              Room.Directions.RIGHT,
                              rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                    {
                        --y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(
                              Room.Directions.BOTTOM,
                              rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                    {
                        --x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(
                              Room.Directions.LEFT,
                              rooms[x, y]));
                        }
                    }
                    break;
            }
        }
        return neighbours;
    }

    private bool GenerateStep()
    {
        if (stack.Count == 0) return true;

        Room r = stack.Peek();
        var neighbours = GetNeighboursNotVisited(r.Index.x, r.Index.y);

        if (neighbours.Count != 0)
        {
            var index = 0;
            if (neighbours.Count > 1)
            {
                index = UnityEngine.Random.Range(0, neighbours.Count);
            }

            var item = neighbours[index];
            Room neighbour = item.Item2;
            neighbour.visited = true;
            RemoveRoomWall(r.Index.x, r.Index.y, item.Item1);

            stack.Push(neighbour);
        }
        else
        {
            stack.Pop();
        }

        return false;
    }

    public void CreateMaze()
    {
        if (generating) return;

        // Destroy old player if exists
        if (player != null && player.gameObject != null)
        {
            Destroy(player.gameObject);
        }

        Reset();

        RemoveRoomWall(0, 0, Room.Directions.BOTTOM);
        // Exit in middle of right wall
        int middleY = numY / 2;
        RemoveRoomWall(numX - 1, middleY, Room.Directions.RIGHT);

        stack.Push(rooms[0, 0]);

        StartCoroutine(Coroutine_Generate());
    }


    IEnumerator Coroutine_Generate()
    {
        generating = true;

        if (rlObject != null)
            rlObject.SetActive(false);

        bool flag = false;
        while (!flag)
        {
            flag = GenerateStep();
            yield return new WaitForSeconds(0.05f);
        }

        generating = false;

        Vector3 spawnPos = new Vector3(rooms[0, 0].transform.position.x, rooms[0, 0].transform.position.y, -1f);
        GameObject p = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player = p.GetComponent<PlayerBehaviour>();

        player.Init(rooms, 0, 0);

        player.mazegen = this;

        //

        // if (aiObject!= null){
        //     Destroy(aiObject);
        // }
        Vector2Int aiStart = new Vector2Int(0, numY - 1);
        Vector3 aiPosition = rooms[aiStart.x, aiStart.y].transform.position;
        aiObject = Instantiate(AIPrefab, aiPosition, Quaternion.identity);
        ai = aiObject.GetComponent<AIBehaviour>();
        ai.Init(rooms, aiStart.x, aiStart.y);


        //spawn rl only if it doesn't exit yet
        int exitY = numY / 2;
        if (rl == null || rlObject == null)
        {
            Vector2Int rlStart = new Vector2Int(numX - 1, 0); // start in bottom right corner
            Vector3 rlPosition = rooms[rlStart.x, rlStart.y].transform.position;
            rlObject = Instantiate(RLPrefab, rlPosition, Quaternion.identity);
            //RLBehaviour rl = rlObject.GetComponent<RLBehaviour>();
            rl = rlObject.GetComponent<RLBehaviour>();
            rl.Init(rooms, rlStart.x, rlStart.y, numX - 1, exitY);
        }
        else
        {
            rl.rooms = rooms;
            rl.Init(rooms, numX - 1, 0, numX - 1, exitY);
        }

        rlObject.SetActive(true);
        rl.ResetEpisode();
        gameActive = true;


    }

    public void OnPlayerMoved()
    {
        if (!gameActive) return;

        if (ai != null)
            ai.NotifyPlayerMoved();
        if (rl != null)
            rl.NotifyPlayerMoved();
        CheckGameEnd();
    }

    public void OnAIMoved()
    {
        if (!gameActive) return;
        CheckGameEnd();
    }

    public void CheckGameEnd()
    {
        if (player == null || rl == null || ai == null) return;

        if (!gameActive) return;

        int exitY = numY / 2;

        //if rl catch the player
        if (rl.gridX == player.gridX && rl.gridY == player.gridY)
        {
            Debug.Log("RL agent caught player - Player loses a point");
            playerScore--;
            gameActive = false;
            StartCoroutine(RestartGame());
            return;
        }

        //if AI reaches the exit
        if (ai.gridX == numX - 1 && ai.gridY == exitY)
        {
            Debug.Log("AI reached exit - AI scores a point");
            aiScore++;
            gameActive = false;
            StartCoroutine(RestartGame());
            return;
        }

        //if player reach the exit
        if (player.gridX == numX - 1 && player.gridY == exitY)
        {
            Debug.Log("Player reached exit - Player scores a point");
            playerScore++;
            gameActive = false;
            StartCoroutine(RestartGame());
            return;
        }

    }

    //restart
    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(0.5f);
        if (aiObject != null)
            Destroy(aiObject);
        // if (rlObject != null)
        //     Destroy(rlObject);
        if (player != null && player.gameObject != null)
            Destroy(player.gameObject);
        CreateMaze();
    }


    private void Reset()
    {
        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                rooms[i, j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
                rooms[i, j].visited = false;
            }
        }
    }
    public void EndGame()
    {
        if (playerScore > aiScore)
        {
            SceneManager.LoadScene("win_scene");
        }
        if(playerScore == aiScore)
        {
            SceneManager.LoadScene("tie_scene");
        }
        else
        {
            SceneManager.LoadScene("lose_scene");
        }
    }

    private void Update()
    {
        // Print stats with T key
        if (Input.GetKeyDown(KeyCode.T) && rl != null)
        {
            Debug.Log(rl.GetStats());
        }
    }
}