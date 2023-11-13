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

    private bool inRespawn = false;
    private TileMap tileMap;

    public override void _Ready() {
        this.sprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        this.sprite.Play("idle");
        this.tileMap = this.GetParent().GetNode<TileMap>("TileMap");
        this.AddChild(new Camera2D());
        spawnpoint = Position;
    }

    bool draw = false;
    public override void _PhysicsProcess(double delta) {
        if (inRespawn) {
            return;
        }
        Vector2 velocity = this.Velocity;

        // Add the gravity.
        if (!IsOnFloor()) {
            velocity.Y = Mathf.Min(velocity.Y + this.gravity * (float)delta, 2000);
        }
        
        if (Input.IsKeyPressed(Key.Semicolon)) {
            GD.Print("Node ", Position, ": ", Position + new Vector2(0, 35), ", ToLocal ", this.ToLocal(Position), ": ", this.ToLocal(Position + new Vector2(0, 35)), "; Parent ", (GetParent() as Node2D).Position, ": ", (GetParent() as Node2D).Position + new Vector2(0, 35), ", ToLocal ", this.ToLocal((GetParent() as Node2D).Position), " : ", this.ToLocal((GetParent() as Node2D).Position + new Vector2(0, 35)));
            GD.Print(tileMap.MapToLocal(new(0, 0)));
            draw = true;
            this.QueueRedraw();
        }
        if (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(Key.Backslash)) {
            GD.Print(this.GetGlobalMousePosition());
            GD.Print(tileMap.GetCellTileData(5, tileMap.LocalToMap(this.GetGlobalMousePosition())));
            GD.Print(tileMap.GetCellTileData(5, tileMap.LocalToMap(ToLocal(this.GetGlobalMousePosition()))));
        }

        var a = tileMap.GetCellTileData(6, tileMap.LocalToMap((GetParent() as Node2D).Position));
        if (a != null) {
            GD.Print(a);
        }
        if (Input.IsKeyPressed(Key.Apostrophe)) {
            GD.Print(a);
        }
        // Handle Jump.
        if (Input.IsActionJustPressed("jump") && this.IsOnFloor()) {
            this.sprite.Play("jump"); 
            velocity.Y = jumpVelocity;
        }

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("left", "right", "up", "down");
        if (direction.X != 0) {
            velocity.X = direction.X * speed;
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

    }

    public void Respawn() {
        this.Position = spawnpoint;
        inRespawn = false;
        this.PlatformFloorLayers = _layer;
        sprite.FlipH = false;
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

    private uint _layer;
    public void AreaEntered(Area2D area) {
        if (!area.HasMeta("InstantDeath")) {
            return;
        }
        GD.Print("Start Respawning");
        inRespawn = true;
        var tween = CreateTween();
        _layer = this.PlatformFloorLayers;
        this.PlatformFloorLayers = 0;
        sprite.Play("hurt");
        tween.TweenProperty(this, "position", new Vector2(this.Position.X + 10, this.Position.Y + 10), 1);
        tween.TweenCallback(new(this, nameof(Respawn)));
    }

}
