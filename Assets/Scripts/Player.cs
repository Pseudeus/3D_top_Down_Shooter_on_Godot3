using Godot;
using System;

public class Player : KinematicBody
{
    [Export] private float speed = 10f;
    [Export] private float acceleration = 100f;
    [Export] private float friction = 100f;

    private GunController m_gunController;
    private Vector3 m_Velocity;
    private Vector2 m_InputCoordinates;

    public override void _Ready()
    {
        m_gunController = GetNode<GunController>("BodyShape/Hand");
        GetNode<Stats>("Stats").Connect("CriticalDamageTaken", this, nameof(OnCriticalDamageTaken));
    }

    private void MoveState(float delta)
    {
        m_InputCoordinates = new Vector2(Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"), Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"));

        if (m_InputCoordinates != Vector2.Zero)
        {
            m_Velocity = m_Velocity.MoveToward(new Vector3(m_InputCoordinates.x, 0f, m_InputCoordinates.y).Normalized() * speed, acceleration * delta);
        }
        else
        {
            m_Velocity = m_Velocity.MoveToward(Vector3.Zero, friction * delta);
        }
    }

    public override void _Process(float delta)
    {
        MoveState(delta);
        
        if (Input.IsActionPressed("primary_action")) m_gunController.ValidateShoot();
    }
    private void OnCriticalDamageTaken()
    {
        QueueFree();
    }
    public override void _PhysicsProcess(float delta)
    {
        if (m_Velocity != Vector3.Zero)
        {
            m_Velocity = MoveAndSlide(m_Velocity);
        }
    }
} 
