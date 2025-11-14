using UnityEngine;
using System.Collections.Generic;

public class AStar
{

    // uses g cost and h cost to find best predicted path
    // start from starting pos and get the end pos
    // evaluate the best option based on g cost + h cost and take that
    // open list - nodes queued up for searching
    // closed list - nodes already searched

    // keep going until current node == end node
    // or open list is empty
    
    public static List<Room> FindPath(Room[,] rooms, Vector2Int start, Vector2Int goal)
    {
        // number of cols
        int width = rooms.GetLength(0);
        // num of rows
        int height = rooms.GetLength(1);
        PathNode[,] nodeGrid = new PathNode[width, height];

        for (int x =0; x < width; x++){
            for (int y =0; y < height; y++){
                nodeGrid[x,y] = new PathNode(x,y);
            }
        }

        List <PathNode> openList = new List<PathNode>();
        List <PathNode> closedList = new List<PathNode>();

        PathNode startNode = nodeGrid[start.x, start.y];
        startNode.gCost =0;
        startNode.hCost = Heuristic(start,goal);
        startNode.fCost = startNode.gCost + startNode.hCost;
        openList.Add(startNode);
        
        while (openList.Count >0){
           PathNode currentNode = getlowestFCost(openList);
           if (currentNode.x == goal.x && currentNode.y == goal.y){

                return sequencePath(currentNode, rooms);
           }

           openList.Remove(currentNode);
           closedList.Add(currentNode);

           // explore every possible next step from the current room cell
            foreach (Vector2Int currentNeighbour in getActualNeighbours(rooms, currentNode.x, currentNode.y, width, height)){
                // currentX = currentNeighbour.x;
                // currentY = currentNeighbour.y
                PathNode neighbourNode = nodeGrid[currentNeighbour.x, currentNeighbour.y];

                bool InClosedList = false;
              
                foreach (PathNode node in closedList){
                    if (node.x == currentNeighbour.x && node.y == currentNeighbour.y){
                        InClosedList = true;
                        break;
                    }
                }
                    

                if (!InClosedList){
                       // PathNode neighbour = nodeGrid[currentNeighbour.x, currentNeighbour.y];
                        // each step is a cost of 1 in the maze
                        float currentG = currentNode.gCost +1;
                        // neighbour gCost intitally max so first should automatically
                        // be less and become the new gCost and then we compare the next
                        //ones to that g cost by making it the new neighbour gCost within loop

                        if (currentG < neighbourNode.gCost){
                            neighbourNode.cameFromNode = currentNode;
                            neighbourNode.gCost= currentG;
                            neighbourNode.hCost = Heuristic(new Vector2Int(neighbourNode.x, neighbourNode.y), goal);
                            neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;
                            bool inOpen = false;
                            foreach (PathNode node in openList){
                                if (node.x == neighbourNode.x && node.y == neighbourNode.y){
                                       inOpen = true;
                                        break;
                                }
                            }
                              

                            if (!inOpen){
                                openList.Add(neighbourNode);
                            }
                        }



                }
            }
        }
            // if not successful
            return null;
           
    }
         



    

    private static List<Vector2Int> getActualNeighbours(Room[,] room, int x, int y, int width, int height){
        List <Vector2Int> neighbourNodes = new List <Vector2Int>();
        Room currentRoom = room[x,y];

        // Check if we have a path to the top
        if (y < height -1 && !currentRoom.dirflags[Room.Directions.TOP]){
            neighbourNodes.Add(new Vector2Int(x,y +1));
        }
        // Check if we have a path to the bottom
        if (y > 0 && !currentRoom.dirflags[Room.Directions.BOTTOM]){
            neighbourNodes.Add(new Vector2Int(x,y-1));
        }
        // Check if we have a path to left
        if (x > 0 && !currentRoom.dirflags[Room.Directions.LEFT]){
            neighbourNodes.Add(new Vector2Int(x -1, y));
        }
        // Check if we have a path to right
        if (x < width -1 && !currentRoom.dirflags[Room.Directions.RIGHT]){
            neighbourNodes.Add(new Vector2Int(x +1, y));
        }

        return neighbourNodes;

    }

    // sequence of path we did
    private static List<Room> sequencePath(PathNode endNode, Room[,] rooms){
        List<Room>path = new List<Room>();
        PathNode currentNode = endNode;
        
        while (currentNode!= null){
            path.Add(rooms[currentNode.x, currentNode.y]);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private static PathNode getlowestFCost(List<PathNode> openList){
        PathNode lowest = openList[0];

        for (int i =1; i < openList.Count; i++){
            if (openList[i].fCost < lowest.fCost){
                lowest = openList[i];
            }
        }

        return lowest;
    }


    private static float Heuristic(Vector2Int theStart, Vector2Int theGoal){
        // manhattan distance
        return Mathf.Abs(theStart.x - theGoal.x) + Mathf.Abs(theStart.y - theGoal.y);
    }

   // private static float Cost()
    
}
