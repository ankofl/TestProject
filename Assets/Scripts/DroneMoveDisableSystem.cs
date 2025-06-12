using System.Drawing;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

partial struct DroneMoveDisableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
		state.RequireForUpdate<Drone>();
	}

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
		var ecb = new EntityCommandBuffer(Allocator.Temp);

		NativeHashSet<Entity> disabledDrones = new(0, Allocator.Temp);

		foreach (var (droneToHome, team, tran) in SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRO<LocalTransform>>()
			.WithAll<DroneToHome>())
		{
			foreach (var (droneOther, teamOther, tranOther, color) in 
				SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRO<LocalTransform>, RefRW<URPMaterialPropertyBaseColor>>()
				.WithAll<DroneToOre>())
			{
				if (disabledDrones.Contains(droneOther.ValueRO.Entity) ||
					droneToHome.ValueRO.Entity == droneOther.ValueRO.Entity ||
					team.ValueRO.CurrentTeam != teamOther.ValueRO.CurrentTeam)
				{
					continue;
				}

				var dist = math.distance(tran.ValueRO.Position, tranOther.ValueRO.Position);

				if(dist < 10f)
				{
					if (!SystemAPI.HasComponent<DroneMoveDisable>(droneOther.ValueRO.Entity))
					{
						disabledDrones.Add(droneOther.ValueRO.Entity);

						ecb.AddComponent(droneOther.ValueRO.Entity, new DroneMoveDisable
						{
							ColorBase = color.ValueRO.Value,
							Drone = droneOther.ValueRO.Entity,
							TimeRemain = 0.5f,
						});

						if(team.ValueRO.CurrentTeam == Teams.Left)
						{
							color.ValueRW.Value = TeamsColor.To4(TeamsColor.LeftInactive);
						}
						else if (team.ValueRO.CurrentTeam == Teams.Right)
						{
							color.ValueRW.Value = TeamsColor.To4(TeamsColor.RightInactive);
						}
					}
				}
			}
		}

		foreach (var (droneMoveDisable, velocity, color) in 
			SystemAPI.Query<RefRW<DroneMoveDisable>, RefRW<PhysicsVelocity>, RefRW<URPMaterialPropertyBaseColor>>())
		{
			if(droneMoveDisable.ValueRO.TimeRemain > 0)
			{

				//velocity.ValueRW.Linear = math.lerp(velocity.ValueRO.Linear, float3.zero, 0.01f);

				droneMoveDisable.ValueRW.TimeRemain -= SystemAPI.Time.DeltaTime;
			}
			else
			{
				color.ValueRW.Value = droneMoveDisable.ValueRO.ColorBase;

				ecb.RemoveComponent<DroneMoveDisable>(droneMoveDisable.ValueRO.Drone);
			}
		}


		ecb.Playback(state.EntityManager);
	}
}

public struct DroneMoveDisable : IComponentData
{
	public float4 ColorBase;
	public Entity Drone;
	public float TimeRemain;
}