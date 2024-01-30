using Godot;
using System;

[Tool]
public partial class Door : Area2D {

    private Sprite2D top;
    private Sprite2D bottom;
    private CollisionShape2D collision;
    [Export]
    public Texture2D top_texture;
    [Export]
    public Texture2D bottom_texture;
    [Export]
    public PackedScene scene = null;
    // [Export(PropertyHint.Link)]
    // public Vector2 image_size;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.top = this.GetNode<Sprite2D>("Top");
        this.bottom = this.GetNode<Sprite2D>("Bottom");
        this.collision = this.GetNode<CollisionShape2D>("CollisionShape2D");
        this.SetMeta("door", true);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (this.top_texture == null || this.bottom_texture == null) {
            return;
        }
        Vector2 top_text_size = top_texture.GetSize();
        Vector2 bottom_text_size = bottom_texture.GetSize();
        this.top.Offset = new(0, -top_text_size.Y / 2);
        this.bottom.Offset = new(0, bottom_text_size.Y / 2);
        this.top.Texture = this.top_texture;
        this.bottom.Texture = this.bottom_texture;
        ((RectangleShape2D) this.collision.Shape).Size = new(Math.Max(top_text_size.X, bottom_text_size.X), top_text_size.Y + bottom_text_size.Y);
    }
}
