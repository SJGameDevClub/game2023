using Godot;
using System;

public class Cherry : IStackUser {

    public string[] forIDs() {
        return new[] {Item.id_base + "cherry"};
    }

    public IStackUser.Result use(ItemStack stack) {
        if (PlayerInfo.Health >= 100) {
            return IStackUser.Result.Continue;
        }
        stack.take(1);
        PlayerInfo.Health = Math.Min(100, PlayerInfo.Health + 10);
        return IStackUser.Result.Continue;
    }
}
