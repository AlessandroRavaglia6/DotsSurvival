using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private InputSystem_Actions _inputActions;

    protected override void OnCreate()
    {
        RequireForUpdate<InputActionMove>();
        RequireForUpdate<InputActionJump>();
        RequireForUpdate<InputActionAim>();
        RequireForUpdate<InputActionShoot>();
        RequireForUpdate<InputActionThrow>();

        _inputActions = new InputSystem_Actions();
        _inputActions.Player.Enable();
    }

    protected override void OnDestroy()
    {
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        // Move
        float2 moveInput = new float2(_inputActions.Player.Move.ReadValue<Vector2>());

        // Jump
        bool jumpIsPressed = _inputActions.Player.Jump.IsPressed();
        bool jumpWasPressedThisFrame = _inputActions.Player.Jump.WasPressedThisFrame();
        bool jumpWasReleasedThisFrame = _inputActions.Player.Jump.WasReleasedThisFrame();

        // Aim
        float2 aimInput = new float2(_inputActions.Player.Aim.ReadValue<Vector2>());

        // Shoot
        bool shootIsPressed = _inputActions.Player.Attack.IsPressed();
        bool shootWasPressedThisFrame = _inputActions.Player.Attack.WasPressedThisFrame();
        bool shootWasReleasedThisFrame = _inputActions.Player.Attack.WasReleasedThisFrame();

        // Throw
        bool throwIsPressed = _inputActions.Player.Throw.IsPressed();
        bool throwWasPressedThisFrame = _inputActions.Player.Throw.WasPressedThisFrame();
        bool throwWasReleasedThisFrame = _inputActions.Player.Throw.WasReleasedThisFrame();

        SystemAPI.SetSingleton(new InputActionMove { Direction = moveInput });
        SystemAPI.SetSingleton(new InputActionJump
        {
            IsPressed = jumpIsPressed,
            WasPressedThisFrame = jumpWasPressedThisFrame,
            WasReleasedThisFrame = jumpWasReleasedThisFrame
        });
        SystemAPI.SetSingleton(new InputActionAim { ScreenCoord = aimInput });
        SystemAPI.SetSingleton(new InputActionShoot
        {
            IsPressed = shootIsPressed,
            WasPressedThisFrame = shootWasPressedThisFrame,
            WasReleasedThisFrame = shootWasReleasedThisFrame
        });
        SystemAPI.SetSingleton(new InputActionThrow
        {
            IsPressed = throwIsPressed,
            WasPressedThisFrame = throwWasPressedThisFrame,
            WasReleasedThisFrame = throwWasReleasedThisFrame
        });
    }
}

public struct InputActionSingletonTag : IComponentData
{ }

public struct InputActionMove : IComponentData
{
    public float2 Direction;
}

public struct InputActionJump : IComponentData
{
    public bool IsPressed;
    public bool WasPressedThisFrame;
    public bool WasReleasedThisFrame;
}

public struct InputActionAim : IComponentData
{
    public float2 ScreenCoord;
}

public struct InputActionShoot : IComponentData
{
    public bool IsPressed;
    public bool WasPressedThisFrame;
    public bool WasReleasedThisFrame;
}

public struct InputActionThrow : IComponentData
{
    public bool IsPressed;
    public bool WasPressedThisFrame;
    public bool WasReleasedThisFrame;
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct InputDataSingletonInitialization : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<InputActionSingletonTag>())
        {
            var entity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponent<InputActionSingletonTag>(entity);
            state.EntityManager.AddComponent<InputActionMove>(entity);
            state.EntityManager.AddComponent<InputActionJump>(entity);
            state.EntityManager.AddComponent<InputActionAim>(entity);
            state.EntityManager.AddComponent<InputActionShoot>(entity);
            state.EntityManager.AddComponent<InputActionThrow>(entity);
        }
    }
}