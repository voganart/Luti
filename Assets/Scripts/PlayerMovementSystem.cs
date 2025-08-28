using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (moveData, transform) in SystemAPI.Query<RefRO<MoveData>, RefRW<LocalTransform>>().WithAll<PlayerTag>())
        {
            float3 direction = moveData.ValueRO.direction;
            float3 move = direction * moveData.ValueRO.moveSpeed * deltaTime;

            transform.ValueRW.Position += move;

            // Поворот по направлению движения
            if (!direction.Equals(float3.zero))
            {
                quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());
                transform.ValueRW.Rotation = math.slerp(transform.ValueRW.Rotation, targetRotation, deltaTime * 10f);
            }
        }
    }
}
