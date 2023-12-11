using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Inventory : Resource {

    [Export]
    public Array<ItemStack> items = new();

    public Inventory(Array<ItemStack> items) {
        this.items = items;
    }

    private Inventory() {}
}
