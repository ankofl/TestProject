using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[RequireComponent(typeof(URPMaterialPropertyBaseColorAuthoring))]
class TeamsAuthoring : MonoBehaviour
{

	public Teams Team;

	class TeamsAuthoringBaker : Baker<TeamsAuthoring>
	{
		public override void Bake(TeamsAuthoring authoring)
		{
			var color = authoring.GetComponent<URPMaterialPropertyBaseColorAuthoring>();
			if(authoring.Team == Teams.Left)
			{
				color.color = Color.red;
			}
			else if(authoring.Team == Teams.Right)
			{
				color.color = Color.blue;
			}
			else
			{
				color.color = Color.black;
			}

			var team = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(team, new Team
			{
				Entity = team,

				CurrentTeam = authoring.Team,
			});
		}
	}
}

public enum Teams
{
	None, Left, Right,
}

public struct Team : IComponentData
{
	public Teams CurrentTeam;
	public Entity Entity;
}


