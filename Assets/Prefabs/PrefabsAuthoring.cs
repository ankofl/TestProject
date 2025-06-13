using Unity.Entities;
using UnityEngine;

class PrefabsAuthoring : MonoBehaviour
{
	public GameObject Drone;

	public GameObject Ore;

	public GameObject OreInDrone;

	class PrefabsAuthoringBaker : Baker<PrefabsAuthoring>
	{
		public override void Bake(PrefabsAuthoring authoring)
		{
			var prefabs = GetEntity(TransformUsageFlags.None);

			AddComponent(prefabs, new Prefabs
			{
				Drone = GetEntity(authoring.Drone, TransformUsageFlags.Dynamic),
				Ore = GetEntity(authoring.Ore, TransformUsageFlags.Dynamic),
				OreInDrone = GetEntity(authoring.OreInDrone, TransformUsageFlags.Dynamic),
			});
		}
	}

}


public struct Prefabs : IComponentData
{
	public Entity Drone;

	public Entity Ore;

	public Entity OreInDrone;
}
