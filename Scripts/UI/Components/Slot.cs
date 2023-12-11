using Godot;
using System;

public partial class Slot : PanelContainer {

    [Signal]
    public delegate void on_clickEventHandler(int index, MouseButton button);
    public int index = 0;
    private ItemStack stack;
    private TextureRect render;
    private RichTextLabel label;

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
        }
    }

    public void setIndex(int i) {
        this.index = i;
    }

    public void reset(bool remove_stack = true) {
        if (remove_stack) {
            this.stack = null;
        }
        this.render.Texture = null;
        this.label.Text = "";
        this.TooltipText = "";
    }

    public void redraw() {
        reset(false);
        if (this.stack == null) {
            return;
        }
        Item item = stack.getItem();
        this.render.Texture = item.texture;
        this.label.Text = "x" + stack.count;
        this.TooltipText = $"{stack.display_name}\n{item.description}";
    }

    public void setStack(ItemStack stack) {
        this.stack = stack;
        try {
            stack.on_change += redraw;
        } catch (NullReferenceException) {/* Stack is nullable */}
        redraw();
    }

    public bool isStackable(ItemStack stack) {
        if (this.stack == null) {
            return false;
        }
        return this.stack.isStackable(stack);
    }

    public int stackItem(ItemStack stack) {
        if (this.stack == null) {
            this.setStack(stack);
            return 0;
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
            this.setStack(stack.takeOne());
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
        
        EmitSignal(SignalName.on_click, this.index, (long) input.ButtonIndex);
    }

}
