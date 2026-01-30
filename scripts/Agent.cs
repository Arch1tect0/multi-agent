using Godot;
using System.Collections.Generic;

public partial class Agent : Node3D
{
	[Export] public string AgentName { get; set; } = "Agent";
	[Export] public float Health { get; set; } = 100f;
	[Export] public float Speed { get; set; } = 5f;
	
	public Dictionary<string, float> Attributes { get; set; } = new Dictionary<string, float>();
	public List<string> Actions { get; set; } = new List<string>();
	
	public override void _Ready()
	{
		Initialize();
	}
	
	protected virtual void Initialize()
	{
		// Override in child classes
	}
	
	public virtual void PerformAction(string actionName)
	{
		if (Actions.Contains(actionName))
		{
			GD.Print($"{AgentName} performs: {actionName}");
		}
	}
}
