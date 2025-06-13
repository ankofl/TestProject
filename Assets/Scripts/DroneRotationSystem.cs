using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct DroneRotationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Drone>();   
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var tran in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Drone>())
        {
            tran.ValueRW.Rotation = quaternion.identity;
        }
    }
}
