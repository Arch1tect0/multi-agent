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
		var obj = worldState.GetFirstObject();

		if (obj == null)
		{
			GD.Print("No object to collect!");
			return;
		}

		// Check if agent is on the same grid cell as the object
		Vector2I agentGrid = worldState.WorldToGrid(agent.GlobalPosition);
		Vector2I objGrid = worldState.WorldToGrid(obj.GlobalPosition);

		if (agentGrid == objGrid)
		{
			GD.Print($"Successfully collected object at ({objGrid.X}, {objGrid.Y})!");
			worldState.UnregisterObject(obj); // This now handles removal too
		}
		else
		{
			GD.Print($"Too far from object! Agent at ({agentGrid.X}, {agentGrid.Y}), Object at ({objGrid.X}, {objGrid.Y})");
		}
	}

	public static void Idle(Agent agent)
	{
		GD.Print("Idling...");
		// Event emission handled in ExecuteAction
	}
}