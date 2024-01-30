using Godot;
using System;

public partial class Slot : PanelContainer {

    [Signal]
    public delegate void on_clickEventHandler(int index, MouseButton button);
    public int index = 0;
    public bool is_hover = false;
    private ItemStack stack;
    private TextureRect render;
    private RichTextLabel label;
    private SpriteFrames frames;
    private double frame = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.render = this.GetNode<TextureRect>("%Render");
        this.label = this.GetNode<RichTextLabel>("%Count");
        // on_click = new(this, "on_click");
        // on_click
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (this.stack == null) {
            return;
        }
        if (this.stack.count <= 0) {
            reset();
            return;
        }
        if (this.is_hover) {
            this.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            this.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            this.SelfModulate = Color.Color8(255, 255, 255, 0);
            this.ZIndex = 1;
            if (this.Visible) {
                this.GlobalPosition = this.GetGlobalMousePosition() + new Vector2(5, 5);
            }
            if (this.stack == null) {
                this.Visible = false;
            } else {
                this.Visible = true;
            }
        }
        int count = frames.GetFrameCount("item");
        if (this.frame >= count) {
            this.frame = 0;
        }

        this.render.Texture = this.frames.GetFrameTexture("item", (int) Math.Floor(this.frame));
        this.frame += this.frames.GetAnimationSpeed("item") * delta;
    }

    public void setIndex(int i) {
        this.index = i;
    }

    public void reset(bool remove_stack = true) {
        if (remove_stack) {
            this.stack = null;
        }
        this.frame = 0;
        this.frames = null;
        this.render.Texture = null;
        this.label.Text = "";
        this.TooltipText = "";
    }

    public void update() {
        reset(false);
        if (this.stack == null) {
            return;
        }
        Item item = stack.getItem();
        this.frames = item.frames;
        this.label.Text = "x" + stack.count;
        this.TooltipText = $"{stack.display_name}\n{item.description}";
        this.frame = 0;
    }

    public void setStack(ItemStack stack) {
        if (this.stack != null) {
            this.stack.on_change -= update;
        }
        this.stack = stack;
        if (stack != null) {
            stack.on_change += update;
        }
        update();
    }

    public bool isStackable(ItemStack stack) {
        if (this.stack == null) {
            return false;
        }
        return this.stack.isStackable(stack);
    }

    public int stackItem(ItemStack stack, bool merge_only = false) {
        if (this.stack == null) {
            if (!merge_only) {
                this.setStack(stack.take(int.MaxValue));
            }
            return merge_only ? stack.count : 0;
        }
        if (!this.stack.isStackable(stack)) {
            return stack.count;
        }
        int result = this.stack.merge(stack);
        return result;
    }

    public ItemStack split() {
        if (this.stack == null) {
            return null;
        }

        ItemStack result = this.stack.split();
        return result;
    }

    public ItemStack swap(ItemStack stack) {
        ItemStack old = this.stack;
        this.setStack(stack);
        return old;
    }

    public void addOneFrom(ItemStack stack) {
        if (stack == null) {
            return;
        }
        if (this.stack == null) {
            this.setStack(stack.take(1));
            return;
        }
        stack.takeOne(this.stack);
    }

    public void use() {
        this.stack?.use();
    }

    public ItemStack getStack() {
        return this.stack;
    }

    public override void _GuiInput(InputEvent @event) {
        if (@event is not InputEventMouseButton input || !@event.IsPressed()) {
            return;
        }
        this.AcceptEvent();
        EmitSignal(SignalName.on_click, this.index, (long) input.ButtonIndex);
    }

    protected override void Dispose(bool disposing) {
        if (this.stack != null) {
            this.stack.on_change -= update;
        }
        base.Dispose(disposing);
    }

}
