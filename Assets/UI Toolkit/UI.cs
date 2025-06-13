using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UI : MonoBehaviour
{
	public static UI Instance
	{
		get
		{
			if (instance == null) Debug.LogError("Instance is null");

			return instance;
		}
	}

	public SliderInt DronesCount;
	public SliderInt DronesSpeed;
	public SliderInt OreRespawnTime;

	public Label LeftTeam;
	public Label RightTeam;

	public Toggle DroneTrail;

	public Button Restart;

	void Start()
    {
		var ui = GetComponent<UIDocument>();

		DronesCount = ui.Get(nameof(DronesCount), new(1, 1000, 10));
		DronesSpeed = ui.Get(nameof(DronesSpeed), new(1, 50, 10));
		OreRespawnTime = ui.Get(nameof(OreRespawnTime), new(1, 60, 30));

		LeftTeam = ui.Get<Label>(nameof(LeftTeam));
		RightTeam = ui.Get<Label>(nameof(RightTeam));

		DroneTrail = ui.Get<Toggle>(nameof(DroneTrail));

		Restart = ui.Get<Button>(nameof(Restart));
		Restart.clicked += Restart_clicked;

		instance = this;

		var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

		manager.AddComponentData(manager.CreateEntity(), new UILoaded { });
	}
	private static UI instance;
	public bool Clicked = false;

	private void Restart_clicked()
	{
		Clicked = true;
	}
}

public struct UILoaded : IComponentData
{

}

public static class UIUtils
{
	public static SliderInt Get(this UIDocument ui, string name, int3 value)
	{
		var slider = ui.Get<SliderInt>(name); 

		slider.lowValue = value.x;
		slider.highValue = value.y;
		slider.value = value.z;
		return slider;
	}

	public static T Get<T>(this UIDocument ui, string name) where T : VisualElement
	{
		var elem = ui.rootVisualElement.Q(name);

		if (elem == null) Debug.LogError($"[{name}] not found");

		return elem as T;
	}
}