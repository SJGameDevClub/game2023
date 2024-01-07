using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D {

    [Export]
    public float speed = 300.0f;

    [Export]
    public float jumpVelocity = -1000.0f;

    [Export]
    public float damage = 2f;

    [Export]
    public int DamageCDMS = 1000;

    [Export]
    private int totalJumps = 2;

    [Export]
    public float totalJumpTimer = .35f;
    [Export]
    public Vector2 spawnpoint;

    private int jumps = 2;
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private AnimatedSprite2D sprite;
    private bool inRespawn = false;
    private TileMap tileMap;
    private Node dcdHolder;
    private RayCast2D attackHitbox;

    public override void _Ready() {
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.sprite.Play("idle");
        this.tileMap = this.GetParent().GetNode<TileMap>("TileMap");
        this.dcdHolder = this.GetNode("Damage CD Holder");
        this.attackHitbox = this.GetNode<RayCast2D>("Weapon Hitbox");
        this.AddChild(new Camera2D());
        spawnpoint = GlobalPosition;
    }

    bool draw = false;
    private float jumpTimer = 0f;
    private int counter = -90;
    private List<ulong> attackCD = new();
    public override void _PhysicsProcess(double delta) {
        if (inRespawn) {
            return;
        }
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor() && (!Input.IsActionPressed("jump") || this.jumpTimer <= 0)) {
            velocity.Y = Mathf.Min(velocity.Y + this.gravity * (float) delta, 2000);
        }

        if (this.IsOnFloor()) {
            this.jumps = this.totalJumps;
        }

        this.handleAttack();
        this.handleMovement(ref velocity);
        this.handleJump(ref velocity, delta);

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
        this.Modulate = Color.Color8(255, 255, 255, 255);
        this.CollisionLayer |= 1u << 3;
        this.GetNode<Area2D>("Area2D").CollisionLayer |= 1u << 3;
        inRespawn = false;
    }

    protected void handleJump(ref Vector2 velocity, double delta) {
        if (Input.IsActionPressed("jump")) {
            this.jumpTimer = !IsOnFloor() ? this.jumpTimer - (float) delta : this.totalJumpTimer;
        } else {
            this.jumpTimer = !IsOnFloor() && this.jumps <= 0 ? 0 : this.totalJumpTimer;
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && ( this.jumpTimer >= 0 || this.jumps > 0 )) {
            this.sprite.Play("jump");
            velocity.Y = jumpVelocity * this.totalJumpTimer;
            jumps--;
        }
    }

    protected void handleMovement(ref Vector2 velocity) {
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        if (direction.X != 0) {
            velocity.X = direction.X * speed * ( this.IsOnFloor() ? 1 : .75f );
            if (counter == -90) {
                this.sprite.FlipH = direction.X < 0;
            }
            if (velocity.Y == 0) {
                this.sprite.Play("walk");
            }
        } else {
            velocity.X = Mathf.MoveToward(this.Velocity.X, 0, speed);
            if (velocity.Y == 0) {
                this.sprite.Play("idle");
            }
        }
    }

    protected void handleAttack() {
        bool isAttacking = this.counter != -90 && this.counter != 90 && this.counter != -270;
        this.attackHitbox.Enabled = isAttacking;
        if (!isAttacking) {
            this.counter = -90;
            this.attackHitbox.GlobalRotationDegrees = counter;
            this.attackCD.Clear();
        }

        if (Input.IsActionPressed("attack") && !isAttacking) {
            if (this.sprite.FlipH) {
                this.counter -= 5;
            } else {
                this.counter += 5;
            }
            this.attackHitbox.GlobalRotationDegrees = Math.Max(Math.Min(counter, 90), -270);
        }

        if (isAttacking) {
            if (this.counter < -90) {
                this.counter -= 5;
            } else {
                this.counter += 5;
            }
            this.attackHitbox.GlobalRotationDegrees = Math.Max(Math.Min(counter, 90), -270);
        }

        if (this.attackHitbox.IsColliding()) {
            GodotObject collider = this.attackHitbox.GetCollider();
            if (collider != null && !this.attackCD.Contains(collider.GetInstanceId())) {
                AttackBody(this.attackHitbox.GetCollider());
                this.attackCD.Add(collider.GetInstanceId());
            }
        }
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
        float outside_damage = (float) node.GetMeta("damage", 10);
        //TODO: Extra damage handling here later
        if (( PlayerInfo.Health -= outside_damage ) <= 0) {
            RespawnAnim();
            return false;
        }
        GD.Print("Damaged: ", PlayerInfo.Health, " HP");
        return true;
    }

    public void AttackBody(GodotObject body) {
        if (body is not BaseEnemy attackable) {
            return;
        }
        attackable.HandleDamage(this.damage);
    }

    private uint _layer;
    private Dictionary<ulong, Timer> damageCooldowns = new();
    public void AreaEntered(Area2D area) {
        if (area.HasMeta("damage")) {
            if (!HandleDamage(area)) {
                return;
            }
            var timer = new Timer();
            this.damageCooldowns.Add(area.GetInstanceId(), timer);
            timer.Autostart = true;
            timer.WaitTime = this.DamageCDMS / 1000f;
            timer.Timeout += () => HandleDamage(area);
            this.dcdHolder.AddChild(timer, false, InternalMode.Back);
            return;
            // this.damageCooldown.Timeout += this._dc = () => HandleDamage(area);
        }
        if (area.HasMeta("pickup")) {
            Pickup pickup = area.GetParent<Pickup>();
            HUD.hud.player_inventory.addItem(pickup.getStack());
            pickup.QueueFree();
        }
        if (area.HasMeta("InstantDeath")) {
            this.RespawnAnim();
        }
    }

    public void AreaLeft(Area2D area) {
        if (area.HasMeta("damage")) {
            if (!damageCooldowns.Remove(area.GetInstanceId(), out Timer timer)) {
                return;
            }
            timer.QueueFree();
        }
    }
    //TODO: Fall from the Sky respawn
    public void RespawnAnim() {
        GD.Print("Start Respawning");
        inRespawn = true;
        var tween = CreateTween();
        _layer = this.PlatformFloorLayers;
        this.PlatformFloorLayers = 0;
        this.GetNode<Area2D>("Area2D").CollisionLayer &= ~(1u << 3);
        sprite.Play("hurt");
        tween.SetParallel();
        tween.TweenProperty(this, "position", new Vector2(this.Position.X, this.Position.Y - 50), 1);
        tween.TweenProperty(this, "modulate", Color.Color8(255, 255, 255, 0), 1);
        tween.Chain();
        tween.TweenCallback(new(this, nameof(Respawn)));
    }

}
