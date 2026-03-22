using Godot;
using System;

public partial class Stick : Node3D
{
	[Export] CharacterBody3D defaultMoveTarget;
	[Export] Vector3 moveTarget;
	[Export] Node3D lookTarget;
	[Export] Camera3D cam;
	
	[Export] public float maxDistance = 4f;
	[Export] public int sideRayCount = 8;
	[Export] public float raySpread = 0.5f;
	[Export] public float sideStrength = 0.3f;
	public bool allowInput = true;
	
	float camSpeed = .1f;
	float pitch = 0f;
	
	float yaw = 0f;
	Vector3 offset = new Vector3(0, 1.25f, 0);
	
	float inputTimer = 0f;
	[Export] float recenterDelay = 3f;
	[Export] float recenterSpeed = 2f;

	bool isRecentering = false;

	public override void _Ready()
	{
		moveTarget = defaultMoveTarget.Position;
	}

	public override void _Process(double delta)
	{
		if (allowInput)
		{
			moveTarget = defaultMoveTarget.Position;
			Vector2 input = Input.GetVector("cam_left", "cam_right", "cam_up", "cam_down");
			
			if (input.Length() > 0.01f)
			{
				inputTimer = 0f;
				isRecentering = true;
			}
			else
			{
				inputTimer += (float)delta;
			}
			
			pitch += input.Y;
			yaw += input.X;
			
			pitch = Mathf.Clamp(pitch, -Mathf.Pi / .25f, Mathf.Pi / .25f);
			
			Vector3 targetRotation;

			if (isRecentering)
			{
				Vector3 forward = defaultMoveTarget.GlobalTransform.Basis.Z;

				float targetYaw = Mathf.Atan2(forward.X, forward.Z);

				yaw = Mathf.LerpAngle(yaw, targetYaw, (float)delta * recenterSpeed);

				pitch = Mathf.Lerp(pitch, 0f, (float)delta * recenterSpeed);
			}

			targetRotation = new Vector3(pitch * camSpeed, yaw * camSpeed, 0);
			Rotation = targetRotation;
			
			GlobalPosition = moveTarget + offset;
			
			UpdateCamera(delta);
		}
		else 
		{
			UpdateCamera(delta);
			cam.Position = Vector3.Zero;
			Position = moveTarget;
			LookAt(lookTarget.Position, Vector3.Up);
		}
	}
	
	void UpdateCamera(double delta)
	{
		var space = GetWorld3D().DirectSpaceState;
		
		Vector3 origin = GlobalTransform.Origin;
		Vector3 backward = GlobalTransform.Basis.Z;
		Vector3 right = GlobalTransform.Basis.X;
		
		int counter = 0;
		
		float targetDistance = maxDistance;
		float targetX = 0f;
		
		var centerHit = CastRay(space, origin, backward, maxDistance);
		if (centerHit.hit)
			targetDistance = centerHit.distance;
		
		for (int i = 0; i < sideRayCount; i++)
		{
			float lerpT = (float)(i + 1 )/ sideRayCount;
			float spread = raySpread * lerpT;
			
			Vector3 leftDir = (backward - right * spread).Normalized();
			// DebugDraw3D.DrawLine(origin, origin + leftDir * maxDistance, new Color(1, 0, 0));
			if (CastRay(space, origin, leftDir, maxDistance).hit)
				counter++;

			Vector3 rightDir = (backward + right * spread).Normalized();
			// DebugDraw3D.DrawLine(origin, origin + leftDir * maxDistance, new Color(1, 0, 0));
			if (CastRay(space, origin, rightDir, maxDistance).hit)
				counter--;
		}
		
		targetX = counter * sideStrength;
		
		Vector3 targetLocalPos = new Vector3(targetX, 0, targetDistance);
		
		float t = 1f - Mathf.Exp(-10f * (float)delta);
		cam.Position = cam.Position.Lerp(targetLocalPos, t);
		
		cam.LookAt(lookTarget.GlobalPosition, Vector3.Up);
	}
	
	(bool hit, float distance) CastRay(PhysicsDirectSpaceState3D space, Vector3 origin, Vector3 dir, float length)
	{
		var query = PhysicsRayQueryParameters3D.Create(origin, origin + dir * length);
		query.Exclude = new Godot.Collections.Array<Rid> { defaultMoveTarget.GetRid() };
		var result = space.IntersectRay(query);
		
		if (result.Count > 0 )
		{
			float dist = origin.DistanceTo((Vector3)result["position"]);
			return (true, dist);
		}
		return (false, length);	
	}
	
	public void ResetCamPos()
	{
		moveTarget = defaultMoveTarget.Position;
		allowInput = true;
	}
	
	public void Hint(Vector3 position)
	{
		moveTarget = position;
		allowInput = false;
	}
}
