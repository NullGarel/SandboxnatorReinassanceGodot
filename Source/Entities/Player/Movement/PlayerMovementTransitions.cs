using Godot;
using NullGarel.Util.StateMachine;
namespace NullGarel.Sandboxnator.Entity;

public static class PlayerMovementTransitions
{
    public static IState<PlayerMovementContext> ResolveGroundedTransition(PlayerMovementContext ctx)
    {

        if (ctx.FlyMode)
            return new StateFly();

        if (!ctx.CharacterBody.IsOnFloor())
            return new StateFall();

        if (ctx.Input.IsJumping)
            return new StateJump();

        if (ctx.Input.MovementVector == Vector2.Zero)
            return new StateIdle();

        return ctx.Input.IsSprinting ? new StateSprint() : new StateWalk();
    }
}