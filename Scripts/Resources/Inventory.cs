using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Inventory : Resource {

    [Export]
    public Array<ItemStack> items = new();

    public Inventory(Array<ItemStack> items) {
        this.items = items;
    }

    public static Inventory of(int size) {
        Array<ItemStack> array = new();
        array.Resize(size);

        return new(array);
    }

    /// <summary>
    /// DO NOT USE IN PRODUCTION
    /// currently duplicates the array
    /// </summary>
    /// <param name="size"></param>
    /// <param name="stack"></param>
    /// <returns></returns>
    public static Inventory of(int size, ItemStack stack) {
        Array<ItemStack> array = new();
        array.Resize(size);
        array.Fill(stack);
        array.Duplicate(true);

        return new(array);
    }

    private Inventory() {}
}
