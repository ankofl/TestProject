using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

partial struct DroneMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Home>();
        state.RequireForUpdate<Drone>();
        state.RequireForUpdate<Ore>();

        homeLeft = float3.zero;
        homeRight = float3.zero;
	}
    float3 homeLeft, homeRight;

	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        if(homeLeft.Equals(float3.zero) || homeRight.Equals(float3.zero))
        {
			foreach (var (home, team, tran) in SystemAPI.Query<RefRO<Home>, RefRO<Team>, RefRO<LocalTransform>>())
			{
				if (team.ValueRO.CurrentTeam == Teams.Left)
				{
					homeLeft = tran.ValueRO.Position;
				}
				else if (team.ValueRO.CurrentTeam == Teams.Right)
				{
					homeRight = tran.ValueRO.Position;
				}
			}
		}

		NativeHashSet<Entity> pathedOres = new(0, Allocator.Temp);

		foreach (var (drone, droneTran) in SystemAPI.Query<RefRO<Drone>, RefRO<LocalTransform>>().WithNone<DroneToOre, DroneToHome>())
        {
            float minDist = 1000;
            Entity closestOre = Entity.Null;
            float3 targetPos = float3.zero;

            foreach (var (ore, oreTran) in SystemAPI.Query<RefRO<Ore>, RefRO<LocalTransform>>().WithNone<OreToDrone>())
            {
                if (!pathedOres.Contains(ore.ValueRO.Entity))
                {
                    var curDist = math.distance(droneTran.ValueRO.Position, oreTran.ValueRO.Position);
                    if(curDist < minDist)
                    {
                        minDist = curDist;
                        closestOre = ore.ValueRO.Entity;

                        targetPos = oreTran.ValueRO.Position;
                    }
                }
            }

            if(closestOre != Entity.Null)
            {
				pathedOres.Add(closestOre);

                ecb.AddComponent(closestOre, new OreToDrone
                {
                    Drone = drone.ValueRO.Entity,
				});

				ecb.AddComponent(drone.ValueRO.Entity, new DroneToOre
                {
                    Ore = closestOre,
                    Target = targetPos,
                });
            }
        }

        foreach (var (drone, team, tran, target, velocity) in 
            SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRW<LocalTransform>, RefRO<DroneToOre>, RefRW<PhysicsVelocity>>()
            .WithNone<DroneToHome>())
        {
            var dist = math.distance(tran.ValueRO.Position, target.ValueRO.Target);
            if(dist > 1.3f)
			{
				var dir = math.normalize(target.ValueRO.Target - tran.ValueRO.Position);

				velocity.ValueRW.Linear = math.clamp(velocity.ValueRO.Linear + drone.ValueRO.Speed * SystemAPI.Time.DeltaTime * dir,
                    new float3(-3), new float3(3));
			}
            else
            {
                ecb.RemoveComponent<OreToDrone>(SystemAPI.GetComponent<DroneToOre>(drone.ValueRO.Entity).Ore);

                ecb.RemoveComponent<DroneToOre>(drone.ValueRO.Entity);
                
                if(team.ValueRO.CurrentTeam == Teams.Left)
                {
					ecb.AddComponent(drone.ValueRO.Entity, new DroneToHome
					{
						HomePos = homeLeft,
					});
				}
                else if (team.ValueRO.CurrentTeam == Teams.Right)
				{
					ecb.AddComponent(drone.ValueRO.Entity, new DroneToHome
					{
						HomePos = homeRight,
					});
				}
			}
        }

        foreach (var (drone, tran, toHome, velocity) in 
            SystemAPI.Query<RefRO<Drone>, RefRW<LocalTransform>, RefRO<DroneToHome>, RefRW<PhysicsVelocity>>()
            .WithNone<DroneToOre>())
        {
            var dist = math.distance(tran.ValueRO.Position, toHome.ValueRO.HomePos);

            if(dist > 3.5f)
            {
				var dir = math.normalize(toHome.ValueRO.HomePos - tran.ValueRO.Position);

				velocity.ValueRW.Linear = math.clamp(velocity.ValueRO.Linear + drone.ValueRO.Speed * SystemAPI.Time.DeltaTime * dir,
					new float3(-3), new float3(3));
			}
            else
            {
                ecb.RemoveComponent<DroneToHome>(drone.ValueRO.Entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct OreToDrone : IComponentData
{
    public Entity Drone;
}

public struct DroneToHome : IComponentData
{
    public float3 HomePos;
}

public struct DroneToOre : IComponentData
{
    public Entity Ore;

	public float3 Target;
}
