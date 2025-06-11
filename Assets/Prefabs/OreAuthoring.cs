using Unity.Entities;
using UnityEngine;

class OreAuthoring : MonoBehaviour
{
	class OreAuthoringBaker : Baker<OreAuthoring>
	{
		public override void Bake(OreAuthoring authoring)
		{
			var ore = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(ore, new Ore
			{
				Entity = ore,
			});
		}
	}
}

public struct Ore : IComponentData
{
	public Entity Entity;
}
