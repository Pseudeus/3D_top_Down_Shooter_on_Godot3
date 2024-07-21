using Godot;
using System;

public class GunController : Node
{
    [Export] private PackedScene startingWeapon;
    private Spatial m_equipedWeapon;

    public override void _Ready()
    {
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    private void EquipWeapon(PackedScene weaponToEquip)
    {
        if (m_equipedWeapon != null)
        {
            m_equipedWeapon.QueueFree();
        }
        else
        {
            m_equipedWeapon = (Spatial)weaponToEquip.Instance();
            AddChild(m_equipedWeapon);
        }
    }

    public void ValidateShoot()
    {
        if (m_equipedWeapon != null) 
        {
            var weaponControl = m_equipedWeapon as Gun;
            weaponControl.Shoot();
        }
    }
} 
