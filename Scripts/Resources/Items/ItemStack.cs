using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// A handler with some helper functions for items in world
/// Also holds data that is separate between instances of an item
/// </summary>
/// <remarks>
/// Implementation Notes: for custom itemstacks with custom id's add a public static field called custom_id with the item's custom id
/// A empty constructor is required
/// </remarks>
[GlobalClass]
public partial class ItemStack : Resource {

    [Export]
    private Item item;

    public int _count = 1;
    private string _display_name = "";

    [Export(PropertyHint.Range, "1,1,or_greater")]
    public int count {
        get => _count;
        protected set => _count = Math.Min(value, item.max_stack_size);
    }

    [Export]
    public string display_name {
        get => string.IsNullOrEmpty(_display_name) ? item.name : _display_name;
        set => _display_name = value;
    }

    [Signal]
    public delegate void on_changeEventHandler();
    private static Dictionary<string, Type> custom_stacks = new();

    protected ItemStack(Item item) {
        this.item = item;
        this.display_name = item.name;
    }

    public bool isStackable(Item item) {
        if (item == null) {
            return false;
        }
        return item.stackable && this.item.stackable && item.id == this.item.id;
    }

    public int merge(ItemStack stack) {
        if (!isStackable(stack)) {
            return 0;
        }
        int result = this.count + stack.count;
        int remainder = 0;
        if (result > this.item.max_stack_size) {
            remainder = result - this.item.max_stack_size;
        }
        this.count = result - remainder;
        EmitSignal(SignalName.on_change);
        return stack.count = remainder;
    }

    public Item getItem() {
        return this.item;
    }

    public ItemStack split() {
        ItemStack stack = fromItem(this.item, (int) Math.Ceiling(this.count / 2f));
        this.count = (int) Math.Floor(this.count / 2f);
        EmitSignal(SignalName.on_change);
        return stack;
    }

    public ItemStack takeOne() {
        ItemStack stack = fromItem(this.item);
        this.count--;
        EmitSignal(SignalName.on_change);
        return stack;
    }

    public void takeOne(ItemStack stack) {
        if (!this.isStackable(stack) || this.count < 1 || stack.count >= item.max_stack_size) {
            return;
        }
        
        this.count--;
        stack.count++;
        EmitSignal(SignalName.on_change);
    }

    public static ItemStack fromItem(Item item, int count = 1) {
        ItemStack stack;
        if (custom_stacks.ContainsKey(item.id)) {
            stack = (ItemStack) Activator.CreateInstance(custom_stacks[item.id]);
        } else {
            stack = new ItemStack(item);
        }
        stack.count = Math.Min(count, item.max_stack_size);

        return stack;
    }

    /// <summary>
    /// Used to check if the item can be used
    /// </summary>
    /// <returns>True if the item can be used</returns>
    public virtual bool canUse() {
        return false;
    }

    /// <summary>
    /// Run EmitSignal(SignalName.on_change) when the fields are modified
    /// </summary>
    public virtual void use() {}

    private ItemStack() {}

    /// <summary>
    /// Checks the Assembly for any classes that are extending ItemStack and adds them to a Dictionary
    /// * Only ran once at startup
    /// </summary>
    static ItemStack() {
        foreach (Type type in Assembly.GetAssembly(typeof(ItemStack)).GetTypes()) {
            if (!type.IsClass || !type.IsSubclassOf(typeof(ItemStack))) {
                return;
            }
            
            FieldInfo _id = type.GetField("custom_id", BindingFlags.Static | BindingFlags.Public);
            if (_id != null) {
                custom_stacks.Add((string) _id.GetValue(null), type);
            } else {
                custom_stacks.Add(Item.id_base + type.Name.ToSnakeCase(), type);
            }
        }
    }

    public static implicit operator Item(ItemStack stack) {
        return stack.getItem();
    }
}
