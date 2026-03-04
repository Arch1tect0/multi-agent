using Godot;
using System.Collections.Generic;

public class AgentAttributes
{
    // ============================================================
    // WEIGHT
    // ============================================================

    public float Weight { get; set; } = 50f;
    public float Speed => Mathf.Lerp(4f, 0.5f, Weight / 100f);
    public Color Color => WeightToColor();

    // ============================================================
    // ENERGY
    // ============================================================

    public float MaxEnergy { get; set; } = 100f;
    public float Energy { get; set; } = 100f;
    public bool IsAlive { get; set; } = true;

    public void ConsumeEnergy(float amount)
    {
        Energy = Mathf.Max(0, Energy - amount);
    }

    // ============================================================
    // INVENTORY
    // ============================================================

    public List<Node3D> Inventory { get; private set; } = new();

    public bool AddToInventory(Node3D obj)
    {
        Inventory.Add(obj);
        return true;
    }

    public bool RemoveFromInventory(Node3D obj)
    {
        return Inventory.Remove(obj);
    }

    public bool HasItem(Node3D obj) => Inventory.Contains(obj);

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private Color WeightToColor()
    {
        float t = Weight / 100f;
        return t < 0.5f
            ? Colors.Green.Lerp(Colors.Yellow, t * 2f)
            : Colors.Yellow.Lerp(Colors.Red, (t - 0.5f) * 2f);
    }
}