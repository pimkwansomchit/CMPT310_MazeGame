using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int gridX;
    public int gridY;
    public int exitX;
    public int exitY;
    public Room[,] rooms;

    private Vector3 targetPos;
    private bool isMoving = false;

    // Q-Learning hyperparameters
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private float discount = 0.95f;
    [SerializeField] private float epsilon = 0.2f;
    [SerializeField] private float minEpsilon = 0.05f;
    [SerializeField] private float epsilonDecay = 0.999f;
    [SerializeField] private int maxRetries = 10;

    // Q-Table
    // private Dictionary<(int, int, int, int, int, int), float[]> Q
    //     = new Dictionary<(int, int, int, int, int, int), float[]>();

    private Dictionary<string, float[]> Q = new Dictionary<string, float[]>();


    // episode tracking
    // moves in current game
    private int currentSteps = 0;
    // no. of complete games
    private int episodeCount = 0;
    private int catchCount = 0;

    private PlayerBehaviour player;
    private int startX, startY;
    private bool playerMoved = false;

    public void Init(Room[,] rooms, int startX, int startY, int exitX, int exitY)
    {
        this.rooms = rooms;
        this.gridX = startX;
        this.gridY = startY;
        this.exitX = exitX;
        this.exitY = exitY;
        this.startX = startX;
        this.startY = startY;

        targetPos = transform.position;
        if (player == null)
            player = UnityEngine.Object.FindFirstObjectByType<PlayerBehaviour>();


        Debug.Log($"RL Agent (re)initialized at ({startX}, {startY})");
    }

    void Update()
    {
        if (rooms == null || player == null) return;

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                isMoving = false;

            return;
        }

        // Only act after player has completed a move
        if (!playerMoved) return;

        playerMoved = false;

        TakeTurn();
    }

    //private bool playerMoved = false;
    public void NotifyPlayerMoved()
    {
        playerMoved = true;
    }

    void TakeTurn()
    {
        currentSteps++;

        //var state = GetState();
        string state = GetState();

        if (!Q.ContainsKey(state))
            Q[state] = new float[4]; // 4 actions

        int action = -1;
        int nx = gridX;
        int ny = gridY;
        int attempts = 0;
        bool valid = false;

       //int action = ChooseAction(state);
        while (!valid && attempts < maxRetries)
        {
            action = ChooseAction(state);
            (nx, ny, valid) = ComputeMove(action);

            if (!valid)
            {
                float wallPenalty = -0.2f;

                string wallNextState = GetState();

                if (!Q.ContainsKey(wallNextState))
                    Q[wallNextState] = new float[4];

                float wallOldQ = Q[state][action];
                float wallMaxNext = Mathf.Max(Q[wallNextState]);

                Q[state][action] = wallOldQ + learningRate * (wallPenalty + discount * wallMaxNext - wallOldQ);
                attempts++;
            }
        }

        if (!valid)
        {
            Debug.LogWarning($"RL Agent stuck at ({gridX}, {gridY}) after {maxRetries} attempts");
            return;
        }


        // calculate reward
        float reward = ComputeReward(nx, ny, true);

        //var nextState = (player.gridX, player.gridY, nx, ny, exitX, exitY);
        string nextState = GetNextState(nx, ny);

        if (!Q.ContainsKey(nextState))
            Q[nextState] = new float[4];

        // Q-learning update
        float oldQ = Q[state][action];
        float maxNext = Mathf.Max(Q[nextState]);

        Q[state][action] = oldQ + learningRate * (reward + discount * maxNext - oldQ);

        // Perform move if valid

        gridX = nx;
        gridY = ny;
        targetPos = rooms[nx, ny].transform.position;
        isMoving = true;


        //check if caught player
        if (gridX == player.gridX && gridY == player.gridY)
        {
            catchCount++;
            Debug.Log($"caught player, total catches: {catchCount}/{episodeCount + 1}");

            GenerateMaze maze = UnityEngine.Object.FindFirstObjectByType<GenerateMaze>();
            if (maze != null)
                maze.CheckGameEnd();
        }

        // decay exploration
        epsilon = Mathf.Max(minEpsilon, epsilon * epsilonDecay);
    }

    //(int, int, int, int, int, int) GetState()
    // {
    //     return (player.gridX, player.gridY, gridX, gridY, exitX, exitY);
    // }

    string GetState()
    {
        int dx = Mathf.Clamp(player.gridX - gridX, -5, 5);
        int dy = Mathf.Clamp(player.gridY - gridY, -5, 5);
        return $"{dx},{dy}";
    }

    string GetNextState(int nx, int ny)
    {
        int dx = Mathf.Clamp(player.gridX - nx, -5, 5);
        int dy = Mathf.Clamp(player.gridY - ny, -5, 5);
        return $"{dx},{dy}";
    }



    //int ChooseAction((int, int, int, int, int, int) state)
    int ChooseAction(string state)
    {
        if (UnityEngine.Random.value < epsilon)
            return UnityEngine.Random.Range(0, 4);

        float[] vals = Q[state];
        float max = Mathf.Max(vals);

        List<int> bestActions = new List<int>();
        for (int i = 0; i < 4; i++)
            if (vals[i] == max)
                bestActions.Add(i);

        return bestActions[UnityEngine.Random.Range(0, bestActions.Count)];
    }

    (int, int, bool) ComputeMove(int action)
    {
        int nx = gridX;
        int ny = gridY;

        Room.Directions dir = Room.Directions.NONE;

        switch (action)
        {
            case 0: ny += 1; dir = Room.Directions.TOP; break;
            case 1: ny -= 1; dir = Room.Directions.BOTTOM; break;
            case 2: nx -= 1; dir = Room.Directions.LEFT; break;
            case 3: nx += 1; dir = Room.Directions.RIGHT; break;
        }

        if (nx < 0 || ny < 0 || nx >= rooms.GetLength(0) || ny >= rooms.GetLength(1))
            return (gridX, gridY, false);

        if (rooms[gridX, gridY].dirflags[dir])
            return (gridX, gridY, false);

        return (nx, ny, true);
    }

    float ComputeReward(int nx, int ny, bool valid)
    {
        float reward = 0f;

        if (!valid)
            return -0.2f;

        // catch player
        if (nx == player.gridX && ny == player.gridY)
            return 10f;

        // chase reward
        float oldDist = Mathf.Abs(gridX - player.gridX) + Mathf.Abs(gridY - player.gridY);
        float newDist = Mathf.Abs(nx - player.gridX) + Mathf.Abs(ny - player.gridY);

        if (newDist < oldDist) reward += 0.5f;
        else reward -= 0.5f;

        // go toward exit
        float oldExitDist = Manhattan(gridX, gridY, exitX, exitY);
        float newExitDist = Manhattan(nx, ny, exitX, exitY);

        if (newExitDist < oldExitDist) reward += 0.05f;

        // blocking path
        int distPE = Manhattan(player.gridX, player.gridY, exitX, exitY);
        int distPA = Manhattan(player.gridX, player.gridY, nx, ny);
        int distAE = Manhattan(nx, ny, exitX, exitY);

        if (distPA + distAE == distPE)
            reward += 0.2f;

        return reward;
    }

    int Manhattan(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    public void ResetEpisode()
    {
        gridX = startX;
        gridY = startY;
        // if (rooms != null && rooms.GetLength(0) > startX && rooms.GetLength(1) > startY)
        // {
        //     transform.position = rooms[startX, startY].transform.position;
        // }

        if (rooms != null && startX < rooms.GetLength(0) && startY < rooms.GetLength(1))
        {
            transform.position = rooms[startX, startY].transform.position;
            targetPos = transform.position;
        }

        //targetPos = transform.position;
        
        currentSteps = 0;
        episodeCount++;
        isMoving = false;
        playerMoved = false;

        Debug.Log($"Episode {episodeCount} starting. Epsilon: {epsilon:F3}, Q-size: {Q.Count}");

    }

    public float GetCatchRate()
    {
        if (episodeCount == 0) return 0f;
        return (catchCount / (float)episodeCount) * 100f;
    }

    public string GetStats()
    {
        return $"Episodes: {episodeCount} | Catches: {catchCount} | Rate: {GetCatchRate():F1}% | Epsilon: {epsilon:F3} | Q-states: {Q.Count}";
    }
}
