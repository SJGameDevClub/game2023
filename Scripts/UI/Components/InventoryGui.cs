using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An interface for the GUI
/// </summary>
public partial class InventoryGui : PanelContainer {

    protected GridContainer slot_container;
    protected Slot held_item_container;
    protected List<Slot> slots = new();
    protected List<ItemStack> _items;
    protected ItemStack held_item;
    [Export]
    public int size {get; private set;} = 5;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.slot_container = this.GetNode<GridContainer>("%Slots");
        this.held_item_container = this.GetNode<Slot>("%HoverItem");
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
    public override void _PhysicsProcess(double delta) {
        if (this.held_item_container.Visible) {
            this.held_item_container.Position = this.GetGlobalMousePosition() + gi_offset;
        }
    }

    /// <summary>
    /// Creates the Slot objects to be used within
    /// </summary>
    private void Create() {
        foreach (Node node in slot_container.GetChildren()) {
            node.QueueFree();
        }
        slots.Clear();
        PackedScene slotScene = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Slot.tscn");
        for (int i = 0; i < size; ++i) {
            Slot slot = slotScene.Instantiate<Slot>();
            this.slot_container.AddChild(slot);
            slot.Owner = slot_container;
            slot.setIndex(i);
            slot.on_click += handleClick;
            slots.Add(slot);
        }
    }

    protected void handleClick(int index, MouseButton button) {
        Slot slot = this.slots[index];
        switch ((button, this.held_item)) {
            case (MouseButton.Left, null): {
                this.held_item = slot.getStack();
                slot.setStack(null);
                break;
            }
            case (MouseButton.Left, _): {
                if (!slot.isStackable(this.held_item)) {
                    this.held_item = slot.swap(this.held_item);
                    break;
                }
                int old_count = this.held_item.count;
                if (slot.stackItem(this.held_item) == old_count) {
                    this.held_item = slot.swap(this.held_item);
                }
                break;
            }
            case (MouseButton.Right, null): {
                slot.use();
                break;
            }
            case (MouseButton.Right, _): {
                slot.addOneFrom(this.held_item);
                break;
            }
            case (MouseButton.Middle, null): {
                this.held_item = slot.split();
                break;
            }
        }
        if (this.held_item?.count < 1) {
            this.held_item = null;
        }
        redrawHeldItem();
    }

    public void redrawHeldItem() {
        if (this.held_item != null) {
            this.held_item_container.Show();
            this.held_item_container.setStack(this.held_item);
        } else {
            this.held_item_container.Hide();
        }
    }

    /// <summary>
    /// Sets the Item in the indexed slot
    /// </summary>
    /// <param name="index">The slot's index</param>
    /// <param name="stack">The item to set</param>
    public void setSlot(int index, ItemStack stack) {
        slots[index].setStack(stack);
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

        for (int i = 0; i < size; ++i) {
            if (i >= items.Count) {
                break;
            }
            slots[i].setStack(items[i]);
        }
    }

    /// <summary>
    /// Adds to the inventory
    /// </summary>
    /// <returns>True if the item was able to be added</returns>
    public bool addItem(ItemStack stack) {
        foreach (Slot slot in this.slots) {
            slot.stackItem(stack);
        }
        return stack.count > 0;
    }

    /// <summary>
    /// Resizes the Inventory
    /// </summary>
    /// <param name="size">New size of inventory</param>
    /// <returns>The old size of the inventory</returns>
    public int setSize(int size) {
        int old_size = size;
        this.size = size;
        this.Create();
        return old_size;
    }

    public static InventoryGui Load(Inventory inventory) {
        return new() {
            size = inventory.items.Count,
            _items = inventory.items.ToList()
        };
    }

    public static void Load(InventoryGui store, Inventory inventory) {
        store.size = inventory.items.Count;
        store.setSlots(inventory.items.ToList());
    }

    public static void Save(InventoryGui inventory, string path) {
        var list = inventory.slots.Select(data => data?.getStack());
        var array = new Array<ItemStack>(list);
        ResourceSaver.Save(new Inventory(array), path);
    }

}
