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

    private int _count = 1;
    private string _display_name = "";

    [Export(PropertyHint.Range, "1,1,or_greater")]
    public int count {
        get => _count;
        protected set => this.setCount(value);
    }

    [Export]
    public string display_name {
        get => string.IsNullOrEmpty(_display_name) ? item.name : _display_name;
        set => _display_name = value;
    }

    [Signal]
    public delegate void on_changeEventHandler();
    private static Dictionary<string, StackUser> stack_users = new();

    public ItemStack(Item item) {
        this.item = item;
        this.display_name = item.name;
    }

    protected void setCount(int count) {
        _count = Math.Min(count, item.max_stack_size);
        EmitSignal(SignalName.on_change);
    }

    public bool isStackable(ItemStack stack) {
        if (stack == null) {
            return false;
        }
        return stack.item.stackable && this.item.stackable && stack.item.id == this.item.id && stack.display_name == this.display_name;
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
        return stack.count = remainder;
    }

    public Item getItem() {
        return this.item;
    }

    public ItemStack split() {
        ItemStack stack = fromItem(this.item, (int) Math.Ceiling(this.count / 2f));
        this.count = (int) Math.Floor(this.count / 2f);
        return stack;
    }

    public ItemStack take(int amount) {
        ItemStack stack = fromItem(this.item, amount > this.count ? amount + (this.count - amount) : amount);
        this.count -= amount;
        return stack;
    }

    public void takeOne(ItemStack stack) {
        if (!this.isStackable(stack) || this.count < 1 || stack.count >= item.max_stack_size) {
            return;
        }
        
        this.count--;
        stack.count++;
    }

    public static ItemStack fromItem(Item item, int count = 1) {
        return new ItemStack(item) {
            count = Math.Min(count, item.max_stack_size)
        };
    }

    /// <summary>
    /// Used to check if the item can be used
    /// </summary>
    /// <returns>True if the item can be used</returns>
    public bool canUse() {
        return stack_users.ContainsKey(this.item.id);
    }

    /// <summary>
    /// Run EmitSignal(SignalName.on_change) when the fields are modified
    /// </summary>
    public void use() {
        if (this.canUse()) {
            stack_users[this.item.id].use(this);
        }
    }

    private ItemStack() {}

    /// <summary>
    /// Checks the Assembly for any classes that are extending ItemStack and adds them to a Dictionary
    /// * Only ran once at startup
    /// </summary>
    static ItemStack() {
        foreach (Type type in Assembly.GetAssembly(typeof(StackUser)).GetTypes()) {
            if (!type.IsClass || !type.IsSubclassOf(typeof(StackUser)) || type.IsAbstract) {
                continue;
            }
            
            StackUser instance = (StackUser) Activator.CreateInstance(type);
            string[] ids = instance.forIDs();
            for (int i = 0; i < ids.Length; i++) {
                stack_users.Add(ids[i], instance);
            }
        }
    }

    public static implicit operator Item(ItemStack stack) {
        return stack.getItem();
    }
}
