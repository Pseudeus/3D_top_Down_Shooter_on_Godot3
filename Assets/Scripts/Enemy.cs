using Godot;
using System;

public class Enemy : KinematicBody
{
    [Export] private float speed = 2f;
    [Export] private float chaseSpeed = 5f;

    public Spawner spawner;
    public bool isDeath;

    private Navigation _navigation;
    private KinematicBody _player;
    private Vector3[] _path;
    private byte _navPathNode = 0;
    private Vector3 _direction;
    private Timer _attackTimer;
    private Timer _patUpdateTimer;
    private float _longRangeUpdateFactor = 2.4f;
    private float _midRangeUpdateFactor = 1.2f;
    private float _shortRangeUpdateFactor = 0.4f;

    private State _currentState = State.Seeking;
    private Vector3 _preChasePosition;

    private enum State
    {
        Seeking,
        Attacking,
        Resting
    }
    
    public override void _Ready()
    {
        var parent = GetTree().CurrentScene;

        _navigation = parent.GetNode<Navigation>("Navigation");
        _player = parent.GetNode<KinematicBody>("Player");
        _patUpdateTimer = GetNode<Timer>("PathUpdateTimer");
        _attackTimer = GetNode<Timer>("AttackRadius/AttackTimer");

        _patUpdateTimer.Connect("timeout", this, nameof(OnPathUpdateTimerTimeout));
        _attackTimer.Connect("timeout", this, nameof(OnAttackTimerTimeout));

        GetNode<Stats>("Stats").Connect("CriticalDamageTaken", this, nameof(OnCriticalDamageTaken));
        GetNode<Area>("AttackRadius").Connect("body_entered", this, nameof(OnAttackRadiusBodyEntered));
        
        UpdatePath(_player.GlobalTransform.origin);
    }
    private Vector3 FollowPath()
    {
        //Get the direction to the next path position or checkpoint
        var direction = _path[_navPathNode] - GlobalTransform.origin;
        var distance2Player = _player.GlobalTransform.origin - GlobalTransform.origin;

        if (distance2Player.LengthSquared() < 1.25f) 
        {   
            _patUpdateTimer.Stop();
            _direction = Vector3.Zero;

            return _direction;
        }
        else if (_patUpdateTimer.IsStopped())
        {
            _patUpdateTimer.Start();
        }
        else
        {
            RefreshTimeFactor(distance2Player.LengthSquared());
        }

        //Update the path target if the enemy has reached the checkpoint
        if (direction.LengthSquared() < 0.1f && _navPathNode < _path.Length - 1)
        {
            _navPathNode++;
            _direction = _path[_navPathNode] - GlobalTransform.origin;
            _direction = _direction.Normalized();
        }
        //If for some reason there are some velocity in the y axis, turn it to zero
        if (_direction.y > float.Epsilon) { _direction.y = 0f; }

        return _direction;
    }
    private void DoAttack()
    {
        var chaseDistance = (GlobalTransform.origin - _preChasePosition).LengthSquared();
        
        if (chaseDistance < 10f)
        {
            MoveAndSlide(_direction * speed * chaseSpeed);
        }
        else
        {
            _currentState = State.Resting;
        }
    }
    private void UpdatePath(Vector3 target)
    {
        _path = _navigation.GetSimplePath(GlobalTransform.origin, target);
        _navPathNode = 0;
    }
    private void OnAttackRadiusBodyEntered(Node body)
    {
        if (body is KinematicBody && _currentState != State.Resting)
        {
            _preChasePosition = GlobalTransform.origin;
            _currentState = State.Attacking;
            _attackTimer.Start();
        }
    }
    private void OnAttackTimerTimeout()
    {
        _currentState = State.Seeking;
    }
    private void OnPathUpdateTimerTimeout()
    {
        UpdatePath(_player.GlobalTransform.origin);
    }
    private void RefreshTimeFactor(float dis2Player)
    {
        if (dis2Player >= 175f)
        {
            _patUpdateTimer.WaitTime = _longRangeUpdateFactor;
        }
        else if (dis2Player > 50 && dis2Player < 175f)
        {
            _patUpdateTimer.WaitTime = _midRangeUpdateFactor;
        }
        else 
        {
            _patUpdateTimer.WaitTime = _shortRangeUpdateFactor;
        }
    }
    private void OnCriticalDamageTaken()
    {
        isDeath = true;
        Visible = false;
        GetNode<CollisionShape>("CollisionShape").Disabled = true;
        spawner.WhenEnemyDies(this as KinematicBody);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (!isDeath)
        {
            switch (_currentState)
            {
                case State.Seeking:

                    MoveAndSlide(FollowPath() * speed);

                    break;
                case State.Attacking:
                
                    DoAttack();

                    break;
                case State.Resting:
                    GD.Print("Rest!");
                    break;
            }
        }   
    }
} 