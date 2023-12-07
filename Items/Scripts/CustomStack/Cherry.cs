using Godot;
using System;

public partial class Cherry : ItemStack {

    public Cherry() : base(Items.Cherry) {}

    public override bool canUse() {
        return true;
    }

    public override void use() {
        if (PlayerInfo.Health >= 100) {
            return;
        }
        this.count--;
        this.EmitSignal(SignalName.on_change);
        PlayerInfo.Health = Math.Min(100, PlayerInfo.Health + 10);
    }
}
