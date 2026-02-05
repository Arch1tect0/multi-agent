using Godot;
using System.Collections.Generic;

public class PathNode
{
	public Vector2I Position;
	public PathNode Parent;
	public float GCost;
	public float HCost;
	public float FCost => GCost + HCost;
}

public class Pathfinding
{
	private int gridSize;
	
	public Pathfinding(int size)
	{
		gridSize = size;
	}
	
	public List<Vector2I> FindPath(Vector2I start, Vector2I end)
	{
		var openList = new List<PathNode>();
		var closedList = new HashSet<Vector2I>();
		
		var startNode = new PathNode { Position = start, GCost = 0, HCost = GetDistance(start, end) };
		openList.Add(startNode);
		
		while (openList.Count > 0)
		{
			var currentNode = GetLowestFCost(openList);
			
			if (currentNode.Position == end)
				return RetracePath(currentNode);
			
			openList.Remove(currentNode);
			closedList.Add(currentNode.Position);
			
			foreach (var neighbor in GetNeighbors(currentNode.Position))
			{
				if (closedList.Contains(neighbor))
					continue;
				
				float newGCost = currentNode.GCost + 1;
				var neighborNode = openList.Find(n => n.Position == neighbor);
				
				if (neighborNode == null)
				{
					neighborNode = new PathNode
					{
						Position = neighbor,
						Parent = currentNode,
						GCost = newGCost,
						HCost = GetDistance(neighbor, end)
					};
					openList.Add(neighborNode);
				}
				else if (newGCost < neighborNode.GCost)
				{
					neighborNode.GCost = newGCost;
					neighborNode.Parent = currentNode;
				}
			}
		}
		
		return new List<Vector2I>();
	}
	
	private PathNode GetLowestFCost(List<PathNode> list)
	{
		PathNode lowest = list[0];
		foreach (var node in list)
		{
			if (node.FCost < lowest.FCost)
				lowest = node;
		}
		return lowest;
	}
	
	private List<Vector2I> GetNeighbors(Vector2I pos)
	{
		var neighbors = new List<Vector2I>();
		Vector2I[] directions = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
		
		foreach (var dir in directions)
		{
			Vector2I neighbor = pos + dir;
			
			if (neighbor.X >= 0 && neighbor.X < gridSize && 
			    neighbor.Y >= 0 && neighbor.Y < gridSize)
			{
				neighbors.Add(neighbor);
			}
		}
		
		return neighbors;
	}
	
	private float GetDistance(Vector2I a, Vector2I b)
	{
		return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
	}
	
	private List<Vector2I> RetracePath(PathNode endNode)
	{
		var path = new List<Vector2I>();
		var currentNode = endNode;
		
		while (currentNode != null)
		{
			path.Add(currentNode.Position);
			currentNode = currentNode.Parent;
		}
		
		path.Reverse();
		return path;
	}
}