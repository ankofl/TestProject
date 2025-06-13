using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

partial struct DroneToOreReorderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ore>();

        state.RequireForUpdate<Drone>();   
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var ore in SystemAPI.Query<RefRO<Ore>>().WithAll<OreReload>().WithAny<OreToTeamLeft, OreToTeamRight>())
		{
			if (SystemAPI.HasComponent<OreToTeamLeft>(ore.ValueRO.Entity))
			{
				var oreToTeamLeft = SystemAPI.GetComponent<OreToTeamLeft>(ore.ValueRO.Entity);

				if (SystemAPI.HasComponent<DroneToOre>(oreToTeamLeft.Drone))
				{
					ecb.RemoveComponent<DroneToOre>(oreToTeamLeft.Drone);
				}

				ecb.RemoveComponent<OreToTeamLeft>(ore.ValueRO.Entity);
			}
			else if(SystemAPI.HasComponent<OreToTeamRight>(ore.ValueRO.Entity))
			{
				var oreToTeamRight = SystemAPI.GetComponent<OreToTeamRight>(ore.ValueRO.Entity);

				if (SystemAPI.HasComponent<DroneToOre>(oreToTeamRight.Drone))
				{
					ecb.RemoveComponent<DroneToOre>(oreToTeamRight.Drone);
				}

				ecb.RemoveComponent<OreToTeamRight>(ore.ValueRO.Entity);
			}
		}

		ecb.Playback(state.EntityManager);
    }
}
