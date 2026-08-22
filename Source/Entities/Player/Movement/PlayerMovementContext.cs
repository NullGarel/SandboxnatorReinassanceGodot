using Godot;
namespace NullGarel.Sandboxnator.Entity;

public class PlayerMovementContext
{
    public CharacterBody3D CharacterBody { get; set; }
    public PlayerInput Input { get; set; }

    public Vector3 Velocity => _velocity;
    private Vector3 _velocity;

    public float CurrentSpeed { get; set; }
    public float WalkSpeed { get; set; }
    public float SprintSpeed { get; set; }
    public float JumpVelocity { get; set; }

    public bool FlyMode { get; set; }

    public Vector3 Facing { get; set; }
    public Vector3 Forward => CharacterBody.GlobalTransform.Basis.Z;
    public Vector3 Right => CharacterBody.GlobalTransform.Basis.X;

    /// <summary>
    /// call externally to copy/"sync" it from CharacterBody3D
    /// </summary>
    /// <param name="velocity"></param>
    public void SetVelocity(Vector3 velocity) => _velocity = velocity;

    public void ProcessHorizontalMovement(double delta)
    {
        Vector2 inputDir = Input.MovementVector;
        Vector3 direction = (Forward * inputDir.Y + Right * inputDir.X).Normalized();

        _velocity.X = direction.X * CurrentSpeed;
        _velocity.Z = direction.Z * CurrentSpeed;
    }

    public void ProcessFlightMovement(double delta)
    {
        Vector2 inputDir = Input.MovementVector;
        Vector3 direction = (Facing * inputDir.Y + Right * inputDir.X).Normalized();

        _velocity = direction * CurrentSpeed;
    }

    public void HaltHorizontalMovement()
    {
        _velocity.X = Mathf.MoveToward(_velocity.X, 0, CurrentSpeed);
        _velocity.Z = Mathf.MoveToward(_velocity.Z, 0, CurrentSpeed);
    }

    public void ProcessGravity(double delta)
    {
        _velocity += CharacterBody.GetGravity() * (float)delta;
    }

    public void ApplyJumpImpulse()
    {
        _velocity.Y = JumpVelocity;
    }


}