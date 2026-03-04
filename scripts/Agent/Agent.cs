using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Agent : Node3D
{
	// ============================================================
	// SIGNALS
	// ============================================================

	[Signal]
	public delegate void ActionCompletedEventHandler(string actionName);

	[Signal]
	public delegate void ActionStartedEventHandler(string actionName);

	// ============================================================
	// COMPONENTS
	// ============================================================

	public AgentMemory Memory { get; private set; }
	private LLMService llmService;
	private WorldState worldState;

	// ============================================================
	// STATE
	// ============================================================

	private bool waitingForResponse = false;
	private bool isActionInProgress = false;

	// ============================================================
	// MOVEMENT
	// ============================================================

	private List<Vector2I> currentPath = new List<Vector2I>();
	private int pathIndex = 0;
	private bool isMoving = false;


	// ============================================================
	// GODOT LIFECYCLE
	// ============================================================

	public override void _Ready()
	{
		Memory = new AgentMemory();
		AddChild(Memory);

		llmService = GetNode<LLMService>("/root/LLMService");
		worldState = GetNode<WorldState>("/root/WorldState");

		Memory.AddVisitedPosition(Position);
		worldState.RegisterAgent(this);

		ActionCompleted += OnActionCompleted;


		Attributes.Weight = GD.Randf() * 100f; // random 0-100
		
		ApplyColor();
		MakeDecision();
	}

	public override void _Process(double delta)
	{
		if (isMoving)
		{
			MoveAlongPath((float)delta);
		}
	}

	// ============================================================
	// MOVEMENT
	// ============================================================

	private void MoveAlongPath(float delta)
	{
		if (pathIndex >= currentPath.Count)
		{
			isMoving = false;
			currentPath.Clear();
			pathIndex = 0;
			Memory.AddVisitedPosition(Position);
			EmitSignal(SignalName.ActionCompleted, "Move");
			return;
		}

		Vector2I targetGrid = currentPath[pathIndex];
		Vector3 targetWorld = worldState.GridToWorld(targetGrid.X, targetGrid.Y);
		targetWorld.Y = Position.Y;

		Vector3 direction = (targetWorld - GlobalPosition).Normalized();
		float distanceToMove = MoveSpeed * delta;
		float distanceToTarget = GlobalPosition.DistanceTo(targetWorld);

		if (distanceToMove >= distanceToTarget)
		{
			GlobalPosition = targetWorld;
			pathIndex++;
		}
		else
		{
			GlobalPosition += direction * distanceToMove;
		}
	}

	public void SetPath(List<Vector2I> path)
	{
		currentPath = path;
		pathIndex = 0;
		isMoving = true;
	}

	// ============================================================
	// DECISION MAKING
	// ============================================================

	private void MakeDecision()
	{
		// Safety net - zero energy is a guaranteed kill
		if (!Attributes.IsAlive) return;
		if (Attributes.Energy <= 0)
		{
			AgentAction.Die(this, worldState);
			return;
		}

		if (isActionInProgress || waitingForResponse)
		{
			GD.Print("Already processing an action or waiting for LLM response");
			return;
		}

		Vector2I currentGrid = worldState.WorldToGrid(GlobalPosition);

		string recentActions = Memory.GetRecentActions().Count > 0
			? string.Join(", ", Memory.GetRecentActions())
			: "None";

		string inventoryStr = Attributes.Inventory.Count > 0
			? string.Join(", ", Attributes.Inventory.Select(o => o.Name))
			: "Empty";

		string worldObjects = worldState.GetAllObjects().Count > 0
			? string.Join("\n", worldState.GetAllObjects().Select(o =>
				$"- {o.Name} at {worldState.WorldToGrid(o.GlobalPosition)}"))
			: "None";

		string availableActions = string.Join("\n", AgentAction.GetAvailableActions());

		string prompt = $@"You are an agent in a grid world.

Position: ({currentGrid.X}, {currentGrid.Y})
Energy: {Attributes.Energy:F0}/{Attributes.MaxEnergy:F0}
Inventory: {inventoryStr}

Objects in world:
{worldObjects}

Recent actions: {recentActions}

Available actions:
{availableActions}

Respond with ONLY one action. Format: Move(5,3) or Collect() or Idle()
Your choice:";

		waitingForResponse = true;
		llmService.SendMessage(prompt, OnLLMResponse);
	}

	private void OnActionCompleted(string actionName)
	{
		isActionInProgress = false;
		GD.Print($"Action '{actionName}' completed. Making next decision...");

		if (!waitingForResponse)
		{
			MakeDecision();
		}
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
			MakeDecision();
		}
	}

	// ============================================================
	// ACTION EXECUTION
	// ============================================================

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
				AgentAction.Move(this, worldState, coords.Value);
			}
			else
			{
				EmitSignal(SignalName.ActionCompleted, action);
			}
		}
		else if (action == "Collect")
		{
			AgentAction.Collect(this, worldState);
			EmitSignal(SignalName.ActionCompleted, action);
		}
		else if (action == "Idle")
		{
			AgentAction.Idle(this);
			EmitSignal(SignalName.ActionCompleted, action);
		}
		else if (action == "Die")
		{
			AgentAction.Die(this, worldState);
		}
		else
		{
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