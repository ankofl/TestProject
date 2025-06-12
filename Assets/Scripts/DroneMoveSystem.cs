using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

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

        TranLookup = SystemAPI.GetComponentLookup<LocalTransform>();
	}
    float3 homeLeft, homeRight;
    ComponentLookup<LocalTransform> TranLookup;


	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        TranLookup.Update(ref state);

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

		NativeHashSet<Entity> pathedOresLeft = new(0, Allocator.Temp);
		NativeHashSet<Entity> pathedOresRight = new(0, Allocator.Temp);

		foreach (var (droneFree, droneTran) in SystemAPI.Query<RefRO<Drone>, RefRO<LocalTransform>>()
            .WithNone<DroneToOre, DroneToHome>())
        {
            var droneTeam = SystemAPI.GetComponent<Team>(droneFree.ValueRO.Entity).CurrentTeam;

			float minDist = 1000;
            Entity closestOre = Entity.Null;
            float3 targetPos = float3.zero;

            var oresFiltered = new NativeList<Entity>(Allocator.Temp);
            if(droneTeam == Teams.Left)
            {
				foreach (var (ore, oreTran) in SystemAPI.Query<RefRO<Ore>, RefRO<LocalTransform>>().WithNone<OreToTeamLeft>())
				{
					if (!pathedOresLeft.Contains(ore.ValueRO.Entity))
					{
                        oresFiltered.Add(ore.ValueRO.Entity);
					}
				}
			}
            else if (droneTeam == Teams.Right)
            {
				foreach (var (ore, oreTran) in SystemAPI.Query<RefRO<Ore>, RefRO<LocalTransform>>().WithNone<OreToTeamRight>())
				{
					if (!pathedOresRight.Contains(ore.ValueRO.Entity))
					{
						oresFiltered.Add(ore.ValueRO.Entity);
					}
				}
			}


            foreach (var oreFiltered in oresFiltered)
            {
                var pos = TranLookup.GetRefRO(oreFiltered).ValueRO.Position;

                var curDist = math.distance(droneTran.ValueRO.Position, pos);
                if (curDist < minDist)
                {
                    minDist = curDist;
                    closestOre = oreFiltered;

                    targetPos = pos;
                }
            }

            if(closestOre != Entity.Null)
            {
				pathedOresLeft.Add(closestOre);


                if (droneTeam == Teams.Left)
                {
                    ecb.AddComponent(closestOre, new OreToTeamLeft { });
				}
				else if (droneTeam == Teams.Right)
				{
					ecb.AddComponent(closestOre, new OreToTeamRight { });
				}

				ecb.AddComponent(droneFree.ValueRO.Entity, new DroneToOre
                {
                    Ore = closestOre,
                    Target = targetPos,
                });
            }
        }

        foreach (var (drone, team, tran, target, velocity) in 
            SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRW<LocalTransform>, RefRO<DroneToOre>, RefRW<PhysicsVelocity>>()
            .WithNone<DroneToHome>().WithNone<DroneMoveDisable>())
        {
            var dist = math.distance(tran.ValueRO.Position, target.ValueRO.Target);
            if(dist > 1.3f)
			{
                var maxSpeed = drone.ValueRO.MaxSpeed;
                if(dist < 3)
                {
                    maxSpeed = 1f;
                }


				var dir = math.normalize(target.ValueRO.Target - tran.ValueRO.Position);

				velocity.ValueRW.Linear = math.clamp(dir * drone.ValueRO.Speed,
					-maxSpeed, maxSpeed);
			}
            else
            {
                var ore = SystemAPI.GetComponent<DroneToOre>(drone.ValueRO.Entity).Ore;

				if (team.ValueRO.CurrentTeam == Teams.Left)
                {
					ecb.RemoveComponent<OreToTeamLeft>(ore);

					ecb.AddComponent(drone.ValueRO.Entity, new DroneToHome
					{
						HomePos = homeLeft,
					});
				}
                else if (team.ValueRO.CurrentTeam == Teams.Right)
				{
					ecb.RemoveComponent<OreToTeamRight>(ore);

					ecb.AddComponent(drone.ValueRO.Entity, new DroneToHome
					{
						HomePos = homeRight,
					});
				}

				ecb.RemoveComponent<DroneToOre>(drone.ValueRO.Entity);
			}
        }

        foreach (var (drone, tran, toHome, velocity) in 
            SystemAPI.Query<RefRO<Drone>, RefRW<LocalTransform>, RefRO<DroneToHome>, RefRW<PhysicsVelocity>>()
            .WithNone<DroneToOre>())
        {
            var dist = math.distance(tran.ValueRO.Position, toHome.ValueRO.HomePos);

            if(dist > 3.5f)
            {

				var maxSpeed = drone.ValueRO.MaxSpeed;
				if (dist < 8)
				{
					maxSpeed = 5f;
				}

				var dir = math.normalize(toHome.ValueRO.HomePos - tran.ValueRO.Position);

				velocity.ValueRW.Linear = math.clamp(dir * drone.ValueRO.Speed,
					-maxSpeed, maxSpeed);
			}
            else
            {
                ecb.RemoveComponent<DroneToHome>(drone.ValueRO.Entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct OreToTeamLeft : IComponentData
{

}

public struct OreToTeamRight : IComponentData
{

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
