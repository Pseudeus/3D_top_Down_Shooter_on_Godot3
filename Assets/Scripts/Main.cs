using Godot;
using System;

public class Main : Spatial
{

    private Vector3 m_RayOrigin;
    private Vector3 m_RayTarget;
    private Camera m_MainCamera;
    private KinematicBody m_Player;
    private float m_sqrDeathZone = 0.5f;
    public override void _Ready()
    {
        m_MainCamera = GetNode<Camera>("Camera");
        m_Player = GetNode<KinematicBody>("Player");
    }

    public override void _PhysicsProcess(float delta)
    {
        var mousePosition = GetViewport().GetMousePosition();
        //GD.Print("Mouse position is: " + mousePosition);
        m_RayOrigin = m_MainCamera.ProjectRayOrigin(mousePosition);
        //GD.Print("ray origin: " + m_RayOrigin);
        m_RayTarget = m_RayOrigin + m_MainCamera.ProjectRayNormal(mousePosition) * 2000;
        //GD.Print("Ray Target: " + m_RayTarget);

        var spaceState = GetWorld().DirectSpaceState;
        var intersection = spaceState.IntersectRay(m_RayOrigin, m_RayTarget);

        if (intersection.Count > 0)
        {
            Vector3 pos = (Vector3)intersection["position"];
            Vector2 playerLookPosition = new Vector2(pos.x, pos.z);
            Vector2 player2Axis = new Vector2(m_Player.Transform.origin.x, m_Player.Transform.origin.z);
            Vector2 dir = playerLookPosition - player2Axis;
            //Quat a = Transform.basis.Quat();
            //var b = Transform.basis.Quat();

            if(dir.LengthSquared() > m_sqrDeathZone)
            {
                m_Player.LookAt(new Vector3(playerLookPosition.x, m_Player.Transform.origin.y, playerLookPosition.y), Vector3.Up);
            }
        }
    }
} 
