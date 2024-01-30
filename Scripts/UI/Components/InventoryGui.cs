using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An interface for the GUI
/// </summary>
public partial class InventoryGui : PanelContainer {

    public static int default_columns = 5;
    public static PackedScene inventory_gui_scene {get;} = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Inventory.tscn");
    public static PackedScene slot_scene {get;} = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Slot.tscn");

    protected static ItemStack held_item;
    protected static Slot held_item_container;
    public int columns = default_columns;
    [Export]
    public bool is_closable = false;
    [Export]
    public string title = "";
    [Export]
    public bool use_header = false;
    protected GridContainer slot_container;
    protected List<Slot> slots = new();
    protected List<ItemStack> _items;
    protected RichTextLabel label;
    protected HBoxContainer header;
    protected Button close_button;

    [Signal]
    public delegate void on_closeEventHandler(Inventory inventory);

    [Export]
    public int size {get; private set;} = 5;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.slot_container = this.GetNode<GridContainer>("%Slots");
        // this.held_item_container = this.GetNode<Slot>("%HoverItem");
        this.label = this.GetNode<RichTextLabel>("%Title");
        this.header = this.GetNode<HBoxContainer>("%Header");
        this.close_button = this.GetNode<Button>("%Close");
        Create();
        if (_items != null) {
            this.setSlots(_items);
            _items = null;
        }
        // this.SetSlots(new() {ItemWrapper.createItem("res://Items/Cherry.tres")});
        // ResourceSaver.Save(new InventoryResource(new Array<ItemStack>(this.slots.Select(data => data.getItem())), this.size), "res://test.tres");
    }

    private Vector2 gi_offset = new(5, 5);
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (held_item_container == null) {
            held_item_container = HUD.hud.held_item_container;
        }
        this.slot_container.Columns = this.columns;
        // if (this.held_item_container.Visible) {
        //     this.held_item_container.Position = this.GetGlobalMousePosition() + gi_offset;
        // }
        this.label.Text = this.title;
        this.header.Visible = this.use_header;
        this.close_button.Visible = this.is_closable;
    }

    Vector2 old_pos;
    Vector2 offset;
    bool is_dragging = false;
    public override void _GuiInput(InputEvent @event) {

        if (@event is InputEventKey key && this.is_dragging && key.Keycode == Key.Escape) {
            this.is_dragging = false;
            this.GlobalPosition = this.old_pos;
        }

        if (@event is InputEventMouseMotion && this.is_dragging) {
            this.GlobalPosition = this.GetGlobalMousePosition() - this.offset;
            // this.GlobalPosition = motion.GlobalPosition;
        }

        if (@event is InputEventMouseButton button && button.ButtonIndex == MouseButton.Left) {
            if (button.IsPressed()) {
                this.old_pos = this.GlobalPosition;
                this.offset = this.GetGlobalMousePosition() - this.GlobalPosition;
                this.is_dragging = true;
            } else if (button.IsReleased() && this.is_dragging) {
                this.is_dragging = false;
            }
        }
    }

    // public void redrawHeldItem() {
    //     if (held_item != null) {
    //         held_item_container.Show();
    //         held_item_container.setStack(held_item);
    //     } else {
    //         held_item_container.Hide();
    //     }
    // }

    /// <summary>
    /// Sets the Item in the indexed slot
    /// </summary>
    /// <param name="index">The slot's index</param>
    /// <param name="stack">The item to set</param>
    public void setSlot(int index, ItemStack stack) {
        this.slots[index].setStack(stack);
    }

    /// <summary>
    /// Populates slots with items
    /// </summary>
    /// <param name="items"></param>
    public void setSlots(List<ItemStack> items) {
        if (items == null) {
            return;
        }
        // foreach (Slot slot in slots) {
        //     ItemStack item = slot.getItem();
        //     item?.Free();
        // }
        GD.PrintS(size, items.Count);
        for (int i = 0; i < this.size; ++i) {
            if (i >= items.Count) {
                this.slots[i].setStack(null);
                continue;
            }
            this.slots[i].setStack((ItemStack) items[i]?.Duplicate());
        }
    }

    /// <summary>
    /// Adds to the inventory
    /// </summary>
    /// <returns>The amount of items remaining in the stack</returns>
    public int addItem(ItemStack stack) {
        int origin = stack.count;

        List<Slot> cached = new();
        foreach (Slot slot in this.slots) {
            int old = stack.count;
            if (old == slot.stackItem(stack, true)) {
                cached.Add(slot);
            }
            if (stack.count == 0) {
                break;
            }
        }
        if (stack.count > 0) {
            foreach (Slot slot in cached) {
                slot.stackItem(stack, false);
                if (stack.count == 0) {
                    break;
                }
            }
        }
        return origin - stack.count;
    }

    /// <summary>
    /// Resizes the Inventory
    /// </summary>
    /// <param name="size">New size of inventory</param>
    /// <returns>The old size of the inventory</returns>
    public int setSize(int size) {
        int old_size = this.size;
        this.size = size;
        this.Create();
        return old_size;
    }

    public void close() {
        EmitSignal(SignalName.on_close, Save(this));
        this.QueueFree();
    }

    protected void handleClick(int index, MouseButton button) {
        Slot slot = this.slots[index];
        // GD.PrintS(index, button, slot.getStack());
        if (slot.getStack() != null && slot.getStack().custom_data.ContainsKey("inv_open") && (bool) slot.getStack().custom_data["inv_open"]) {
            return;
        }
        switch ((button, held_item)) {
            case (MouseButton.Left, null): {
                held_item = slot.getStack();
                slot.setStack(null);
                break;
            }
            case (MouseButton.Left, _): {
                if (!slot.isStackable(held_item)) {
                    held_item = slot.swap(held_item);
                    break;
                }
                int old_count = held_item.count;
                if (slot.stackItem(held_item) == old_count) {
                    held_item = slot.swap(held_item);
                }
                break;
            }
            case (MouseButton.Right, null): {
                slot.use();
                break;
            }
            case (MouseButton.Right, _): {
                slot.addOneFrom(held_item);
                break;
            }
            case (MouseButton.Middle, null): {
                held_item = slot.split();
                break;
            }
        }
        if (held_item?.count < 1) {
            held_item = null;
        }
        // redrawHeldItem();
        held_item_container.setStack(held_item);
    }

    /// <summary>
    /// Creates the Slot objects to be used within
    /// </summary>
    private void Create() {
        foreach (Node node in slot_container.GetChildren()) {
            node.QueueFree();
        }
        slots.Clear();
        for (int i = 0; i < this.size; ++i) {
            Slot slot = slot_scene.Instantiate<Slot>();
            this.slot_container.AddChild(slot, true);
            slot.Owner = slot_container;
            slot.setIndex(i);
            slot.on_click += handleClick;
            slots.Add(slot);
        }
    }

    public static InventoryGui Load(Inventory inventory) {
        InventoryGui gui = inventory_gui_scene.Instantiate<InventoryGui>();
        Load(gui, inventory, true);
        return gui;
    }

    public static void Load(InventoryGui store, Inventory inventory) {
        Load(store, inventory, false);
    }

    private static void Load(InventoryGui store, Inventory inventory, bool created) {
        if (!created) {
            store.setSize(inventory.items.Count);
            store.setSlots(inventory.items.ToList());
            return;
        }
        store.size = inventory.items.Count;
        store._items = inventory.items.ToList();
    }

    public static Inventory Save(InventoryGui inventory) {
        var list = inventory.slots.Select(data => data?.getStack());
        var array = new Array<ItemStack>(list);
        array.Resize(inventory.size);
        return new Inventory(array);
    }

}
