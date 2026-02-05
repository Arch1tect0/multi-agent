using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ObjectSpawner : Node3D
{
	private PackedScene[] _objectPrefabs = Array.Empty<PackedScene>();
	private int _spawnCount = 10;
	private NodePath _gridPath;
	private float _spawnHeight = 0.5f;
	private Vector3 _objectScale = Vector3.One;
	private bool _respawn = false;
	
	[Export]
	public PackedScene[] ObjectPrefabs
	{
		get => _objectPrefabs;
		set
		{
			_objectPrefabs = value;
			if (Engine.IsEditorHint() && IsInsideTree() && _respawn)
				SpawnObjects();
		}
	}
	
	[Export]
	public int SpawnCount
	{
		get => _spawnCount;
		set
		{
			_spawnCount = value;
			if (Engine.IsEditorHint() && IsInsideTree() && _respawn)
				SpawnObjects();
		}
	}
	
	[Export]
	public NodePath GridPath
	{
		get => _gridPath;
		set
		{
			_gridPath = value;
			if (Engine.IsEditorHint() && IsInsideTree() && _respawn)
				SpawnObjects();
		}
	}
	
	[Export]
	public float SpawnHeight
	{
		get => _spawnHeight;
		set
		{
			_spawnHeight = value;
			if (Engine.IsEditorHint() && IsInsideTree() && _respawn)
				SpawnObjects();
		}
	}
	
	[Export]
	public Vector3 ObjectScale
	{
		get => _objectScale;
		set
		{
			_objectScale = value;
			if (Engine.IsEditorHint() && IsInsideTree() && _respawn)
				SpawnObjects();
		}
	}
	
	[Export]
	public bool Respawn
	{
		get => false;
		set
		{
			if (value && Engine.IsEditorHint())
			{
				_respawn = true;
				SpawnObjects();
			}
		}
	}
	
	private Node3D _grid;
	private int _gridSize = 10;
	private float _cellSize = 1.0f;
	private List<Vector2I> _usedPositions = new List<Vector2I>();
	private Random _random = new Random();
	
	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{
			InitializeGrid();
			SpawnObjects();
		}
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
		if (Engine.IsEditorHint())
			InitializeGrid();
			
		ClearSpawnedObjects();
		_usedPositions.Clear();
		
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
		instance.Position = new Vector3(gridPos.X * _cellSize, SpawnHeight, gridPos.Y * _cellSize);
		instance.Scale = ObjectScale;
		
		AddChild(instance);
		if (Engine.IsEditorHint())
			instance.Owner = GetTree().EditedSceneRoot;
		else
		{
			// Register with WorldState
			var worldState = GetNode<WorldState>("/root/WorldState");
			worldState.RegisterObject(instance);
		}
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
	
	private void ClearSpawnedObjects()
	{
		foreach (Node child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}
	}
}
