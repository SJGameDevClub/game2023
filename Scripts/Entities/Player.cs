using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D {

    [Export]
    public float speed = 350.0f;

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
    public Vector2 spawnpoint = Vector2.Zero;

    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    public float speed_multiplier = 1f;

    private int jumps = 2;
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private AnimatedSprite2D sprite;
    private bool inRespawn = false;
    private TileMap tilemap;
    private Node dcd_holder;
    private RayCast2D attack_hitbox;
    private uint _layer;
    private Dictionary<ulong, Timer> damageCooldowns = new();

    public override void _Ready() {
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.sprite.Play("idle");
        this.tilemap = this.GetTree().CurrentScene.GetNode<TileMap>("TileMap");
        this.dcd_holder = this.GetNode("Damage CD Holder");
        this.attack_hitbox = this.GetNode<RayCast2D>("Weapon Hitbox");
        this.AddChild(new Camera2D());
        if (this.spawnpoint == Vector2.Zero) {
            this.spawnpoint = GlobalPosition;
        }
    }

    public override void _Process(double delta) {
        if (inRespawn || this.Owner == null) {
            return;
        }
        try {
            this.tilemap.GetIndex();
        } catch (Exception e) when (e is ObjectDisposedException || e is NullReferenceException) {
            this.tilemap = this.GetTree().CurrentScene.GetNode<TileMap>("TileMap");
        }
    }

    bool draw = false;
    private float jumpTimer = 0f;
    private int counter = -90;
    private List<ulong> attackCD = new();
    private TileData current_water_tile;
    public override void _PhysicsProcess(double delta) {
        if (inRespawn || this.Owner == null) {
            return;
        }
        Vector2 velocity = this.Velocity;
        this.current_water_tile = this.tilemap.GetCellTileData(3, this.tilemap.LocalToMap(this.Position));
        if (this.current_water_tile != null && ( (bool) this.current_water_tile.GetCustomData("liquid") )) {
            this.speed_multiplier = .6f;
        } else {
            this.speed_multiplier = 1;
        }
        // GD.PrintS(this.current_water_tile, this.tilemap.LocalToMap(this.Position), this.tilemap.LocalToMap(this.ToLocal(this.GetViewport().GetMousePosition())), this.tilemap.LocalToMap(this.tilemap.ToLocal(this.GetViewport().GetMousePosition())), this.tilemap.LocalToMap(this.tilemap.ToLocal(this.GlobalPosition)));

        // Add the gravity.
        if (!IsOnFloor() && (!Input.IsActionPressed("jump") || this.jumpTimer <= 0)) {
            velocity.Y = this.current_water_tile != null && ((bool) this.current_water_tile.GetCustomData("liquid")) ? (this.gravity * (float) delta) * 4.5f * (Input.IsActionPressed("down") ? 2f : 1) : Mathf.Min(velocity.Y + this.gravity * (float) delta, 2000);
        }

        if (this.IsOnFloor()) {
            this.jumps = this.totalJumps;
        }

        if (!Utils.text_focused) {
            this.handleAttack();
            this.handleMovement(ref velocity);
            this.handleJump(ref velocity, delta);
        }

        if (velocity.Y > 0 && this.sprite.Animation != "jump") {
            this.sprite.Play("jump");
            this.sprite.Frame = 2;
        }

        this.Velocity = velocity;
        this.MoveAndSlide();

        if (PlayerInfo.Health <= 0) {
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
        this.sprite.FlipH = false;
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

        if (this.current_water_tile != null && (bool) this.current_water_tile.GetCustomData("liquid")) {
            this.jumps = this.totalJumps;
            this.jumpTimer = this.totalJumpTimer;
        }
    }

    protected void handleMovement(ref Vector2 velocity) {
        Vector2 direction = Input.GetVector("left", "right", "interact", "down");
        if (direction.X != 0) {
            velocity.X = direction.X * speed * ( this.IsOnFloor() ? 1 : .75f );
            if (this.counter == -90) {
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
        if (Input.IsActionJustPressed("interact") && this.door != null) {
            Utils.EnterArea(this, this.door);
        }

        velocity.X *= this.speed_multiplier;
    }

    protected void handleAttack() {
        bool isAttacking = this.counter != -90 && this.counter != 90 && this.counter != -270;
        this.attack_hitbox.Enabled = isAttacking;
        if (!isAttacking) {
            this.counter = -90;
            this.attack_hitbox.GlobalRotationDegrees = counter;
            this.attackCD.Clear();
        }

        if (Input.IsActionPressed("attack") && !isAttacking) {
            if (this.sprite.FlipH) {
                this.counter -= 5;
            } else {
                this.counter += 5;
            }
            this.attack_hitbox.GlobalRotationDegrees = Math.Max(Math.Min(counter, 90), -270);
        }

        if (isAttacking) {
            if (this.counter < -90) {
                this.counter -= 5;
            } else {
                this.counter += 5;
            }
            this.attack_hitbox.GlobalRotationDegrees = Math.Max(Math.Min(counter, 90), -270);
        }

        if (this.attack_hitbox.IsColliding()) {
            GodotObject collider = this.attack_hitbox.GetCollider();
            if (collider != null && !this.attackCD.Contains(collider.GetInstanceId())) {
                AttackBody(this.attack_hitbox.GetCollider());
                this.attackCD.Add(collider.GetInstanceId());
            }
        }
    }

    public override void _Draw() {
        base._Draw();
        if (draw) {
            DrawCircle((GetParent() as Node2D).Position, 50f, Color.Color8(255, 0, 0));
            DrawCircle(Position, 50f, Color.Color8(0, 0, 255));
            DrawCircle(tilemap.MapToLocal(new(0, 0)), 50f, Color.Color8(0, 255, 0));
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

    private Door door;
    public List<(PackedScene scene, Vector2 pos)> prev_scenes = new();
    public PackedScene prev_door_scene = null;
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
            this.dcd_holder.AddChild(timer, false, InternalMode.Back);
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

        if (area.HasMeta("door")) {
            this.door = (Door) area;
            if (this.prev_door_scene != null) {
                this.door.scene = this.prev_door_scene;
                this.prev_door_scene = null;
            }
        }
    }

    public void AreaLeft(Area2D area) {
        if (area.HasMeta("damage")) {
            if (!damageCooldowns.Remove(area.GetInstanceId(), out Timer timer)) {
                return;
            }
            timer.QueueFree();
        }
        if (area.HasMeta("door")) {
            this.door = null;
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
        this.sprite.Play("hurt");
        tween.SetParallel();
        tween.TweenProperty(this, "position", new Vector2(this.Position.X, this.Position.Y - 50), 1);
        tween.TweenProperty(this, "modulate", Color.Color8(255, 255, 255, 0), 1);
        tween.Chain();
        tween.TweenCallback(new(this, nameof(Respawn)));
    }

}
