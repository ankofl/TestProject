using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.GraphicsBuffer;

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

        NativeHashSet<Entity> pathedOres = new (0, Allocator.Temp);

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

        foreach (var (drone, droneTran) in SystemAPI.Query<RefRO<Drone>, RefRO<LocalTransform>>().WithNone<DroneToOre, DroneToHome>())
        {
            float minDist = 1000;
            Entity closestOre = Entity.Null;
            float3 targetPos = float3.zero;

            foreach (var (ore, oreTran) in SystemAPI.Query<RefRO<Ore>, RefRO<LocalTransform>>())
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

				ecb.AddComponent(drone.ValueRO.Entity, new DroneToOre
                {
                    Target = targetPos,
                });
            }
        }

        foreach (var (drone, team, tran, target) in 
            SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRW<LocalTransform>, RefRO<DroneToOre>>().WithNone<DroneToHome>())
        {
            var dist = math.distance(tran.ValueRO.Position, target.ValueRO.Target);
            if(dist > 0.3f)
			{
				var dir = math.normalize(target.ValueRO.Target - tran.ValueRO.Position);

				tran.ValueRW.Position = tran.ValueRO.Position + drone.ValueRO.Speed * SystemAPI.Time.DeltaTime * dir;
			}
            else
            {
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

        foreach (var (drone, tran, toHome) in 
            SystemAPI.Query<RefRO<Drone>, RefRW<LocalTransform>, RefRO<DroneToHome>>().WithNone<DroneToOre>())
        {
            var dist = math.distance(tran.ValueRO.Position, toHome.ValueRO.HomePos);

            if(dist > 0.3f)
            {
				var dir = math.normalize(toHome.ValueRO.HomePos - tran.ValueRO.Position);

				tran.ValueRW.Position = tran.ValueRO.Position + drone.ValueRO.Speed * SystemAPI.Time.DeltaTime * dir;
			}
            else
            {
                ecb.RemoveComponent<DroneToHome>(drone.ValueRO.Entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct DroneToHome : IComponentData
{
    public float3 HomePos;
}

public struct DroneToOre : IComponentData
{
    public float3 Target;
}
