using Godot;
using System;

public partial class BaseEnemy : CharacterBody2D {
    public const float speed = 200.0f;
    public const float jumpVelocity = -15;
    [Export]
    public float damage = 10;
    [Export]
    public float health = 10;
    public float max_health = 10;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    protected AnimatedSprite2D sprite;
    protected Node2D patrol_1;
    protected Node2D patrol_2;
    protected Area2D player_detection;
    protected TextureProgressBar health_bar; 
    // public Area2D hitbox;
    public Vector2 last_player_position = Vector2.Zero;
    public Vector2 respawn_point;
    public bool player_within => this.player != null;
    public Node2D player;

    public override void _Ready() {
        this.respawn_point = this.GlobalPosition;
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.patrol_1 = this.GetNode<Node2D>("../Patrol 1");
        this.patrol_2 = this.GetNode<Node2D>("../Patrol 2");
        this.player_detection = this.GetNode<Area2D>("Player Detector");
        this.health_bar = this.GetNode<TextureProgressBar>("Health Bar");
        this.GetNode("Hitbox Checker").SetMeta("damage", this.damage);
        this.health_bar.MaxValue = this.max_health;
    }

    public override void _PhysicsProcess(double delta) {
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor()) {
            velocity.Y += Mathf.Min(velocity.Y + this.gravity * (float) delta, 10);
        }

        float distanceFromLPP = new Vector2(this.GlobalPosition.X, 0).DistanceTo(new(this.last_player_position.X, 0));
        if (this.player_within) {
            Vector2 playerPos = this.player.GlobalPosition;
            float distance = new Vector2(this.GlobalPosition.X, 0).DistanceTo(new(playerPos.X, 0));
            Vector2 direction = ( playerPos - this.GlobalPosition ).Normalized();
            if (distance > 48.2) {
                velocity.X = (direction.X < 0 ? -1 : 1) * speed;
            }
            Mathf.MoveToward(velocity.X, 0, speed);
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
            velocity.X = ( direction.X < 0 ? -1 : 1 ) * speed;
        } else {
            velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
        }

        if (velocity.X != 0) {
            // velocity.X = direction.X * speed;
            this.sprite.FlipH = velocity.X < 0;
        }

        Velocity = velocity;
        this.health_bar.Value = this.health;
        MoveAndSlide();
    }

    public void DetectionEntered(Area2D area) {
        if (area.GetParent().HasMeta("Player")) {
            this.player = area;
        }
    }

    public void DetectionLeft(Area2D area) {
        this.last_player_position = this.player.GlobalPosition;
        if (area.GetParent().HasMeta("Player")) {
            this.player = null;
        }
    }

    public void HandleDamage(float amount) {
        this.health -= amount;
        if (this.health < 1) {
            Test.StartEnemyRespawn(this);
            this.QueueFree();
        }
    }

    // public void HitboxEntered(Area2D area) {
    //     if (area.GetParent().HasMeta("Player")) {
    //         // var child = ResourceLoader.Load<PackedScene>(this.GetParent().SceneFilePath).Instantiate<Node2D>();
    //         // child.Position = respawnPoint;
    //         // this.GetTree().CurrentScene.AddChild(child);
    //         GD.Print("Hit");
    //     }
    // }

}
