using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

class OreInDroneAuthoring : MonoBehaviour
{
	class OreInDroneBaker : Baker<OreInDroneAuthoring>
	{
		public override void Bake(OreInDroneAuthoring authoring)
		{
			var oreInDrone = GetEntity(authoring, TransformUsageFlags.Dynamic);
			AddComponent(oreInDrone, new OreInDrone 
			{
				Entity = oreInDrone 
			});
			AddComponent(oreInDrone, new DisableRendering { });
		}
	}
}



public struct OreInDrone : IComponentData
{
    public Entity Entity;
}