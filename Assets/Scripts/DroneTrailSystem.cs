using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial class DroneTrailSystem : SystemBase
{
	protected override void OnCreate()
    {
        RequireForUpdate<DroneTrailDrawRequest>();
    }

    protected override void OnUpdate()
    {
        foreach (var (drone, team, droneToOre, tran) in 
            SystemAPI.Query<RefRO<Drone>, RefRO<Team>, RefRO<DroneToOre>, RefRO<LocalTransform>>())
        {
            Color color = Color.blue;
            if(team.ValueRO.CurrentTeam == Teams.Left)
            {
                color = TeamsColor.Left;
            }
            else if (team.ValueRO.CurrentTeam == Teams.Right)
			{
				color = TeamsColor.Right;
			}

			Debug.DrawLine(tran.ValueRO.Position, droneToOre.ValueRO.Target, color);
        }
    }
}

public struct DroneTrailDrawRequest : IComponentData
{
    public bool moak;
}