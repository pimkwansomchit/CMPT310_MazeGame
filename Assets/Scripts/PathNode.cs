using UnityEngine;

public class PathNode
{
    // Information about the individual cell you're on
    // path cost func
    public float gCost;
    // goal proximity 
    public float hCost;
    public float fCost;
    public PathNode cameFromNode;
    public int x;
    public int y;

    public PathNode(int x, int y){
        this.x = x;
        this.y = y;
        gCost =float.MaxValue;
        hCost =0;
        fCost=0;
        cameFromNode = null;
    }


    
}
