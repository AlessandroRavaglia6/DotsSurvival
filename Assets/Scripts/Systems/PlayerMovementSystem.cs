using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public partial struct PlayerMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputActionMove>();
        state.RequireForUpdate<InputActionJump>();
    }

    public void OnUpdate(ref SystemState state)
    {
        float2 moveInput = SystemAPI.GetSingleton<InputActionMove>().Direction;
        bool jumpWasPresedThisFrame = SystemAPI.GetSingleton<InputActionJump>().WasPressedThisFrame;

        foreach (var (movementDirection, physicsVelocity, playerJumpForce) in SystemAPI.Query<RefRW<CharacterMovementDirection>, RefRW<PhysicsVelocity>, RefRO<PlayerJumpForce>>().WithAll<PlayerTag>())
        {
            movementDirection.ValueRW.Direction = new float3(moveInput.x, 0.0f, moveInput.y);

            if (jumpWasPresedThisFrame)
            {
                physicsVelocity.ValueRW.Linear.y = playerJumpForce.ValueRO.Value;
            }
        }
    }
}
