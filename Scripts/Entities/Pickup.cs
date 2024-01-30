using Godot;
using System;

[Tool]
public partial class Pickup : RigidBody2D {

    private ItemStack _stack;

    [Export(PropertyHint.Link)]
    public Vector2 render_scale = Vector2.One;
    [Export(PropertyHint.Link)]
    public Vector2 scale = Vector2.One;
    protected Sprite2D render;
    // protected SpriteFrames frames;
    protected double frame = 0;
    [Export]
    private ItemStack stack {
        get {
            return _stack ??= ItemStack.of(Items.Missing);
        }
        set => _stack = value;
    }

    public Pickup(ItemStack stack) {
        this.stack = stack;
    }

    public ItemStack getStack() {
        return this.stack;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.render = this.GetNode<Sprite2D>("%Render");
        this.GetNode("%Character Pickup").SetMeta("pickup", true);
        // this.render.Texture = this.stack.getItem().texture;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (this.stack == null) {
            if (!Engine.IsEditorHint()) {
                this.QueueFree();
            }
            return;
        }
        SpriteFrames frames = this.stack.getItem().frames;
        if (frames == null) {
            return;
        }
        int count = frames.GetFrameCount("item");
        if (this.frame >= count) {
            this.frame = 0;
        }
        this.Scale = this.scale;
        this.render.Scale = this.render_scale;
        this.render.Texture = frames.GetFrameTexture("item", (int) Math.Floor(frame));
        this.frame += frames.GetAnimationSpeed("item") * delta;
    }

    private Pickup() {
        try {
            if (this.stack != null) {
                this.stack = ItemStack.of(Items.Missing);
            }
        } catch {}
    }
}
