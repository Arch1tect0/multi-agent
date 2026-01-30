using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AgentMemory : Node
{
	private Dictionary<string, object> memory = new Dictionary<string, object>
	{
		{ "visited_positions", new List<Vector3>() },
		{ "known_resources", new List<Vector3>() },
		{ "action_history", new List<string>() }
	};
	
	private const int MAX_HISTORY = 20;
	
	public void AddVisitedPosition(Vector3 position)
	{
		((List<Vector3>)memory["visited_positions"]).Add(position);
	}
	
	public void AddKnownResource(Vector3 resourcePosition)
	{
		((List<Vector3>)memory["known_resources"]).Add(resourcePosition);
	}
	
	public void AddAction(string action)
	{
		var history = (List<string>)memory["action_history"];
		history.Add(action);
		
		if (history.Count > MAX_HISTORY)
		{
			history.RemoveAt(0);
		}
	}
	
	public List<string> GetRecentActions(int count = 5)
	{
		return ((List<string>)memory["action_history"]).TakeLast(count).ToList();
	}
	
	public int GetVisitedCount()
	{
		return ((List<Vector3>)memory["visited_positions"]).Count;
	}
	
	public List<Vector3> GetKnownResources()
	{
		return (List<Vector3>)memory["known_resources"];
	}
}