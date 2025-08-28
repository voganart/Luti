using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float3 inputDirection = new float3(h, 0f, v);
        if (math.lengthsq(inputDirection) > 1f)
        {
            inputDirection = math.normalize(inputDirection);
        }

        foreach (var moveData in SystemAPI.Query<RefRW<MoveData>>().WithAll<PlayerTag>())
        {
            moveData.ValueRW.direction = inputDirection;
        }
    }
}
