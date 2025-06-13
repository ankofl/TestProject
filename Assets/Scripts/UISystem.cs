using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial class UISystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireForUpdate<UILoaded>();
    }
    UI ui;

	protected override void OnUpdate()
    {
		var ecb = new EntityCommandBuffer(Allocator.Temp);

		if (ui == null) ui = UI.Instance;

		bool empty = !SystemAPI.HasSingleton<OreSpawnRequest>() && !SystemAPI.HasSingleton<DroneRespawnData>();

		if (empty)
		{
			ui.Restart.SetEnabled(true);
		}
		else
		{
			ui.Restart.SetEnabled(false);
		}


		if (ui.Clicked)
		{
			ui.Clicked = false;

			if (empty)
			{
				foreach (var drone in SystemAPI.Query<RefRO<Drone>>())
				{
					ecb.DestroyEntity(drone.ValueRO.Entity);
				}
				foreach (var ore in SystemAPI.Query<RefRO<Ore>>())
				{
					ecb.DestroyEntity(ore.ValueRO.Entity);
				}

				ecb.AddComponent(ecb.CreateEntity(), new OreSpawnRequest
				{
					OresCount = 1000
				});

				ecb.AddComponent(ecb.CreateEntity(), new DroneRespawnData
				{
					DronesCount = ui.DronesCount.value,
					DronesSpeed = ui.DronesSpeed.value,
					OreRespawnTime = ui.OreRespawnTime.value,
				});
			}
		}


		ecb.Playback(EntityManager);
	}
}

public struct DroneRespawnData : IComponentData
{
	public int DronesCount;
	public float DronesSpeed;
	public float OreRespawnTime;
}