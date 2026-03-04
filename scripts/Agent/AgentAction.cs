using Godot;
using System.Collections.Generic;

public static class AgentAction
{

	public static List<string> GetAvailableActions() // --> this is the list of actions that the LLM can choose from when deciding what to do. Each action corresponds to a method in this class that implements the behavior for that action.
	{
		return new List<string>
	{
		"Move(x,z): Move to grid coordinates",
		"Collect(): Collect object at current position",
		"Idle(): Do nothing",
		"Die(): Agent dies"
	};
	}
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
		Vector2I agentGrid = worldState.WorldToGrid(agent.GlobalPosition);
		var obj = worldState.GetObjectAtGrid(agentGrid);

		if (obj == null)
		{
			GD.Print($"No object to collect at {agentGrid}");
			return;
		}

		agent.Attributes.AddToInventory(obj);  // <-- add to inventory
		worldState.UnregisterObject(obj);
		GD.Print($"Collected {obj.Name}! Inventory size: {agent.Attributes.Inventory.Count}");
	}


	public static void Idle(Agent agent)
	{
		GD.Print("Idling...");
		// Event emission handled in ExecuteAction
	}

	// the Die action is called from Agent.cs when energy reaches 0, so it doesn't need to be triggered by the LLM. It can be called directly from the Agent class when the condition is met.
	public static void Die(Agent agent, WorldState worldState)
	{
		agent.Attributes.IsAlive = false;
		agent.Attributes.Energy = 0;
		GD.Print($"{agent.Name} has died!");
		worldState.UnregisterAgent(agent);
		agent.QueueFree();
	}
}