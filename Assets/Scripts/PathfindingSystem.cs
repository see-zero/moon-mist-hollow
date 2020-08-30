using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;


public class PathfindingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref PathParams pathParams) =>
        {
            FindPathJob findPathJob = new FindPathJob
            {
                startPosition = pathParams.startPosition,
                endPosition = pathParams.endPosition,
                pathPositionBuffer = pathPositionBuffer,
                entity = entity,
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
            };

            findPathJob.Run();

            PostUpdateCommands.RemoveComponent<PathParams>(entity);
        });
    }


    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;

        public Entity entity;
        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;

        public DynamicBuffer<PathPosition> pathPositionBuffer;

        public void Execute()
        {
            int2 gridSize = new int2(20, 20);

            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);


            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode pathNode = new PathNode
                    {
                        x = x,
                        y = y,

                        index = CalculateIndex(x, y, gridSize.x),

                        gCost = int.MaxValue,
                    };

                    pathNode.CalcH(x, y, endPosition);
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


            NativeArray<int2> offsets = new NativeArray<int2>(8, Allocator.Temp);
            offsets[0] = new int2(-1, +0);
            offsets[1] = new int2(+1, +0);
            offsets[2] = new int2(+0, +1);
            offsets[3] = new int2(+0, -1);
            offsets[4] = new int2(-1, -1);
            offsets[5] = new int2(-1, +1);
            offsets[6] = new int2(-1, -1);
            offsets[7] = new int2(-1, +1);

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalcF();

            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);

                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    break;
                }

                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

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

            pathPositionBuffer.Clear();

            if (endNode.previousNodeIndex == -1)
            {
                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = -1 };
            }
            else
            {
                CalculatePath(pathNodeArray, endNode, pathPositionBuffer);
                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 1 };
            }

            pathNodeArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
            offsets.Dispose();
        }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPosition> pathPositionBuffer)
        {
            if (endNode.previousNodeIndex != -1)
            {
                pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                PathNode currentNode = endNode;

                while (currentNode.previousNodeIndex != -1)
                {
                    PathNode prevNode = pathNodeArray[currentNode.previousNodeIndex];
                    pathPositionBuffer.Add(new PathPosition { position = new int2(prevNode.x, prevNode.y) });
                    currentNode = prevNode;
                }
            }
        }


        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.previousNodeIndex != -1)
            {
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;

                while (currentNode.previousNodeIndex != -1)
                {
                    PathNode prevNode = pathNodeArray[currentNode.previousNodeIndex];
                    path.Add(new int2(prevNode.x, prevNode.y));
                    currentNode = prevNode;
                }

                return path;
            }
            else
            {
                return new NativeList<int2>(Allocator.Temp);
            }
        }


        private bool OnGrid(int2 pos, int2 gridSize)
        {
            return
                pos.x >= 0 && pos.y >= 0 &&
                pos.x < gridSize.x && pos.y < gridSize.y;
        }


        private int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];

            for (int i = 1; i < openList.Length; i++)
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
    }
}
