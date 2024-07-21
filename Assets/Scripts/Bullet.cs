using Godot;
using System;

public class Bullet : Area
{
    [Export] private float speed = 70f;
    [Export] private byte bulletDamage = 1;

    private Vector3 m_ForwardDirection;

    public Gun bulletSpawner;
    public bool isMoving;

    public override void _Ready()
    {
        GetNode<VisibilityNotifier>("VisibilityNotifier").Connect("screen_exited", this, nameof(OnVisibilityScreenExited));
        Connect("body_entered", this, nameof(OnBulletCollides));

        SetDirection();
        isMoving = true;
    }
    public void SetDirection()
    {
        m_ForwardDirection = GlobalTransform.basis.z.Normalized();
    }
    private void OnVisibilityScreenExited()
    {
        WhenBulletImpacted();
    }
    private void WhenBulletImpacted()
    {
        isMoving = false;
        Visible = false;
        GetNode<CollisionShape>("CollisionShape").Disabled = true;
        bulletSpawner.StoreBullet(this as Spatial);
    }
    private void OnBulletCollides(Node body)
    {
        if (body is KinematicBody)
        {
            body.GetNode<Stats>("Stats").WhenReciveDamage(bulletDamage);
            WhenBulletImpacted();
        }
        else if (m_ForwardDirection.z > m_ForwardDirection.x)
        {
            m_ForwardDirection *= new Vector3(1, 0, -1);
        }
        else 
        {
            m_ForwardDirection *= new Vector3(-1, 0, 1);
        }
    }

    public override void _PhysicsProcess(float delta)
    { 
        if (isMoving)
        {
            GlobalTranslate(-m_ForwardDirection * speed * delta);
        }
    }
} 
