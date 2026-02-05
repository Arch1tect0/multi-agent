using Godot;
using System.Collections.Generic;

public partial class WorldState : Node
{
	private GridGenerator3D grid;
	private Pathfinding pathfinding;
	
	private List<Agent> agents = new List<Agent>();
	private List<Node3D> objects = new List<Node3D>();
	
	public override void _Ready()
	{
		GD.Print("WorldState initialized!");
	}
	
	public void SetGrid(GridGenerator3D gridGenerator)
	{
		grid = gridGenerator;
		pathfinding = new Pathfinding(grid.GridSize);
		GD.Print($"Grid registered: {grid.GridSize}x{grid.GridSize}");
	}
	
	public GridGenerator3D GetGrid()
	{
		return grid;
	}
	
	// ============================================================
	// REGISTRATION
	// ============================================================
	
	public void RegisterAgent(Agent agent)
	{
		if (!agents.Contains(agent))
		{
			agents.Add(agent);
			GD.Print($"Agent registered at {agent.Position}. Total: {agents.Count}");
		}
	}
	
	public void UnregisterAgent(Agent agent)
	{
		agents.Remove(agent);
		GD.Print($"Agent unregistered. Total: {agents.Count}");
	}
	
	public void RegisterObject(Node3D obj)
	{
		if (!objects.Contains(obj))
		{
			objects.Add(obj);
			GD.Print($"Object registered at {obj.Position}. Total: {objects.Count}");
		}
	}
	
	public void UnregisterObject(Node3D obj)
	{
		objects.Remove(obj);
		GD.Print($"Object unregistered. Total: {objects.Count}");
	}
	
	// ============================================================
	// PATHFINDING
	// ============================================================
	
	public List<Vector2I> FindPath(Vector2I start, Vector2I end)
	{
		return pathfinding.FindPath(start, end);
	}
	
	// ============================================================
	// QUERIES
	// ============================================================
	
	public List<Agent> GetAllAgents()
	{
		return new List<Agent>(agents);
	}
	
	public List<Node3D> GetAllObjects()
	{
		return new List<Node3D>(objects);
	}
	
	// Returns the first registered object, For testing purposes and simplicity
	public Node3D GetFirstObject()
	{
	if (objects.Count > 0)
		return objects[0];
	return null;
	}


	// ============================================================
	// UTILITIES
	// ============================================================
	
	public Vector2I WorldToGrid(Vector3 worldPos)
	{
		int x = Mathf.RoundToInt(worldPos.X / grid.CellSize);
		int z = Mathf.RoundToInt(worldPos.Z / grid.CellSize);
		return new Vector2I(x, z);
	}
	
	public Vector3 GridToWorld(int x, int z)
	{
		return new Vector3(x * grid.CellSize, 0, z * grid.CellSize);
	}
}