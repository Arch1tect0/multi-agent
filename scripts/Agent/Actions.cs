using Godot;
using System.Collections.Generic;

public static class Actions
{
	public static void Move(Agent agent, WorldState worldState, Vector2 target)
	{
		Vector2I startGrid = worldState.WorldToGrid(agent.GlobalPosition);
		Vector2I endGrid = new Vector2I((int)target.X, (int)target.Y);

		var path = worldState.FindPath(startGrid, endGrid);

		if (path.Count > 0)
		{
			GD.Print($"Path found with {path.Count} steps");
			agent.SetPath(path);
			// Movement completion is handled in MoveAlongPath
		}
		else
		{
			GD.Print("No path found!");
			// No valid path, emit completion immediately
			agent.EmitSignal(Agent.SignalName.ActionCompleted, "Move");
		}
	}

	public static void Collect(Agent agent, WorldState worldState)
	{
		// Convert agent position to grid
		Vector2I agentGrid = worldState.WorldToGrid(agent.GlobalPosition);

		// Check if an object exists on this grid cell
		var obj = worldState.GetObjectAtGrid(agentGrid);

		if (obj == null)
		{
			GD.Print($"No object to collect at {agentGrid}");
			return;
		}

		// Collect it
		GD.Print($"Collected object at {agentGrid}!");
		worldState.UnregisterObject(obj);
	}


	public static void Idle(Agent agent)
	{
		GD.Print("Idling...");
		// Event emission handled in ExecuteAction
	}
}