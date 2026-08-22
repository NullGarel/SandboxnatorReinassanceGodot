using Godot;
using NullGarel.Util.StateMachine;
namespace NullGarel.Sandboxnator.Entity;

public class StateFly : IState<PlayerMovementContext>
{
    public void Enter(PlayerMovementContext ctx)
    {
        ctx.CurrentSpeed = ctx.SprintSpeed;
    }

    public IState<PlayerMovementContext> CheckTransitions(PlayerMovementContext ctx)
    {
        if (ctx.FlyMode)
            return null;
        else
            return PlayerMovementTransitions.ResolveGroundedTransition(ctx);
        
    }

    public void PhysicsProcess(PlayerMovementContext ctx, double delta)
    {
        ctx.ProcessFlightMovement(delta);
    }
}