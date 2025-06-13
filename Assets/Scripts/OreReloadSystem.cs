using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

partial struct OreReloadSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Ore>().WithAll<OreReload>().Build());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var ore in SystemAPI.Query<RefRO<Ore>>().WithAll<OreReload>().WithNone<DisableRendering>())
		{
			ecb.AddComponent<DisableRendering>(ore.ValueRO.Entity);
		}


		foreach (var (ore, oreReload) in SystemAPI.Query<RefRO<Ore>, RefRW<OreReload>>())
		{
			if (oreReload.ValueRO.TimeRemained > 0)
			{
				oreReload.ValueRW.TimeRemained -= SystemAPI.Time.DeltaTime;
			}
			else
			{
				ecb.RemoveComponent<DisableRendering>(ore.ValueRO.Entity);
				ecb.RemoveComponent<OreReload>(ore.ValueRO.Entity);
			}
		}

		ecb.Playback(state.EntityManager);

    }
}

public struct OreReload : IComponentData
{
    public float TimeRemained;
}