using Godot;
using Godot.Collections;
using System;

public partial class InventoryResource : Resource {

    [Export]
    public Array<ItemStack> items;

    [Export]
    public int size;

    public InventoryResource(Array<ItemStack> items, int size) {
        this.items = items;
        this.size = size;
    }

    private InventoryResource() {}
}
