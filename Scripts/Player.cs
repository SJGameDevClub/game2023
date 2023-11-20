using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D {
    
    public const float speed = 300.0f;
    
    public const float jumpVelocity = -500.0f;
    
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    
    public AnimatedSprite2D sprite;
    
    public Vector2 spawnpoint;
    [Export]
    public int DamageCDMS = 1000;

    private bool inRespawn = false;

    private TileMap tileMap;

    private Node dcdHolder;

    public override void _Ready() {
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.sprite.Play("idle");
        this.tileMap = this.GetParent().GetNode<TileMap>("TileMap");
        this.dcdHolder = this.GetNode("Damage CD Holder");
        this.AddChild(new Camera2D());
        spawnpoint = GlobalPosition;
    }

    bool draw = false;
    public static double baseJumpTimer {get;} = .25d;
    private double jumpTimer = baseJumpTimer;
    private int jumps = 2;
    public override void _PhysicsProcess(double delta) {
        if (inRespawn) {
            return;
        }
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor() && (!Input.IsActionPressed("jump") || jumpTimer <= 0)) {
            velocity.Y = Mathf.Min(velocity.Y + this.gravity * (float) delta, 2000);
        }

        if (this.IsOnFloor()) {
            this.jumps = 2;
        }

        if (Input.IsActionPressed("jump")) {
            jumpTimer = !IsOnFloor() ? jumpTimer - delta : baseJumpTimer;
        } else {
            jumpTimer = !IsOnFloor() ? 0 : baseJumpTimer;
        }
        
        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && (jumpTimer >= 0 || this.jumps > 0)) {
            this.sprite.Play("jump"); 
            velocity.Y = jumpVelocity;
            jumps--;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        if (direction.X != 0) {
            velocity.X = direction.X * speed * (this.IsOnFloor() ? 1 : .75f);
            this.sprite.FlipH = direction.X < 0;
            if (velocity.Y == 0) {
                this.sprite.Play("walk");
            }
        } else {
            velocity.X = Mathf.MoveToward(this.Velocity.X, 0, speed);
            if (velocity.Y == 0) {
                this.sprite.Play("idle");
            }
        }

        if (velocity.Y > 0 && this.sprite.Animation != "jump") {
            this.sprite.Play("jump");
            this.sprite.Frame = 2;
        }

        this.Velocity = velocity;
        this.MoveAndSlide();

        if (PlayerInfo.Health <= 0) {
            GD.Print("DAETH");
            this.RespawnAnim();
        }

    }

    public void Respawn() {
        foreach (var kvp in damageCooldowns) {
            kvp.Value.QueueFree();
        }
        damageCooldowns.Clear();
        this.GlobalPosition = spawnpoint;
        PlayerInfo.Health = 100f;
        this.PlatformFloorLayers = _layer;
        sprite.FlipH = false;
        inRespawn = false;
    }

    public override void _Draw() {
        base._Draw();
        if (draw) {
            DrawCircle((GetParent() as Node2D).Position, 50f, Color.Color8(255, 0, 0));
            DrawCircle(Position, 50f, Color.Color8(0, 0, 255));
            DrawCircle(tileMap.MapToLocal(new(0, 0)), 50f, Color.Color8(0, 255, 0));
        }
        draw = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node">The node with a float or double damage meta</param>
    /// <returns>true if alive</returns>
    public bool HandleDamage(Node node) {
        float damage = (float) node.GetMeta("damage", 10);
        //TODO: Extra damage handling here later
        if (( PlayerInfo.Health -= damage ) <= 0) {
            RespawnAnim();
            return false;
        }
        GD.Print("Damaged: ", PlayerInfo.Health, " HP");
        return true;
    }

    private uint _layer;
    private Dictionary<ulong, Timer> damageCooldowns = new();
    public void AreaEntered(Area2D area) {
        if (area.HasMeta("damage")) {
            if (!HandleDamage(area)) {
                return;
            }
            var timer = new Timer();
            damageCooldowns.Add(area.GetInstanceId(), timer);
            timer.Autostart = true;
            timer.WaitTime = this.DamageCDMS / 1000f;
            timer.Timeout += () => HandleDamage(area);
            this.dcdHolder.AddChild(timer, false, InternalMode.Back);
            return;
            // this.damageCooldown.Timeout += this._dc = () => HandleDamage(area);
        }
        if (!area.HasMeta("InstantDeath")) {
            return;
        }
        RespawnAnim();
    }

    public void AreaLeft(Area2D area) {
        if (area.HasMeta("damage")) {
            if (!damageCooldowns.Remove(area.GetInstanceId(), out Timer timer)) {
                return;
            }
            timer.QueueFree();
        }
    }

    public void RespawnAnim() {
        GD.Print("Start Respawning");
        inRespawn = true;
        var tween = CreateTween();
        _layer = this.PlatformFloorLayers;
        this.PlatformFloorLayers = 0;
        sprite.Play("hurt");
        tween.TweenProperty(this, "position", new Vector2(this.Position.X, this.Position.Y - 50), 1);
        tween.TweenCallback(new(this, nameof(Respawn)));
    }

}
