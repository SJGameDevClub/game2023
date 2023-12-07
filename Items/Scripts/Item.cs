using Godot;
using System;
using System.Diagnostics;

/// <summary>
/// A Wrapper around the resource Item.gd
/// </summary>
public partial class Item : Resource {
    public static string id_base {get;} = "item.";

    [Export]
    private GodotObject item;

    public string name {
        get => item.Get("name").AsString();
        set => item.Set("name", value);
    }

    public string description {
        get => item.Get("description").AsString();
        set => item.Set("description", value);
    }

    protected string _id => item.Get("id").AsString();
    public string id {
        get => string.IsNullOrEmpty(_id) ? id_base + this.name.ToLower().Replace(' ', '_') : _id;
        set => item.Set("id", value);
    }

    public bool custom_id => !string.IsNullOrEmpty(_id);

    public int max_stack_size {
        get => item.Get("max_stack_size").AsInt32();
        set => item.Set("max_stack_size", value);
    }

    public bool stackable => max_stack_size > 1;

    public Texture2D texture {
        get => item.Get("texture").As<Texture2D>();
        set => item.Set("texture", value);
    }

    public Item(GodotObject item) {
        this.item = item;
    }

    private Item() {}

    public static Item createItem(string path) {
        return new(ResourceLoader.Load(path));
    }

}
