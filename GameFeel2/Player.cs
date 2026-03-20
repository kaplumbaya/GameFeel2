using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// Movement
	[Export] public float MoveSpeed = 10f;
	[Export] public float Acceleration = 20f;
	[Export] public float Deceleration = 40f;
	private int movementMode = 1;

	// Jump
	[Export] public float JumpVelocity = 8f;
	[Export] public float Gravity = 25f;
	[Export] public float AirGravity = 12f;
	[Export] public float JumpDelay = 0.15f;
	private bool preparingJump = false;
	private float jumpTimer = 0f;


	// Coyote Time
	[Export] public float CoyoteTime = 0.2f;
	private float coyoteTimer = 0f;

	// Duck
	[Export] public float DuckSpeed = 3f;
	private bool isDucking = false;
	[Export] public float StandingHeight = 1.6f;
	[Export] public float DuckingHeight = 0.8f;
	private CollisionShape3D collision;
	private CapsuleShape3D capsule;
	private float originalPlayerY;

	// Nodes
	private Node3D gfx;
	
	[Export] private Node3D camera;
	
	public override void _Ready()
	{
		gfx = GetNode<Node3D>("GFX");

		collision = GetNode<CollisionShape3D>("CollisionShape3D");
		capsule = (CapsuleShape3D)collision.Shape;
		originalPlayerY = GlobalPosition.Y;
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;

		Vector3 velocity = Velocity;

		HandleCoyoteTime(d);
		HandleGravity(ref velocity, d);
		Vector3 direction = GetCameraRelativeInput();
		HandleMovement(ref velocity, direction, d);
		HandleJumpInput();
		HandleJump(ref velocity, d);
		HandleDuck(d);
		Velocity = velocity;
		MoveAndSlide();
		RotateGraphics(d);
		
		if (Input.IsActionJustPressed("respawn"))
		{
			Position = new Vector3(0f, 1.5f, 31f);
		}
	}

	// Input Direction Relative Camera
	private Vector3 GetCameraRelativeInput()
	{
		Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");

		Vector3 forward = camera.GlobalTransform.Basis.Z;
		Vector3 right = camera.GlobalTransform.Basis.X;

		Vector3 direction = (forward * input.Y + right * input.X);

		direction.Y = 0;

		if (direction.Length() > 0)
			direction = direction.Normalized();

		return direction;
	}

	// Movement Logic
	private void HandleMovement(ref Vector3 velocity, Vector3 direction, float delta)
{
	float speed = isDucking ? DuckSpeed : MoveSpeed;
	Vector3 targetVelocity = direction * speed;
	Vector3 horizontalVelocity = new Vector3(velocity.X, 0, velocity.Z);
	
	horizontalVelocity = targetVelocity;

	velocity.X = horizontalVelocity.X;
	velocity.Z = horizontalVelocity.Z;
}
	// Gravity + Hang Time
	private void HandleGravity(ref Vector3 velocity, float delta)
	{
		if (!IsOnFloor())
		{
			if (velocity.Y > 0)
				velocity.Y -= AirGravity * delta;
			else
				velocity.Y -= Gravity * delta;
		}
	}

	// Coyote Time
	private void HandleCoyoteTime(float delta)
	{
		if (IsOnFloor())
			coyoteTimer = CoyoteTime;
		else
			coyoteTimer -= delta;
	}

	// Jump Input
	private void HandleJumpInput()
	{
		if (Input.IsActionJustPressed("jump") && coyoteTimer > 0)
		{
			preparingJump = true;
			jumpTimer = JumpDelay;
		}
	}

	// Jump Execution
	private void HandleJump(ref Vector3 velocity, float delta)
	{
		if (!preparingJump)
			return;

		jumpTimer -= delta;

		// Squash animation
		gfx.Scale = new Vector3(1, 0.7f, 1);

		if (jumpTimer <= 0)
		{
			velocity.Y = JumpVelocity;
			gfx.Scale = Vector3.One;
			preparingJump = false;
			coyoteTimer = 0;
		}
	}

	// Duck
	private void HandleDuck(float delta)
	{
		float targetHeight;
		float targetPlayerY;

		if (Input.IsActionPressed("duck"))
		{
			isDucking = true;
			targetHeight = DuckingHeight;
		}
		else
		{
			isDucking = false;
			targetHeight = StandingHeight;
		}

		float oldHeight = capsule.Height;
		capsule.Height = Mathf.Lerp(capsule.Height, targetHeight, 10f * delta);
		float heightDifference = capsule.Height - oldHeight;
		GlobalPosition += new Vector3(0, heightDifference / 2f, 0);

		float scaleY = capsule.Height / StandingHeight;
		gfx.Scale = new Vector3(1, scaleY, 1);
		gfx.Position = new Vector3(0, capsule.Height / 2f, 0);
	}
	
	private void RotateGraphics(float delta)
	{
		Vector3 horizontalVelocity = Velocity;
		horizontalVelocity.Y = 0;
		if (horizontalVelocity.Length() < 0.1f)
		   
			return;
		float angle = Mathf.Atan2(horizontalVelocity.X, horizontalVelocity.Z);
		Vector3 rot = gfx.Rotation;
		rot.Y = Mathf.LerpAngle(rot.Y, angle, 10f * delta);
		gfx.Rotation = rot;
	}
}
