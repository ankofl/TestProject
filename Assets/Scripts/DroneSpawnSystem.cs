using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

partial struct DroneSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Home>().WithAll<HomeDronesRequest>().Build());
        state.RequireForUpdate<Prefabs>();

        rnd = new();
        rnd.InitState();
    }
    Random rnd;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var prefabs = SystemAPI.GetSingleton<Prefabs>();

        foreach (var (home, team, tran, droneRequest, color) in 
            SystemAPI.Query<RefRW<Home>, RefRO<Team>, RefRO<LocalTransform>, RefRO<HomeDronesRequest>, RefRO<URPMaterialPropertyBaseColor>>())
        {
            for (int i = 0; i < droneRequest.ValueRO.DronesCount; i++)
            {
                var deltaX = 2;
				if (tran.ValueRO.Position.x > 0)
				{
                    deltaX = -deltaX;
				}
				var droneSpawnPoint = tran.ValueRO.Position + new float3(deltaX, 0, (i + 0.5f) * 2f - (droneRequest.ValueRO.DronesCount));
                

				var drone = ecb.Instantiate(prefabs.Drone);
				ecb.SetComponent(drone, LocalTransform.FromPosition(droneSpawnPoint));
				ecb.SetComponent(drone, new DroneToBufferRequest { Team = team.ValueRO.CurrentTeam });
				ecb.SetComponent(drone, new URPMaterialPropertyBaseColor { Value = color.ValueRO.Value });
			}

			ecb.RemoveComponent<HomeDronesRequest>(home.ValueRO.Entity);
        }

        ecb.Playback(state.EntityManager);
    }
}