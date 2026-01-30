using Godot;
using System;

[Tool]
public partial class GridGenerator3D : Node3D
{
	private int _gridSize = 10;
	private float _cellSize = 1.0f;
	private float _cellHeight = 0.1f;
	private Color _gridColor = Colors.White;
	private Color _alternateColor = Colors.Gray;
	
	[Export] 
	public int GridSize 
	{ 
		get => _gridSize;
		set 
		{
			_gridSize = value;
			if (Engine.IsEditorHint() && IsInsideTree())
				GenerateGrid();
		}
	}
	
	[Export] 
	public float CellSize 
	{ 
		get => _cellSize;
		set 
		{
			_cellSize = value;
			if (Engine.IsEditorHint() && IsInsideTree())
				GenerateGrid();
		}
	}
	
	[Export] 
	public float CellHeight 
	{ 
		get => _cellHeight;
		set 
		{
			_cellHeight = value;
			if (Engine.IsEditorHint() && IsInsideTree())
				GenerateGrid();
		}
	}
	
	[Export] 
	public Color GridColor 
	{ 
		get => _gridColor;
		set 
		{
			_gridColor = value;
			if (Engine.IsEditorHint() && IsInsideTree())
				GenerateGrid();
		}
	}
	
	[Export] 
	public Color AlternateColor 
	{ 
		get => _alternateColor;
		set 
		{
			_alternateColor = value;
			if (Engine.IsEditorHint() && IsInsideTree())
				GenerateGrid();
		}
	}
	
	private MeshInstance3D[,] cells;
	
	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
			GenerateGrid();
	}
	
	public void GenerateGrid()
	{
		ClearGrid();
		
		cells = new MeshInstance3D[GridSize, GridSize];
		
		for (int x = 0; x < GridSize; x++)
		{
			for (int z = 0; z < GridSize; z++)
			{
				var mesh = new MeshInstance3D();
				var box = new BoxMesh();
				box.Size = new Vector3(CellSize, CellHeight, CellSize);
				mesh.Mesh = box;
				
				// Position on XZ plane
				mesh.Position = new Vector3(x * CellSize, 0, z * CellSize);
				
				// Create material with color
				var material = new StandardMaterial3D();
				bool isAlternate = (x + z) % 2 == 0;
				material.AlbedoColor = isAlternate ? GridColor : AlternateColor;
				mesh.SetSurfaceOverrideMaterial(0, material);
				
				AddChild(mesh);
				if (Engine.IsEditorHint())
				{
					mesh.Owner = GetTree().EditedSceneRoot;
				}
				cells[x, z] = mesh;
			}
		}
	}
	
	public void ClearGrid()
	{
		foreach (Node child in GetChildren())
		{
			if (child is MeshInstance3D)
			{
				RemoveChild(child);
				child.QueueFree();
			}
		}
		cells = null;
	}
	
	public MeshInstance3D GetCell(int x, int z)
	{
		if (cells != null && x >= 0 && x < GridSize && z >= 0 && z < GridSize)
			return cells[x, z];
		return null;
	}
}
