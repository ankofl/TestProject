using Unity.Entities;
using UnityEngine;

class PrefabsAuthoring : MonoBehaviour
{
	public GameObject Drone;

	public GameObject Ore;

	class PrefabsAuthoringBaker : Baker<PrefabsAuthoring>
	{
		public override void Bake(PrefabsAuthoring authoring)
		{
			var prefabs = GetEntity(TransformUsageFlags.None);

			AddComponent(prefabs, new Prefabs
			{
				Drone = GetEntity(authoring.Drone, TransformUsageFlags.Dynamic),
				Ore = GetEntity(authoring.Ore, TransformUsageFlags.Dynamic),
			});
		}
	}

}


public struct Prefabs : IComponentData
{
	public Entity Drone;

	public Entity Ore;
}
