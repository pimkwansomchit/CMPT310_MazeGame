using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int gridX;
    public int gridY;
    public Room[,] rooms;

    private Vector3 targetPos;
    private bool isMoving = false;

    // Q-Learning hyperparameters
    private float learningRate = 0.1f;
    private float discount = 0.95f;
    private float epsilon = 0.2f;   // exploration rate

    // Q-Table
    private Dictionary<(int, int, int, int), float[]> Q
        = new Dictionary<(int, int, int, int), float[]>();

    private PlayerBehaviour player;

    public void Init(Room[,] rooms, int startX, int startY)
    {
        this.rooms = rooms;
        this.gridX = startX;
        this.gridY = startY;
        targetPos = transform.position;

        player = UnityEngine.Object.FindFirstObjectByType<PlayerBehaviour>();

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

    private bool playerMoved = false;
    public void NotifyPlayerMoved()
    {
        playerMoved = true;
    }

    void TakeTurn()
    {
        var state = GetState();

        if (!Q.ContainsKey(state))
            Q[state] = new float[4]; // 4 actions

        int action = ChooseAction(state);

        (int nx, int ny, bool valid) = ComputeMove(action);

        // calculate reward
        float reward = ComputeReward(nx, ny);

        var nextState = (player.gridX, player.gridY, nx, ny);

        if (!Q.ContainsKey(nextState))
            Q[nextState] = new float[4];

        // Q-learning update
        float oldQ = Q[state][action];
        float maxNext = Mathf.Max(Q[nextState]);

        Q[state][action] = oldQ + learningRate * (reward + discount * maxNext - oldQ);

        // Perform move if valid
        if (valid)
        {
            gridX = nx;
            gridY = ny;
            targetPos = rooms[nx, ny].transform.position;
            isMoving = true;
        }
    }

    (int, int, int, int) GetState()
    {
        return (player.gridX, player.gridY, gridX, gridY);
    }

    int ChooseAction((int, int, int, int) state)
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

    // Action -> new pos
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

    float ComputeReward(int nx, int ny)
    {
        // catch player?
        if (nx == player.gridX && ny == player.gridY)
            return 10f;

        // distance reward
        float oldDist = Mathf.Abs(gridX - player.gridX) + Mathf.Abs(gridY - player.gridY);
        float newDist = Mathf.Abs(nx - player.gridX) + Mathf.Abs(ny - player.gridY);

        if (newDist < oldDist) return 0.1f;
        else return -0.1f;
    }
}
