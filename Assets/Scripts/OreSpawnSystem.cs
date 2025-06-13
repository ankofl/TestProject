using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct OreSpawnSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<Prefabs>();

		state.RequireForUpdate<OreSpawnRequest>();

		rnd = new();
		rnd.InitState();
	}
	Random rnd;

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

		var requestEntity = SystemAPI.GetSingletonEntity<OreSpawnRequest>();
		var requestComp = SystemAPI.GetComponent<OreSpawnRequest>(requestEntity);
		var oresCount = requestComp.OresCount;

		var prefabs = SystemAPI.GetSingleton<Prefabs>();
		var rw = SystemAPI.GetComponentRW<Ore>(prefabs.Ore);
		rw.ValueRW.OreRespawnTime = requestComp.OreRespawnTime;


		var size = oresCount / 10f;

		NativeList<float3> spawnedPos = new(Allocator.Temp);
		for (int i = 0, c = 0; i < oresCount && c < oresCount * 10; c++)
		{

			var pos = rnd.NextFloat3(new(-size, -1f, -size), new(size, -1f, size));

			float closest = size;
			foreach (var item in spawnedPos)
			{
				var curDist = math.distance(item, pos);
				if (curDist < closest)
				{
					closest = curDist;
				}
			}


			if (closest > 2)
			{
				i++;

				spawnedPos.Add(pos);

				var ore = ecb.Instantiate(prefabs.Ore);
				ecb.SetComponent(ore, LocalTransform.FromPositionRotation(pos, quaternion.Euler(math.radians(35), 0, math.radians(45))));
			}
		}


		ecb.RemoveComponent<OreSpawnRequest>(requestEntity);
		ecb.AddComponent(requestEntity, new OreSpawnEnded
		{

		});


		ecb.Playback(state.EntityManager);
	}
}

public struct OreSpawnRequest : IComponentData
{
	public int OresCount;

	public float OreRespawnTime;
}

public struct OreSpawnEnded : IComponentData
{

}
