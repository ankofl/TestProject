using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

partial struct DroneToHomeSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<Drone>();
		state.RequireForUpdate<Home>();
	}

	[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

		foreach (var (drone, tran, toHome, velocity) in
			   SystemAPI.Query<RefRO<Drone>, RefRW<LocalTransform>, RefRO<DroneToHome>, RefRW<PhysicsVelocity>>()
			   .WithNone<DroneMining, DroneToOre>())
		{
			var dist = math.distance(tran.ValueRO.Position, toHome.ValueRO.HomePos);

			if (dist > 3.5f)
			{
				var dir = math.normalize(toHome.ValueRO.HomePos - tran.ValueRO.Position);

				velocity.ValueRW.Linear = dir * drone.ValueRO.Speed;
			}
			else
			{
				foreach (var child in SystemAPI.GetBuffer<Child>(drone.ValueRO.Entity))
				{
					if (SystemAPI.HasComponent<OreInDrone>(child.Value))
					{
						ecb.AddComponent(child.Value, new DisableRendering { });
					}
				}

				ecb.RemoveComponent<DroneToHome>(drone.ValueRO.Entity);
			}
		}

		ecb.Playback(state.EntityManager);
	}
}
