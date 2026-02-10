using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text;

public partial class AgentMemory : Node
{
    private List<Vector3> visitedPositions = new List<Vector3>();
    private List<string> actionHistory = new List<string>();
    private int maxHistorySize = 10;
    private string agentId;

    public override void _Ready()
    {
        // Generate unique ID for this agent
        agentId = GetParent().Name; // Uses agent's node name
    }

    public void AddVisitedPosition(Vector3 position)
    {
        visitedPositions.Add(position);
    }

    public List<Vector3> GetVisitedPositions()
    {
        return new List<Vector3>(visitedPositions);
    }

    public void AddAction(string action)
    {
        actionHistory.Add(action);
        if (actionHistory.Count > maxHistorySize)
        {
            actionHistory.RemoveAt(0);
        }
    }

    public List<string> GetRecentActions()
    {
        return new List<string>(actionHistory);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            SaveToDesktop();
        }
    }

    private void SaveToDesktop()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        // Create "Log" folder on Desktop
        string logFolder = Path.Combine(desktopPath, "Log");
        if (!Directory.Exists(logFolder))
            Directory.CreateDirectory(logFolder);

        string filename = $"agent_{agentId}_memory.txt";
        string fullPath = Path.Combine(logFolder, filename);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"AGENT MEMORY LOG - {agentId}");
        sb.AppendLine("================");
        sb.AppendLine();

        sb.AppendLine("Actions:");
        foreach (var action in actionHistory)
        {
            sb.AppendLine($"- {action}");
        }

        sb.AppendLine();
        sb.AppendLine("Visited Positions:");
        foreach (var pos in visitedPositions)
        {
            sb.AppendLine($"- ({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1})");
        }

        File.WriteAllText(fullPath, sb.ToString());
        GD.Print($"Memory saved to: {fullPath}");
    }

}