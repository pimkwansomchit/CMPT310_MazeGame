# CMPT310_MazeGame
Group 4 - Maze Craze

## Short Description

Maze Craze is a maze race game with a single player and two enemies. The goal of the game is to reach the exit before our AI competitor does and avoid capture by the enemy. Our first enemy model’s goal is to exit the maze. It uses the A* pathfinding algorithm to compute the best path from its starting point to the exit. The other enemy model is an RL agent. The RL agent is tasked to chase and catch the player before they reach the exit. Over several iterations of gameplay, the RL agent is meant to learn from the player’s movements in order to improve its decision-making. The player and RL agent move at the same time while the AI moves at a constant speed. The system is built on Unity Game Engine which utilises C#.

## Features
1. Depth-First Search (DFS) Maze Generation
2. A* Pathfinding Enemy
3. Q-Learning Reinforcement Learning Agent

## Requirements 

Clone the repository
```
git clone https://github.com/pimkwansomchit/CMPT310_MazeGame.git
```

Open the project using Unity version 6.2 (6000.2.7f2) on Unity Hub. You can do this by clicking on ‘Add’, and then ‘Add project from disk’. From there, find the project on your local device and select it.

## Running the project

After opening the project in Unity, click the play button at the top to run the game.
