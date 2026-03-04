using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectSpawner : Node3D
{
	[Export] public PackedScene[] ObjectPrefabs;
	[Export] public int SpawnCount = 10;
	[Export] public NodePath GridPath;
	[Export] public float SpawnHeight = 0.5f;
	[Export] public Vector3 ObjectScale = Vector3.One;
	
	private Node3D _grid;
	private int _gridSize = 10;
	private float _cellSize = 1.0f;
	private List<Vector2I> _usedPositions = new List<Vector2I>();
	private Random _random = new Random();
	private Dictionary<string, int> _prefabCounts = new Dictionary<string, int>();
	
	public override void _Ready()
	{
		InitializeGrid();
		SpawnObjects();
	}
	
	private void InitializeGrid()
	{
		if (GridPath != null && !GridPath.IsEmpty)
		{
			_grid = GetNode<Node3D>(GridPath);
			
			if (_grid.Get("GridSize").AsInt32() != 0)
				_gridSize = _grid.Get("GridSize").AsInt32();
			
			if (_grid.Get("CellSize").AsSingle() != 0)
				_cellSize = _grid.Get("CellSize").AsSingle();
		}
	}
	
	public void SpawnObjects()
	{
		_usedPositions.Clear();
		_prefabCounts.Clear();
		
		if (ObjectPrefabs == null || ObjectPrefabs.Length == 0)
			return;
		
		for (int i = 0; i < SpawnCount; i++)
		{
			Vector2I gridPos = GetRandomGridPosition();
			PackedScene prefab = ObjectPrefabs[_random.Next(ObjectPrefabs.Length)];
			SpawnObject(prefab, gridPos);
		}
	}
	
	private void SpawnObject(PackedScene prefab, Vector2I gridPos)
	{
		if (prefab == null) return;
		
		var instance = prefab.Instantiate<Node3D>();
		
		// Get the prefab base name
		string baseName = System.IO.Path.GetFileNameWithoutExtension(prefab.ResourcePath);
		
		// Increment counter for this prefab type
		if (!_prefabCounts.ContainsKey(baseName))
			_prefabCounts[baseName] = 0;
		
		_prefabCounts[baseName]++;
		
		// Set the name with incremental number
		instance.Name = $"{baseName}_{_prefabCounts[baseName]}";
		
		instance.Position = new Vector3(gridPos.X * _cellSize, SpawnHeight, gridPos.Y * _cellSize);
		instance.Scale = ObjectScale;
		
		AddChild(instance);
		
		// Register with WorldState
		var worldState = GetNode<WorldState>("/root/WorldState");
		worldState.RegisterObject(instance);
	}
	
	private Vector2I GetRandomGridPosition()
	{
		Vector2I pos;
		int attempts = 0;
		
		do
		{
			pos = new Vector2I(_random.Next(0, _gridSize), _random.Next(0, _gridSize));
			attempts++;
			
			if (attempts > 1000)
				break;
		} while (_usedPositions.Contains(pos));
		
		_usedPositions.Add(pos);
		return pos;
	}
}