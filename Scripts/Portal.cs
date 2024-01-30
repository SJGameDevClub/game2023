using Godot;
using System;

public partial class Portal : Area2D {

    public AnimatedSprite2D sprite;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (this.sprite.SpriteFrames.GetFrameCount("default") == 1) {
            this.sprite.Rotation = 0;
            return;
        }
        this.sprite.Rotation += (float) Math.PI / 6;
    }

}
