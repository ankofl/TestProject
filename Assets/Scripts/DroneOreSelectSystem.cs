using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

partial struct DroneOreSelectSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Home>();
        state.RequireForUpdate<Drone>();
        state.RequireForUpdate<Ore>();

        TranLookup = SystemAPI.GetComponentLookup<LocalTransform>();
	}
    ComponentLookup<LocalTransform> TranLookup;


	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        TranLookup.Update(ref state);

		NativeHashSet<Entity> pathedOresLeft = new(0, Allocator.Temp);
		NativeHashSet<Entity> pathedOresRight = new(0, Allocator.Temp);

		foreach (var (droneFree, droneTran) in SystemAPI.Query<RefRO<Drone>, RefRO<LocalTransform>>()
            .WithNone<DroneToOre, DroneToHome, DroneMining>())
        {
            var droneTeam = SystemAPI.GetComponent<Team>(droneFree.ValueRO.Entity).CurrentTeam;

			float minDist = 1000;
            Entity closestOre = Entity.Null;
            float3 targetPos = float3.zero;

            var oresFiltered = new NativeList<Entity>(Allocator.Temp);
            if(droneTeam == Teams.Left)
            {
				foreach (var ore in SystemAPI.Query<RefRO<Ore>>().WithNone<OreMining>().WithNone<OreReload>().WithNone<OreToTeamLeft>())
				{
					if (!pathedOresLeft.Contains(ore.ValueRO.Entity))
					{
                        oresFiltered.Add(ore.ValueRO.Entity);
					}
				}
			}
            else if (droneTeam == Teams.Right)
            {
				foreach (var ore in SystemAPI.Query<RefRO<Ore>>().WithNone<OreMining>().WithNone<OreReload>().WithNone<OreToTeamRight>())
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
                if (droneTeam == Teams.Left)
                {
					pathedOresLeft.Add(closestOre);

					ecb.AddComponent(closestOre, new OreToTeamLeft 
                    {
                        Drone = droneFree.ValueRO.Entity,
					});
				}
				else if (droneTeam == Teams.Right)
				{
					pathedOresRight.Add(closestOre);

					ecb.AddComponent(closestOre, new OreToTeamRight
					{
						Drone = droneFree.ValueRO.Entity,
					});
				}

				ecb.AddComponent(droneFree.ValueRO.Entity, new DroneToOre
                {
                    Ore = closestOre,
                    Target = targetPos,
                });
            }
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct OreToTeamLeft : IComponentData
{
    public Entity Drone;
}

public struct OreToTeamRight : IComponentData
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
