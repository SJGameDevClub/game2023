using Godot;

public class Cloud : IStackUser {
    public string[] forIDs() {
        return new[] {Item.id_base + "cloud"};
    }

    public IStackUser.Result use(ItemStack stack) {
        if (stack.custom_data.ContainsKey("inv_open") && (bool) stack.custom_data["inv_open"]) {
            return IStackUser.Result.Continue;
        }
        if (!stack.custom_data.ContainsKey("inventory")) {
            stack.custom_data["inventory"] = Inventory.of(3); // Inventory.of(3, ItemStack.of(Items.Cherry, 15));
        }
        Inventory inventory = (Inventory) stack.custom_data["inventory"];
        if (inventory.items.Count != 3) {
            inventory.items.Resize(3);
        }
        InventoryGui gui = InventoryGui.Load(inventory);
        
        gui.GlobalPosition = HUD.hud.GetTree().Root.Size / 2;
        gui.use_header = true;
        gui.title = stack.display_name + " Inventory";
        gui.is_closable = true;
        gui.on_close += inventory => {
            stack.custom_data["inventory"] = inventory;
            stack.custom_data["inv_open"] = false;
        };
        stack.custom_data["inv_open"] = true;

        HUD.hud.AddChild(gui);
        gui.Owner = HUD.hud;

        return IStackUser.Result.Continue;
    }
}
