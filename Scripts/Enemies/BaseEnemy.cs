using Godot;
using System;

public partial class BaseEnemy : CharacterBody2D {
    public const float speed = 200.0f;
    public const float jumpVelocity = -15;
    [Export]
    public float damage = 10;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public AnimatedSprite2D sprite;
    public Node2D patrol_1;
    public Node2D patrol_2;
    public Area2D playerDetection;
    // public Area2D hitbox;
    public Vector2 lastPlayerPosition = Vector2.Zero;
    public Vector2 respawnPoint;
    public bool playerWithin => this.player != null;
    public Node2D player;

    public override void _Ready() {
        this.respawnPoint = this.GlobalPosition;
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.patrol_1 = this.GetNode<Node2D>("../Patrol 1");
        this.patrol_2 = this.GetNode<Node2D>("../Patrol 2");
        this.playerDetection = this.GetNode<Area2D>("Player Detector");
        this.GetNode("Hitbox Checker").SetMeta("damage", this.damage);
    }

    public override void _PhysicsProcess(double delta) {
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor()) {
            velocity.Y += Mathf.Min(velocity.Y + this.gravity * (float) delta, 10);
        }

        var position = this.GlobalPosition;
    
        if (this.playerWithin) {
            var playerPos = this.player.GlobalPosition;
            var direction = ( playerPos - position ).Normalized();
            if (!(direction.X < 0.185 && direction.X > -0.185)) {
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

        } else {
            velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
        }

        if (velocity.X != 0) {
            // velocity.X = direction.X * speed;
            this.sprite.FlipH = velocity.X < 0;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public void DetectionEntered(Area2D area) {
        if (area.GetParent().HasMeta("Player")) {
            GD.Print("Entered");
            this.player = area;
        }
    }

    public void DetectionLeft(Area2D area) {
        if (area.GetParent().HasMeta("Player")) {
            GD.Print("Exited");
            this.player = null;
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
