using Godot;
using System;

[Tool]
public partial class BaseEnemy : CharacterBody2D {
    public const float speed = 200.0f;
    [Export]
    public float damage = 10;
    [Export]
    public float health = 10;
    [Export]
    public float max_health = 10;
    [Export]
    public float speed_multiplier = 1.0f;
    [Export]
    public bool patrol = true;
    [Export]
    protected Vector2 point_1;
    [Export]
    protected Vector2 point_2;
    [Export]
    protected SpriteFrames animations;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    protected AnimatedSprite2D sprite;
    protected Area2D player_detection;
    protected TextureProgressBar health_bar; 
    // public Area2D hitbox;
    public Vector2 last_player_position = Vector2.Zero;
    public Vector2 respawn_point;
    public bool player_within => this.player != null;
    public Node2D player;

    public override void _Ready() {
        if (!Engine.IsEditorHint()) {

            Callable.From(() => {
                var patrol_1 = this.GetNode<Node2D>("Patrol 1");
                var patrol_2 = this.GetNode<Node2D>("Patrol 2");
                if (this.point_1 == default) {
                    this.point_1 = patrol_1.GlobalPosition;
                }
                if (this.point_2 == default) {
                    this.point_2 = patrol_2.GlobalPosition;
                }
                patrol_1.QueueFree();
                patrol_2.QueueFree();
            }).CallDeferred();
        }
        this.respawn_point = this.GlobalPosition;
        this.sprite = this.GetNode<AnimatedSprite2D>("%Render");
        this.player_detection = this.GetNode<Area2D>("Player Detector");
        this.health_bar = this.GetNode<TextureProgressBar>("Health Bar");
        this.GetNode("Hitbox Checker").SetMeta("damage", this.damage);
    }

    public override void _Draw() {
        base._Draw();
        if (Test.run) {
            this.DrawRect(new Rect2(ToLocal(this.point_1 - (Vector2.One * 15)), new Vector2(10, 10)), Color.Color8(255, 255, 255));
            this.DrawRect(new Rect2(ToLocal(this.point_2 - (Vector2.One * 15)), new Vector2(10, 10)), Color.Color8(255, 255, 255));
        }
    }

    public override void _Process(double delta) {
        this.sprite.SpriteFrames = this.animations;

        if (this.Velocity.X != 0) {
            this.sprite.FlipH = this.Velocity.X < 0;
            if (this.Velocity.Y == 0) {
                this.sprite.Play("walk");
            }
        }
        if (!this.sprite.IsPlaying()) {
            this.sprite.Play("idle");
        }
    }

    public override void _PhysicsProcess(double delta) {
        
        if (Engine.IsEditorHint()) {
            return;
        }
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor()) {
            velocity.Y += Mathf.Min(velocity.Y + this.gravity * (float) delta, 10);
        }

        if (PlayerInfo.Health <= 0) {
            this.last_player_position = Vector2.Zero;
            this.player = null;
        }
        float distanceFromLPP = new Vector2(this.GlobalPosition.X, 0).DistanceTo(new(this.last_player_position.X, 0));
        if (this.player_within) {
            Vector2 playerPos = this.player.GlobalPosition;
            float distance = new Vector2(this.GlobalPosition.X, 0).DistanceTo(new(playerPos.X, 0));
            Vector2 direction = ( playerPos - this.GlobalPosition ).Normalized();
            if (distance > 48.2) {
                velocity.X = (direction.X < 0 ? -1 : 1) * (speed * speed_multiplier);
            }
            Mathf.MoveToward(velocity.X, 0, speed * this.speed_multiplier);
            // if (position.DistanceTo(new Vector2(playerPos.X, position.Y)) < 45 && this.IsOnFloor()) {
            //     float amount = playerPos.Y - position.Y;
            //     if (amount < 0) {
            //         velocity.Y = jumpVelocity;
            //     }
            //     GD.Print(amount);
            // }
            // GD.Print(position.DistanceTo(new Vector2(playerPos.X, position.Y)), " ", direction);

        } else if (this.last_player_position != Vector2.Zero && distanceFromLPP > 35 ) {
            Vector2 direction = ( this.last_player_position - this.GlobalPosition ).Normalized();
            velocity.X = ( direction.X < 0 ? -1 : 1 ) * (speed * this.speed_multiplier);
        } else {
            velocity.X = Mathf.MoveToward(velocity.X, 0, speed * this.speed_multiplier);
        }

        if (velocity.X != 0) {
            // velocity.X = direction.X * speed;
            this.sprite.FlipH = velocity.X < 0;
        }

        Velocity = velocity;
        this.health_bar.Value = this.health;
        this.health_bar.MaxValue = this.max_health;
        this.QueueRedraw();
        this.MoveAndSlide();
    }

    public void DetectionEntered(Area2D area) {
        if (area.GetParent().HasMeta("Player")) {
            this.player = area;
        }
    }

    public void DetectionLeft(Area2D area) {
        if (this.player == null) {
            return;
        }
        this.last_player_position = this.player.GlobalPosition;
        this.player = null;
    }

    public void HitboxAreaEntered(Area2D area) {
        if (area.HasMeta("InstantDeath")) {
            this.health = 0f;
            this.HandleDamage(0);
        }
    }

    public void HandleDamage(float amount) {
        this.health -= amount;
        this.sprite.Play("hurt");
        if (this.health < 1) {
            Test.StartEnemyRespawn(this);
            this.QueueFree();
        }
    }

}
