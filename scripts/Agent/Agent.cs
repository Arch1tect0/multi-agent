using Godot;
using System.Collections.Generic;

public partial class Agent : Node3D
{
	public float DecisionInterval { get; set; } = 10f;
	
	// State
	public Vector3 Position => GlobalPosition;
	
	// Components
	public AgentMemory Memory { get; private set; }
	private LLMService llmService;
	
	// Decision making
	private float decisionTimer = 0f;
	private bool waitingForResponse = false;
	
	public override void _Ready()
	{
		Memory = new AgentMemory();
		AddChild(Memory);
		
		llmService = GetNode<LLMService>("/root/LLMService");
		Memory.AddVisitedPosition(Position);
	}
	
	public override void _Process(double delta)
	{
		decisionTimer += (float)delta;
		
		if (decisionTimer >= DecisionInterval && !waitingForResponse)
		{
			decisionTimer = 0f;
			MakeDecision();
		}
	}
	
	private void MakeDecision()
	{
		var recentActions = Memory.GetRecentActions();
		string recentStr = recentActions.Count > 0 ? string.Join(", ", recentActions) : "None";
		
		string prompt = $@"You are an agent in a grid world.
Current position: {Position}
Recent actions: {recentStr}

Available actions:
- Move(x,z): Move to coordinates x,z (grid is 0-10)
- Collect(): Collect a resource at current position
- Idle(): Do nothing

Respond with ONLY the action, nothing else. Format: Move(5,3) or Collect() or Idle()
Your choice:";
		
		waitingForResponse = true;
		llmService.SendMessage(prompt, OnLLMResponse);
	}
	
	private void OnLLMResponse(string response)
	{
		waitingForResponse = false;
		if (response != "Error") ExecuteAction(response);
	}
	
	private void ExecuteAction(string actionString)
	{
		GD.Print($"Executing: {actionString}");
		
		string action = actionString.Split('(')[0].Trim();
		
		if (action == "Move")
		{
			var coords = ParseMoveParams(actionString);
			if (coords.HasValue) PerformMove(coords.Value);
		}
		else if (action == "Collect")
		{
			PerformCollect();
		}
		else if (action == "Idle")
		{
			PerformIdle();
		}
		
		Memory.AddAction(action);
	}
	
	private Vector2? ParseMoveParams(string actionString)
	{
		try
		{
			var paramsStr = actionString.Split('(')[1].TrimEnd(')');
			var parts = paramsStr.Split(',');
			if (parts.Length >= 2)
			{
				float x = float.Parse(parts[0].Trim());
				float z = float.Parse(parts[1].Trim());
				return new Vector2(x, z);
			}
		}
		catch { }
		return null;
	}
	
	private void PerformMove(Vector2 target)
	{
		Vector3 targetPos = new Vector3(target.X, Position.Y, target.Y);
		GlobalPosition = targetPos; // Just set the position directly
		Memory.AddVisitedPosition(Position);
		GD.Print($"Moving to ({target.X}, {target.Y})");
	}
	
	private void PerformCollect()
	{
		// TODO: Implement collect
	}
	
	private void PerformIdle()
	{
		// TODO: Implement idle
	}
}