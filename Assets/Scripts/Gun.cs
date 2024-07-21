using Godot;
using System;
using System.Collections.Generic;

public class Gun : Spatial
{
    [Export] private PackedScene bullet;
    [Export] private float muzzleSpeed = 30f;
    [Export] private ushort millisBetweenShots = 100;

    private Queue<Spatial> _bulletsRingQueue;
    private Timer _shotTimer;
    private bool _canShoot = true;
    private Position3D _muzzle;
    private Stats _playerStats;
    private byte _bulletsSpawned;

    public override void _Ready()
    {
        _bulletsRingQueue = new Queue<Spatial>();
        _playerStats = GetNode<Stats>("../../../Stats");
        _muzzle = GetNode<Position3D>("Muzzle");
        _shotTimer = GetNode<Timer>("Timer");

        _shotTimer.WaitTime = millisBetweenShots / 1_000f;

        _shotTimer.Connect("timeout", this, nameof(OnTimerTimeout));
    }

    public void Shoot()
    {
        if(_canShoot)
        {
            if (_bulletsRingQueue.Count > 0)
            {
                SpawnFromQueue();
            }
            else
            {
                SpawnFromInstance();
            }
        }
    }
    private void SpawnFromInstance()
    {
        Spatial bullet = this.bullet.Instance() as Spatial;

        (bullet as Bullet).bulletSpawner = this;
        bullet.Transform = _muzzle.GlobalTransform;  
        bullet.SetDeferred("speed", muzzleSpeed);
        bullet.SetDeferred("bulletDamage", _playerStats.Get("damagePoints"));

        GetTree().CurrentScene.AddChild(bullet);
        _canShoot = false;
        _shotTimer.Start();
        _bulletsSpawned++;
    }
    private void SpawnFromQueue()
    {
        var bullet = _bulletsRingQueue.Dequeue();
        var bulletClass = bullet as Bullet;

        bullet.Transform = _muzzle.GlobalTransform;

        bullet.Visible = true;
        bullet.GetNode<CollisionShape>("CollisionShape").Disabled = false;

        bulletClass.SetDirection();
        bulletClass.isMoving = true;

        _canShoot = false;
        _shotTimer.Start();
    }
    private void OnTimerTimeout()
    {
        _canShoot = true;
    }  
    public void StoreBullet(Spatial bullet)
    {
        if (!_bulletsRingQueue.Contains(bullet))
        {
            _bulletsRingQueue.Enqueue(bullet);
        }
    }
} 
