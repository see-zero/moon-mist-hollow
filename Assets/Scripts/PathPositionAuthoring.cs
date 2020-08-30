using UnityEngine;
using Unity.Entities;


public class PathPositionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        _ = dstManager.AddBuffer<PathPosition>(entity);
    }
}
