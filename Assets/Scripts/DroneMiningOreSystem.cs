using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

partial struct DroneMiningOreSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Ore>().WithAll<OreMining>().Build());
	}
	

	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

		Homes homes = default;
		if (!SystemAPI.HasSingleton<Homes>())
		{
			var entity = state.EntityManager.CreateEntity();

			foreach (var (home, team, tran) in SystemAPI.Query<RefRO<Home>, RefRO<Team>, RefRO<LocalTransform>>())
			{
				if (team.ValueRO.CurrentTeam == Teams.Left)
				{
					homes.Left = home.ValueRO.Entity;
				}
				else if (team.ValueRO.CurrentTeam == Teams.Right)
				{
					homes.Right = home.ValueRO.Entity;
				}

			}

			state.EntityManager.AddComponentData(entity, homes);
		}
		else
		{
			homes = SystemAPI.GetSingleton<Homes>();
		}

		foreach (var (ore, oreMining) in SystemAPI.Query<RefRO<Ore>, RefRW<OreMining>>())
		{
			var drone = SystemAPI.GetComponent<OreMining>(ore.ValueRO.Entity).Drone;
			var rw = SystemAPI.GetComponentRW<PhysicsVelocity>(drone);
			rw.ValueRW.Linear = float3.zero;

			if (oreMining.ValueRO.MiningRemained > 0)
			{
				oreMining.ValueRW.MiningRemained -= SystemAPI.Time.DeltaTime;
			}
			else
			{
				ecb.RemoveComponent<DroneMining>(drone);

				foreach (var child in SystemAPI.GetBuffer<Child>(drone))
				{
					if (SystemAPI.HasComponent<OreInDrone>(child.Value))
					{
						ecb.RemoveComponent<DisableRendering>(child.Value);
					}
				}


				var team = SystemAPI.GetComponent<Team>(drone);

				if (team.CurrentTeam == Teams.Left)
				{
					ecb.AddComponent(drone, new DroneToHome
					{
						Home = homes.Left,
						HomePos = SystemAPI.GetComponent<LocalTransform>(homes.Left).Position,
					});
				}
				else if (team.CurrentTeam == Teams.Right)
				{
					ecb.AddComponent(drone, new DroneToHome
					{
						Home = homes.Right,
						HomePos = SystemAPI.GetComponent<LocalTransform>(homes.Right).Position,
					});
				}


				ecb.RemoveComponent<OreMining>(ore.ValueRO.Entity);
				ecb.AddComponent(ore.ValueRO.Entity, new OreReload
				{
					TimeRemained = ore.ValueRO.OreRespawnTime,
				});
			}
		}

		ecb.Playback(state.EntityManager);
    }

}

public struct Homes : IComponentData
{
	public Entity Left, Right;
}

public struct OreMining : IComponentData
{
	public Entity Drone;
    public float MiningRemained;
}

public struct DroneMining : IComponentData
{

}

