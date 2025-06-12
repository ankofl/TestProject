using Unity.Entities;
using UnityEngine;

class HomeAuthoring : MonoBehaviour
{
	public int DronesCount;
	class HomeAuthoringBaker : Baker<HomeAuthoring>
	{
		public override void Bake(HomeAuthoring authoring)
		{
			var home = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(home, new Home
			{
				Entity = home,

			});

			AddComponent(home, new HomeDronesRequest
			{
				TimeToNextWave = 0,
				DronesRemainedCount = authoring.DronesCount,
			});
		}
	}
}

public struct HomeDronesRequest : IComponentData
{
	public float TimeToNextWave;

	public int DronesRemainedCount;
}


public struct Home : IComponentData
{
	public Entity Entity;
}