using Godot;
using System;

public class Stats : Node
{
    [Signal] public delegate void CriticalDamageTaken();

    [Export] private byte healthPoints = 8;
    [Export] private byte damagePoints = 1;    

    private byte m_SavedHealthPoints;

    public override void _Ready()
    {
        m_SavedHealthPoints = healthPoints;
    }

    public void WhenReciveDamage(byte dmg)
    {
        if (healthPoints - dmg > 0)
        {
            healthPoints -= dmg;
        }
        else
        {
            healthPoints = m_SavedHealthPoints;
            EmitSignal(nameof(CriticalDamageTaken));
        }
    }
} 
