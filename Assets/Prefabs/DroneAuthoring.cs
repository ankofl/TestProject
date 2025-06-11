using Unity.Entities;
using UnityEngine;

class DroneAuthoring : MonoBehaviour
{
	public float Speed;
	class DroneAuthoringBaker : Baker<DroneAuthoring>
	{
		public override void Bake(DroneAuthoring authoring)
		{
			var drone = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(drone, new Drone
			{
				Speed = authoring.Speed,
				Entity = drone,
			});
		}
	}
}

public struct Drone : IComponentData
{
	public float Speed;
	public Entity Entity;
}


