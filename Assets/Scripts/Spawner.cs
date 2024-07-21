using Godot;
using System;
using System.Collections.Generic;

public class Spawner : Spatial
{
    [Export] private PackedScene enemyReference;

    private Wave[] m_Waves;
    private byte m_CurrentWave = 0;
    private Queue<KinematicBody> m_EnemySpawnLoop;
    private Timer m_Timer;
    private byte m_NumOfEnemiesRemainingToSpawn;
    private byte m_NumOfEnemiesKiledThisWave;

    public override void _Ready()
    {
        var wavesParent = GetNode("WavesBehaviour");
        m_Waves = new Wave[wavesParent.GetChildCount()];

        for (byte i = 0; i < wavesParent.GetChildCount(); i++) { m_Waves[i] = wavesParent.GetChild(i) as Wave; }

        m_EnemySpawnLoop = new Queue<KinematicBody>();

        m_Timer = GetNode<Timer>("Timer");
        m_Timer.Connect("timeout", this, nameof(OnTimerTimeout));
        m_Timer.Start();

        StartNextWave(false);
    }

    private void StartNextWave(bool turnUp = true)
    {
        m_CurrentWave = turnUp && m_CurrentWave < m_Waves.Length - 1
                      ? ++m_CurrentWave
                      : m_CurrentWave;

        var wave = m_Waves[m_CurrentWave];
        
        m_NumOfEnemiesKiledThisWave = 0;
        m_NumOfEnemiesRemainingToSpawn = wave.numEnemies;
        m_Timer.WaitTime = wave.secondsBetweenSpawn;
    }

    private void OnTimerTimeout()
    {
        if (m_NumOfEnemiesRemainingToSpawn > 0)
        {
            if (m_EnemySpawnLoop.Count > 0)
            {
                SpawnFromQueue();
            }
            else
            {
                SpawnFromInstance();
            }
        }
        else 
        {
            if (m_NumOfEnemiesKiledThisWave == m_Waves[m_CurrentWave].numEnemies)
            {
                StartNextWave();
            }
        }
    }
    private void SpawnFromQueue()
    {
        var enemy = m_EnemySpawnLoop.Dequeue();

        enemy.GetNode<CollisionShape>("CollisionShape").Disabled = false;
        enemy.Visible = true;
        (enemy as Enemy).isDeath = false;
        m_NumOfEnemiesRemainingToSpawn--;
    }
    private void SpawnFromInstance()
    {
        var enemy = enemyReference.Instance() as KinematicBody;

        (enemy as Enemy).spawner = this;
        GetNode<Spatial>("Enemies").AddChild(enemy);
        m_NumOfEnemiesRemainingToSpawn--;
    }
    public void WhenEnemyDies(KinematicBody enemy)
    {
        if (!m_EnemySpawnLoop.Contains(enemy))
        {
            m_NumOfEnemiesKiledThisWave++;
            enemy.Translation = GlobalTransform.origin;
            m_EnemySpawnLoop.Enqueue(enemy);
        }
    }
} 
