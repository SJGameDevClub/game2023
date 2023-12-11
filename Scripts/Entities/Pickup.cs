using Godot;
using System;

public partial class Pickup : RigidBody2D {

    [Export]
    private ItemStack stack;
    protected Sprite2D render;

    public Pickup(ItemStack stack) {
        if (stack == null) {
            this.QueueFree();
        }
        this.stack = stack;
    }

    public ItemStack getStack() {
        return this.stack;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.render = this.GetNode<Sprite2D>("%Render");
        this.GetNode("%Character Pickup").SetMeta("pickup", true);
        this.render.Texture = this.stack.getItem().texture;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private Pickup() {}
}
