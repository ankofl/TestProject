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
        state.RequireForUpdate<DroneRestartData>();
        state.RequireForUpdate<Home>();
        state.RequireForUpdate<Prefabs>();
        state.RequireForUpdate<OreSpawnEnded>();

        rnd = new();
        rnd.InitState();
    }
    Random rnd;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var prefabs = SystemAPI.GetSingleton<Prefabs>();

		var droneRestartData = SystemAPI.GetSingleton<DroneRestartData>();

        var rw = SystemAPI.GetComponentRW<Drone>(prefabs.Drone);
        rw.ValueRW.Speed = droneRestartData.DronesSpeed;


        foreach (var home in SystemAPI.Query<RefRO<Home>>().WithNone<HomeDronesRequest, HomeDronesRequestComplete>())
        {
            ecb.AddComponent(home.ValueRO.Entity, new HomeDronesRequest
            {
                TimeToNextWave = 0,
                DronesRemainedCount = droneRestartData.DronesCount,
            });
        }

        foreach (var (home, team, tran, droneRequest, color) in 
            SystemAPI.Query<RefRW<Home>, RefRO<Team>, RefRO<LocalTransform>, RefRW<HomeDronesRequest>, RefRO<URPMaterialPropertyBaseColor>>())
        {
            if(droneRequest.ValueRO.TimeToNextWave > 0)
            {
                droneRequest.ValueRW.TimeToNextWave -= SystemAPI.Time.DeltaTime;
            }
            else
            {
                droneRequest.ValueRW.TimeToNextWave = 0.2f;

				var portion = 10;
                if(droneRequest.ValueRO.DronesRemainedCount <= portion)
                {
                    portion = droneRequest.ValueRO.DronesRemainedCount;
					ecb.RemoveComponent<HomeDronesRequest>(home.ValueRO.Entity);

                    ecb.AddComponent(home.ValueRO.Entity, new HomeDronesRequestComplete
                    {

                    });
				}
                else
                {
                    droneRequest.ValueRW.DronesRemainedCount -= portion;
				}

				for (int i = 0; i < portion; i++)
				{
					var deltaX = 4;
					if (tran.ValueRO.Position.x > 0)
					{
						deltaX = -deltaX;
					}
					var droneSpawnPoint = tran.ValueRO.Position +
						new float3(deltaX, 0, (i + 0.5f) * 2f - portion);


					var drone = ecb.Instantiate(prefabs.Drone);
					ecb.SetComponent(drone, LocalTransform.FromPosition(droneSpawnPoint));
					ecb.SetComponent(drone, new Team { CurrentTeam = team.ValueRO.CurrentTeam });
					ecb.SetComponent(drone, new URPMaterialPropertyBaseColor { Value = color.ValueRO.Value });


				}
			}
        }

		var homesWithRequestCount = SystemAPI.QueryBuilder().WithAll<Home>().WithAll<HomeDronesRequestComplete>().Build().CalculateEntityCount();
		if (homesWithRequestCount == 2)
		{
            foreach (var home in SystemAPI.Query<RefRO<Home>>().WithAll<HomeDronesRequestComplete>())
            {
                ecb.RemoveComponent<HomeDronesRequestComplete>(home.ValueRO.Entity);
            }

			ecb.DestroyEntity(SystemAPI.GetSingletonEntity<DroneRestartData>());
		}

		ecb.Playback(state.EntityManager);
    }
}

public struct HomeDronesRequestComplete : IComponentData
{

}