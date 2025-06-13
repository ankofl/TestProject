using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

partial struct DroneToOreSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{

	}

	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (drone, team, tran, target, velocity) in
			SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRW<LocalTransform>, RefRO<DroneToOre>, RefRW<PhysicsVelocity>>()
			.WithNone<DroneMining>().WithNone<DroneToHome>().WithNone<DroneMoveDisable>())
		{
			var dist2d = math.distance(
				new float2(tran.ValueRO.Position.x, tran.ValueRO.Position.z),
				new float2(target.ValueRO.Target.x, target.ValueRO.Target.z));



			var dir = math.normalize(target.ValueRO.Target - tran.ValueRO.Position);

			if (dist2d > 3f)
			{
				if (tran.ValueRO.Position.y < 5)
				{
					var deltaY = math.max(0, 5 - tran.ValueRO.Position.y) / 10;

					dir = math.normalize(new float3(dir.x, deltaY, dir.z));
				}

				velocity.ValueRW.Linear = dir * drone.ValueRO.Speed;
			}
			else
			{
				velocity.ValueRW.Linear = dir * drone.ValueRO.Speed;

				var dist3d = math.distance(tran.ValueRO.Position, target.ValueRO.Target);

				if (dist3d < 1.3f)
				{
					var ore = SystemAPI.GetComponent<DroneToOre>(drone.ValueRO.Entity).Ore;
					ecb.RemoveComponent<DroneToOre>(drone.ValueRO.Entity);

					ecb.AddComponent(drone.ValueRO.Entity, new DroneMining
					{

					});

					ecb.AddComponent(ore, new OreMining
					{
						Drone = drone.ValueRO.Entity,
						MiningRemained = 2f,
					});


					if (team.ValueRO.CurrentTeam == Teams.Left)
					{
						ecb.RemoveComponent<OreToTeamLeft>(ore);
					}
					else if (team.ValueRO.CurrentTeam == Teams.Right)
					{
						ecb.RemoveComponent<OreToTeamRight>(ore);
					}
				}
			}
		}

		ecb.Playback(state.EntityManager);

	}
}
