using Unity.Entities;
using Unity.Mathematics;
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
				color.color = TeamsColor.Left;
			}
			else if(authoring.Team == Teams.Right)
			{
				color.color = TeamsColor.Right;
			}
			else
			{
				color.color = Color.black;
			}

			foreach (var child in GetChildren(true))
			{
				if(!child.TryGetComponent<OreInDroneAuthoring>(out _))
				{
					if (child.TryGetComponent<URPMaterialPropertyBaseColorAuthoring>(out var childColor))
					{
						childColor.color = color.color;
					}
				}				
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

public readonly struct TeamsColor
{
	public static float4 To4(Color color)
	{
		return new float4 { x = color.r, y = color.g, z = color.b, w = color.a };
	}

	public static readonly Color Left = new() { r = 1, g = 0, b = 0, a = 1, };
	public static readonly Color LeftInactive = new() { r = c, g = 0, b = 0, a = 1, };
	public static readonly Color Right = new() { r = 0, g = 0, b = 1, a = 1, };
	public static readonly Color RightInactive = new() { r = 0, g = 0, b = c, a = 1, };

	private const float c = 0.1f;
}

public struct Team : IComponentData
{
	public Teams CurrentTeam;
	public Entity Entity;
}


