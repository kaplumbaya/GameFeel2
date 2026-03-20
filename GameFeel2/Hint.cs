using Godot;
using System;

public partial class Hint : Area3D
{
	[Export] private Node3D cam;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body.IsInGroup("Player"))
		{
			cam.Call("Hint", Position);
		}
	}
}
