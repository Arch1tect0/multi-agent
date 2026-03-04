using Godot;

public partial class Agent : Node3D
{
    public AgentAttributes Attributes { get; private set; } = new();

    public float MoveSpeed => Attributes.Speed;


    private void ApplyColor()
    {
        var mesh = GetNodeOrNull<MeshInstance3D>("Mesh");
        if (mesh == null) return;

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = Attributes.Color;
        mesh.MaterialOverride = mat;
    }
}