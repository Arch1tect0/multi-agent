using Godot;
using System.Collections.Generic;

public partial class Agent : Node3D
{
	public float DecisionInterval { get; set; } = 10f;
	public float MoveSpeed { get; set; } = 2f; // Units per second
	
	// State
	public Vector3 Position => GlobalPosition;
	
	// Components
	public AgentMemory Memory { get; private set; }
	private LLMService llmService;
	private WorldState worldState;
	
	// Decision making
	private float decisionTimer = 0f;
	private bool waitingForResponse = false;
	
	// Movement
	private List<Vector2I> currentPath = new List<Vector2I>();
	private int pathIndex = 0;
	private bool isMoving = false;
	
	public override void _Ready()
	{
		Memory = new AgentMemory();
		AddChild(Memory);
		
		llmService = GetNode<LLMService>("/root/LLMService");
		worldState = GetNode<WorldState>("/root/WorldState");
		
		Memory.AddVisitedPosition(Position);
		worldState.RegisterAgent(this);
	}
	
	public override void _Process(double delta)
	{
		// Handle movement along path
		if (isMoving)
		{
			MoveAlongPath((float)delta);
		}
		else
		{
			// Only make decisions when not moving
			decisionTimer += (float)delta;
			
			if (decisionTimer >= DecisionInterval && !waitingForResponse)
			{
				decisionTimer = 0f;
				MakeDecision();
			}
		}
	}
	
	private void MoveAlongPath(float delta)
	{
		if (pathIndex >= currentPath.Count)
		{
			// Reached destination
			isMoving = false;
			currentPath.Clear();
			pathIndex = 0;
			Memory.AddVisitedPosition(Position);
			GD.Print("Reached destination!");
			return;
		}
		
		// Get target position for current waypoint
		Vector2I targetGrid = currentPath[pathIndex];
		Vector3 targetWorld = worldState.GridToWorld(targetGrid.X, targetGrid.Y);
		targetWorld.Y = Position.Y; // Keep same height
		
		// Move towards target
		Vector3 direction = (targetWorld - GlobalPosition).Normalized();
		float distanceToMove = MoveSpeed * delta;
		float distanceToTarget = GlobalPosition.DistanceTo(targetWorld);
		
		if (distanceToMove >= distanceToTarget)
		{
			// Reached this waypoint
			GlobalPosition = targetWorld;
			pathIndex++;
			GD.Print($"Reached waypoint {pathIndex}/{currentPath.Count}");
		}
		else
		{
			// Move towards waypoint
			GlobalPosition += direction * distanceToMove;
		}
	}
	
private void MakeDecision()
{
	var recentActions = Memory.GetRecentActions();
	string recentStr = recentActions.Count > 0 ? string.Join(", ", recentActions) : "None";
	
	// Get object location from WorldState
	var obj = worldState.GetFirstObject();
	Vector2I currentGrid = worldState.WorldToGrid(GlobalPosition);
	
	string objectInfo = "No objects found";
	if (obj != null)
	{
		Vector2I objGrid = worldState.WorldToGrid(obj.GlobalPosition);
		objectInfo = $"Object at grid position ({objGrid.X}, {objGrid.Y})";
	}
	
	string prompt = $@"You are an agent in a grid world trying to find and collect an object.

Current grid position: ({currentGrid.X}, {currentGrid.Y})
{objectInfo}
Recent actions: {recentStr}

Available actions:
- Move(x,z): Move to grid coordinates x,z
- Collect(): Collect object at current position
- Idle(): Do nothing

Your goal is to reach the object and collect it.
Respond with ONLY the action. Format: Move(5,3) or Collect() or Idle()
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
		// Convert current and target positions to grid coordinates
		Vector2I startGrid = worldState.WorldToGrid(GlobalPosition);
		Vector2I endGrid = new Vector2I((int)target.X, (int)target.Y);
		
		// Find path using WorldState
		currentPath = worldState.FindPath(startGrid, endGrid);
		
		if (currentPath.Count > 0)
		{
			GD.Print($"Path found with {currentPath.Count} steps");
			pathIndex = 0;
			isMoving = true;
		}
		else
		{
			GD.Print("No path found!");
		}
	}
	
	private void PerformCollect()
	{
		// TODO: Implement collect
		GD.Print("Collecting...");
	}
	
	private void PerformIdle()
	{
		// TODO: Implement idle
		GD.Print("Idling...");
	}
}