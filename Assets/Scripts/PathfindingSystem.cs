using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

public class PathfindingSystem : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;


    private void Start()
    {
        FindPath(new int2(0, 0), new int2(2, 1));
    }


    private void FindPath(int2 startPos, int2 endPos)
    {
        int2 gridSize = new int2(4, 4);

        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);


        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode pathNode = new PathNode();
                pathNode.x = x;
                pathNode.y = y;

                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;
                pathNode.CalcH(x, y, endPos);
                pathNode.CalcF();

                pathNode.Solid = false;

                pathNode.previousNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }


        {
            PathNode freePathNode = pathNodeArray[CalculateIndex(1, 0, gridSize.x)];
            freePathNode.Solid = true;
            pathNodeArray[CalculateIndex(1, 0, gridSize.x)] = freePathNode;

            freePathNode = pathNodeArray[CalculateIndex(1, 1, gridSize.x)];
            freePathNode.Solid = true;
            pathNodeArray[CalculateIndex(1, 1, gridSize.x)] = freePathNode;

        }


        NativeArray<int2> offsets = new NativeArray<int2>(new int2[] {
            new int2(-1, 0),
            new int2(+1, 0),
            new int2(0, +1),
            new int2(0, -1),
            new int2(-1, -1),
            new int2(-1, +1),
            new int2(+1, -1),
            new int2(+1, +1),
        }, Allocator.Temp);


        int endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);

        PathNode startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalcF();

        pathNodeArray[startNode.index] = startNode;

        List<int> openList = new List<int>();
        List<int> closedList = new List<int>();

        openList.Add(startNode.index);

        while (openList.Count > 0)
        {
            int currentNodeIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);

            PathNode currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                break;
            }

            openList.Remove(currentNodeIndex);
            closedList.Add(currentNodeIndex);

            for (int i = 0; i < offsets.Length; i++)
            {
                int2 offset = offsets[i];
                int2 position = new int2(currentNode.x + offset.x, currentNode.y + offset.y);

                if (!OnGrid(position, gridSize))
                {
                    continue;
                }

                int positionIndex = CalculateIndex(position.x, position.y, gridSize.x);

                if (closedList.Contains(positionIndex))
                {
                    continue;
                }

                PathNode neighborNode = pathNodeArray[positionIndex];

                if (neighborNode.Solid)
                {
                    continue;
                }

                int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                currentNode.CalcH(currentNodePosition.x, currentNodePosition.y, position);

                int tentativeCost = currentNode.gCost + currentNode.hCost;

                if (tentativeCost < neighborNode.gCost)
                {
                    neighborNode.previousNodeIndex = currentNodeIndex;
                    neighborNode.gCost = tentativeCost;
                    neighborNode.CalcF();

                    pathNodeArray[positionIndex] = neighborNode;

                    if (!openList.Contains(neighborNode.index))
                    {
                        openList.Add(neighborNode.index);
                    }
                }
            }
        }

        PathNode endNode = pathNodeArray[endNodeIndex];

        if (endNode.previousNodeIndex == -1)
        {
            Debug.Log("No path found.");
        } else
        {
            List<int2> path = CalculatePath(pathNodeArray, endNode);

            foreach (int2 pathPosition in path)
            {
                Debug.Log(pathPosition);
            }
        }


        pathNodeArray.Dispose();
        offsets.Dispose();


    }


    private List<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.previousNodeIndex == -1)
        {
            return new List<int2>();
        } else
        {
            List<int2> path = new List<int2>
            {
                new int2(endNode.x, endNode.y)
            };

            PathNode currentNode = endNode;

            while (currentNode.previousNodeIndex != -1)
            {
                PathNode prevNode = pathNodeArray[currentNode.previousNodeIndex];
                path.Add(new int2(prevNode.x, prevNode.y));
                currentNode = prevNode;
            }

            return path;
        }
    }


    private bool OnGrid(int2 pos, int2 gridSize)
    {
        return
            pos.x >= 0 && pos.y >= 0 &&
            pos.x < gridSize.x && pos.y < gridSize.y;
    }


    private int GetLowestFCostNodeIndex(List<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];

        for (int i = 1; i < openList.Count; i++)
        {
            PathNode testNode = pathNodeArray[openList[i]];

            if (testNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testNode;
            }
        }

        return lowestCostPathNode.index;
    }


    private int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }


   


    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        private bool solid;

        public bool Solid { get; set; }

        public int previousNodeIndex;


        public void CalcF()
        {
            fCost = gCost + hCost;
        }


        public void CalcH(int x, int y, int2 bPos)
        {
            int dx = math.abs(x - bPos.x);
            int dy = math.abs(y - bPos.y);

            int remaining = math.abs(dx - dy);

            hCost = MOVE_DIAGONAL_COST * math.min(dx, dy) + MOVE_STRAIGHT_COST * remaining;
        }
    }
}
