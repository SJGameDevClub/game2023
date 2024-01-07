using Godot;
using System;

public abstract class StackUser {

    public abstract string[] forIDs();

    public abstract void use(ItemStack stack);

}
