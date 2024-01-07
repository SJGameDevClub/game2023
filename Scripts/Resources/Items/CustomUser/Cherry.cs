using Godot;
using System;

public class Cherry : StackUser {

    public override string[] forIDs() {
        return new[] {Item.id_base + "cherry"};
    }

    public override void use(ItemStack stack) {
        if (PlayerInfo.Health >= 100) {
            return;
        }
        stack.take(1);
        PlayerInfo.Health = Math.Min(100, PlayerInfo.Health + 10);
    }
}
