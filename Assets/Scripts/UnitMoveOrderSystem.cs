using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


public class UnitMoveOrderSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Entities.ForEach((Entity entity, ref Translation translation) =>
            {
                _ = EntityManager.AddComponentData(
                    entity,
                    new PathParams {
                        startPosition = new int2(0, 0),
                        endPosition = new int2(4, 0)
                    }
                );
            });
        }
    }
}
