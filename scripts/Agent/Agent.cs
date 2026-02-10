using Godot;
using System.Collections.Generic;

public partial class Agent : Node3D
{
	// Events
	[Signal]
	public delegate void ActionCompletedEventHandler(string actionName);

	[Signal]
	public delegate void ActionStartedEventHandler(string actionName);

	public float MoveSpeed { get; set; } = 2f;

	// State
	public Vector3 Position => GlobalPosition;

	// Components
	public AgentMemory Memory { get; private set; }
	private LLMService llmService;
	private WorldState worldState;

	// Decision making
	private bool waitingForResponse = false;
	private bool isActionInProgress = false;

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

		// Connect to our own ActionCompleted event
		ActionCompleted += OnActionCompleted;

		// Start the decision-making loop
		MakeDecision();
	}

	public override void _Process(double delta)
	{
		// Handle movement along path
		if (isMoving)
		{
			MoveAlongPath((float)delta);
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
			//GD.Print("Reached destination!");

			// Emit completion event
			EmitSignal(SignalName.ActionCompleted, "Move");
			return;
		}

		// Get target position for current waypoint
		Vector2I targetGrid = currentPath[pathIndex];
		Vector3 targetWorld = worldState.GridToWorld(targetGrid.X, targetGrid.Y);
		targetWorld.Y = Position.Y;

		// Move towards target
		Vector3 direction = (targetWorld - GlobalPosition).Normalized();
		float distanceToMove = MoveSpeed * delta;
		float distanceToTarget = GlobalPosition.DistanceTo(targetWorld);

		if (distanceToMove >= distanceToTarget)
		{
			GlobalPosition = targetWorld;
			pathIndex++;
			//GD.Print($"Reached waypoint {pathIndex}/{currentPath.Count}");
		}
		else
		{
			GlobalPosition += direction * distanceToMove;
		}
	}

	private void OnActionCompleted(string actionName)
	{
		isActionInProgress = false;
		GD.Print($"Action '{actionName}' completed. Making next decision...");

		// Automatically make next decision when action completes
		if (!waitingForResponse)
		{
			MakeDecision();
		}
	}

	private void MakeDecision()
	{
		if (isActionInProgress || waitingForResponse)
		{
			GD.Print("Already processing an action or waiting for LLM response");
			return;
		}

		var recentActions = Memory.GetRecentActions();
		string recentStr = recentActions.Count > 0 ? string.Join(", ", recentActions) : "None";

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
		if (response != "Error")
		{
			ExecuteAction(response);
		}
		else
		{
			// On error, make decision again
			MakeDecision();
		}
	}

	public void SetPath(List<Vector2I> path)
	{
		currentPath = path;
		pathIndex = 0;
		isMoving = true;
	}

	private void ExecuteAction(string actionString)
	{
		GD.Print($"Executing: {actionString}");

		string action = actionString.Split('(')[0].Trim();
		isActionInProgress = true;

		EmitSignal(SignalName.ActionStarted, action);

		if (action == "Move")
		{
			var coords = ParseMoveParams(actionString);
			if (coords.HasValue)
			{
				Actions.Move(this, worldState, coords.Value);
			}
			else
			{
				// Invalid move parameters, complete immediately
				EmitSignal(SignalName.ActionCompleted, action);
			}
		}
		else if (action == "Collect")
		{
			Actions.Collect(this, worldState);
			// Collect completes immediately
			EmitSignal(SignalName.ActionCompleted, action);
		}
		else if (action == "Idle")
		{
			Actions.Idle(this);
			// Idle completes immediately
			EmitSignal(SignalName.ActionCompleted, action);
		}
		else
		{
			// Unknown action, complete immediately
			isActionInProgress = false;
			MakeDecision();
			return;
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
}