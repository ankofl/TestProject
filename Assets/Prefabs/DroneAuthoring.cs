using Unity.Entities;
using UnityEngine;

class DroneAuthoring : MonoBehaviour
{
	class DroneAuthoringBaker : Baker<DroneAuthoring>
	{
		public override void Bake(DroneAuthoring authoring)
		{
			var drone = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(drone, new Drone
			{ 
				Entity = drone,
			});

			AddComponent(drone, new DroneToBufferRequest
			{
				Team = Teams.None,
			});
		}
	}
}

public struct Drone : IComponentData
{
	public Entity Entity;
}

public struct DroneToBufferRequest : IComponentData
{
	public Teams Team;
}


