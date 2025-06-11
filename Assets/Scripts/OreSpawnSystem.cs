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

		state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new OreSpawnRequest { OresCount = 10 });

		rnd = new();
		rnd.InitState();
	}
	Random rnd;

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

		var prefabs = SystemAPI.GetSingleton<Prefabs>();

		var requestEntity = SystemAPI.GetSingletonEntity<OreSpawnRequest>();
		var oresCount = SystemAPI.GetComponent<OreSpawnRequest>(requestEntity).OresCount;
		ecb.DestroyEntity(requestEntity);

		NativeList<float3> spawnedPos = new(Allocator.Temp);
		for (int i = 0, c = 0; i < oresCount && c < oresCount * 10; c++)
		{

			var pos = rnd.NextFloat3(new(-10, 0, -10), new(10, 0, 10));

			float closest = 10;
			foreach (var item in spawnedPos)
			{
				var curDist = math.distance(item, pos);
				if (curDist < closest)
				{
					closest = curDist;
				}
			}


			if (closest > 3)
			{
				i++;

				spawnedPos.Add(pos);

				var ore = ecb.Instantiate(prefabs.Ore);
				ecb.SetComponent(ore, LocalTransform.FromPosition(pos));
			}
		}


		ecb.Playback(state.EntityManager);
	}

	public struct OreSpawnRequest : IComponentData
	{
		public int OresCount;
	}
}
