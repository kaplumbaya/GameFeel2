using Godot;
using System;

public partial class ResetCam : Area3D
{
	[Export] Node3D camera;
	
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body.IsInGroup("Player"))
		{
			camera.Call("ResetCamPos");
		}
	}
}
