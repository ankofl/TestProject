using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;

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

		bool empty = true;

#if UNITY_EDITOR
		if (ui.DroneTrail.value)
		{
			if (!SystemAPI.HasSingleton<DroneTrailDrawRequest>())
			{
				ecb.AddComponent(ecb.CreateEntity(), new DroneTrailDrawRequest { });
			}
		}
		else
		{
			if (SystemAPI.HasSingleton<DroneTrailDrawRequest>())
			{
				ecb.DestroyEntity(SystemAPI.GetSingletonEntity<DroneTrailDrawRequest>());
			}
		}
#else
		if(ui.DroneTrail.text != "Drone Trail (Editor-only)")
		{
			ui.DroneTrail.text = "Drone Trail (Editor-only)";			
			ui.DroneTrail.SetEnabled(false);
		}
#endif

		if (SystemAPI.HasSingleton<OreSpawnRequest>())
		{
			empty = false;

			ui.Restart.text = $"Spawning ores...";
		}
		else if (SystemAPI.HasSingleton<DroneRestartData>())
		{
			var totalCount = 0;

			foreach (var homeDroneRequest in SystemAPI.Query<RefRO<HomeDronesRequest>>())
			{
				totalCount += homeDroneRequest.ValueRO.DronesRemainedCount;
			}

			ui.Restart.text = $"Drones spawns ramained:{totalCount}";

			empty = false;
		}
		else
		{
			if (ui.Restart.text != "Reload")
			{
				ui.Restart.text = "Reload";
			}
		}

		foreach (var (home, team) in SystemAPI.Query<RefRO<Home>, RefRO<Team>>())
		{
			if(home.ValueRO.OresDelivered > 0)
			{
				if(team.ValueRO.CurrentTeam == Teams.Left)
				{
					ui.LeftTeam.text = $"Left Team Ores Count: {home.ValueRO.OresDelivered}";
				}
				else if (team.ValueRO.CurrentTeam == Teams.Right)
				{
					ui.RightTeam.text = $"Right Team Ores Count: {home.ValueRO.OresDelivered}";
				}
			}
		}

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
				foreach (var home in SystemAPI.Query<RefRW<Home>>())
				{
					home.ValueRW.OresDelivered = 0;
				}

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
					OresCount = 1000,
					OreRespawnTime = ui.OreRespawnTime.value,
				});

				ecb.AddComponent(ecb.CreateEntity(), new DroneRestartData
				{
					DronesCount = ui.DronesCount.value,
					DronesSpeed = ui.DronesSpeed.value,
				});
			}
		}


		ecb.Playback(EntityManager);
	}
}

public struct DroneRestartData : IComponentData
{
	public int DronesCount;
	public float DronesSpeed;
}