using Unity.Entities;
using Unity.Mathematics;

public struct MoveData : IComponentData
{
    public float moveSpeed;
    public float3 direction;
}
