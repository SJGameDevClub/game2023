using Godot;
using System;

public partial class InventoryManager : Node {

    public Godot.Collections.Array<Slot> playerInventory = new Godot.Collections.Array<Slot>();

    public override void _PhysicsProcess(double delta) {
        if (Input.IsActionJustReleased("inventory")) {
            
        }
    }
}
