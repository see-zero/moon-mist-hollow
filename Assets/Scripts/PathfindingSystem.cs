using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;


public class PathfindingSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();

        Tilemap tilemap = GameObject.FindObjectOfType<Tilemap>();

        Debug.Log(tilemap.HasTile(new Vector3Int(1, 0, 0)));
        Debug.Log(tilemap.HasTile(new Vector3Int(0, 1, 0)));
        Debug.Log(tilemap.HasTile(new Vector3Int(1, 1, 0)));

       
    }


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

            var pathNodeArray = new NativeArray<PathNode>(
                SimulationConstants.MAP_SIZE * SimulationConstants.MAP_SIZE,
                Allocator.Temp
            );


            for (int x = 0; x < SimulationConstants.MAP_SIZE; x++)
            {
                for (int y = 0; y < SimulationConstants.MAP_SIZE; y++)
                {
                    PathNode pathNode = new PathNode
                    {
                        x = x,
                        y = y,

                        index = CalculateIndex(x, y),

                        gCost = int.MaxValue,
                        Solid = false,
                        previousIndex = -1,
                    };

                    pathNode.CalcH(x, y, endPosition);
                    pathNode.CalcF();

                    pathNodeArray[pathNode.index] = pathNode;
                }
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

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y)];
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

                    if (!OnGrid(position))
                    {
                        continue;
                    }

                    int positionIndex = CalculateIndex(position.x, position.y);

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
                        neighborNode.previousIndex = currentNodeIndex;
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

            if (endNode.previousIndex == -1)
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
            if (endNode.previousIndex != -1)
            {
                pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

                PathNode currentNode = endNode;

                while (currentNode.previousIndex != -1)
                {
                    PathNode prevNode = pathNodeArray[currentNode.previousIndex];
                    pathPositionBuffer.Add(new PathPosition { position = new int2(prevNode.x, prevNode.y) });
                    currentNode = prevNode;
                }
            }
        }


        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.previousIndex != -1)
            {
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;

                while (currentNode.previousIndex != -1)
                {
                    PathNode prevNode = pathNodeArray[currentNode.previousIndex];
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


        private bool OnGrid(int2 pos)
        {
            return
                pos.x >= 0 && pos.y >= 0 &&
                pos.x < SimulationConstants.MAP_SIZE && pos.y < SimulationConstants.MAP_SIZE;
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


        private int CalculateIndex(int x, int y)
        {
            return x + y * SimulationConstants.MAP_SIZE;
        }
    }
}
