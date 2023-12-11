using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public partial class KeybindSettings : MarginContainer {

    public static PackedScene keybindScene {get;} = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Keybinder.tscn");
    public static InputEventKey empty {get;} = new() {
        PhysicalKeycode = Key.None
    };
    // public static List<FieldInfo> inputs;
    // public static List<FieldInfo> inputs_alt;
    public Keybinder[] keybinders;
    // public Keybinds keybinds;
    public int selected = -1;
    public bool alt = false;
    public List<string> actions;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

        if (ResourceLoader.Exists("user://settings/keybinds.tres")) {
            KeybindResource resource = ResourceLoader.Load<KeybindResource>("user://settings/keybinds.tres");
            foreach (var keybind in resource.vals) {
                InputMap.ActionEraseEvents(keybind.Key);
                if (InputMap.HasAction(keybind.Key + "_temp")) {
                    InputMap.EraseAction(keybind.Key + "_temp");
                }
                foreach (var @event in keybind.Value) {
                    InputMap.ActionAddEvent(keybind.Key, @event);
                }
            }
        }
        // this.keybinds = (Keybinds) Keybinds.keybinds.Duplicate();
        // FieldInfo[] fields = typeof(Keybinds).GetFields(BindingFlags.Instance | BindingFlags.Public);
        // inputs ??= fields.Where(field => field.FieldType == typeof(InputEventKey) && !field.Name.ToLower().EndsWith("_alt")).ToList();
        // inputs_alt ??= fields.Where(field => field.FieldType == typeof(InputEventKey) && field.Name.ToLower().EndsWith("_alt")).ToList();
        // if (inputs.Count != inputs_alt.Count) {
        //     throw new NotSupportedException("Keybinds and alternate keybinds are not equal");
        // }
        Node kbHolder = this.GetNode("KeybindHolder");
        actions = InputMap.GetActions().Select(action => action.ToString()).Where(action => !action.StartsWith("ui_")).ToList();
        this.keybinders = new Keybinder[actions.Count];
        for (int _i = 0; _i < actions.Count; ++_i) {
            int i = _i;
            InputMap.AddAction(actions[i] + "_temp");
            Keybinder keybind = keybindScene.Instantiate<Keybinder>();
            keybind.setIndex(i);
            // keybind.setKeybindName(string.Join(" ", inputs[i].Name.Split("_").Select(str => str.ToPascalCase())));
            keybind.setKeybindName(string.Join(" ", actions[i].Split("_").Select(str => str.ToPascalCase())));
            kbHolder.AddChild(keybind);
            keybind.Owner = kbHolder;
            keybind.on_click += onClick;
            keybind.on_reset += () => {
                // InputEventKey _normal = (InputEventKey) typeof(Keybinds).GetProperty("DEFAULT_" + inputs[i].Name.ToUpper(), BindingFlags.Public | BindingFlags.Static).GetValue(null);
                // InputEventKey _alt = (InputEventKey) typeof(Keybinds).GetProperty("DEFAULT_" + inputs_alt[i].Name.ToUpper()).GetValue(null);
                var events = InputMap.ActionGetEvents(actions[i]);
                keybind.setText(((InputEventKey) events[0]).AsTextPhysicalKeycode(), false);
                try {
                    keybind.setText(( (InputEventKey) events[1] ).AsTextPhysicalKeycode(), true);
                } catch {
                    keybind.setText(empty.AsTextPhysicalKeycode(), true);
                }
                // keybind.setText(_normal.AsTextPhysicalKeycode(), false);
                // keybind.setText(_alt.AsTextPhysicalKeycode(), true);
                InputMap.ActionEraseEvents(actions[i] + "_temp");
                int j = 0;
                foreach (var @event in InputMap.ActionGetEvents(actions[i])) {
                    InputMap.ActionAddEvent(actions[i] + "_temp", @event);
                    ++j;
                }
                if (j < 2) {
                    InputMap.ActionAddEvent(actions[i] + "_temp", (InputEvent) empty.Duplicate());
                }
                // inputs[i].SetValue(this.keybinds, _normal);
                // inputs_alt[i].SetValue(this.keybinds, _alt);
            };
            keybind.on_clear += () => {
                keybind.setText("(Unset)", false);
                keybind.setText("(Unset)", true);
            };
            this.keybinders[i] = keybind;
            int m = 0;
            foreach (var @event in InputMap.ActionGetEvents(actions[i])) {
                InputMap.ActionAddEvent(actions[i] + "_temp", @event);
                ++m;
            }

            if (m < 2) {
                InputMap.ActionAddEvent(actions[i], (InputEvent) empty.Duplicate());
            }
            keybind.setText(((InputEventKey) InputMap.ActionGetEvents(this.actions[i])[0]).AsTextPhysicalKeycode(), false);
            InputEventKey key;
            try {
                key = (InputEventKey) InputMap.ActionGetEvents(this.actions[i])[1];
            } catch {
                key = empty;
            }
            keybind.setText(key.AsTextPhysicalKeycode(), true);
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is not InputEventKey key || this.selected == -1) {
            return;
        }

        this.GetViewport().SetInputAsHandled();
        if (key.PhysicalKeycode == Key.Escape) {
            key = empty;
        }

        key.Pressed = false;
        var action = actions[this.selected];
        var events = InputMap.ActionGetEvents(action + "_temp");
        InputMap.ActionEraseEvents(action + "_temp");
        if (this.alt) {
            InputMap.ActionAddEvent(action + "_temp", events[0]);
            InputMap.ActionAddEvent(action + "_temp", key);
        } else {
            InputMap.ActionAddEvent(action + "_temp", key);
            InputMap.ActionAddEvent(action + "_temp", events[1]);
        }
    //     (this.alt ? inputs_alt : inputs)[this.selected].SetValue(this.keybinds, key.Duplicate());
        this.reset();
    }

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is not InputEventMouseButton mouse) {
            return;
        }

        if (mouse.IsPressed() && mouse.ButtonIndex == MouseButton.Left) {
            this.reset();
        }
    }

    public void reset() {
        if (this.selected == -1) {
            return;
        }
        this.keybinders[this.selected].setText(((InputEventKey) InputMap.ActionGetEvents(actions[this.selected] + "_temp")[this.alt ? 1 : 0]).AsTextPhysicalKeycode(), this.alt);
        // this.keybinders[this.selected].setText(getKey(this.keybinds, this.selected, this.alt).AsTextPhysicalKeycode(), this.alt);
        this.selected = -1;
        this.alt = false;
    }

    public void onClick(int index, bool alt) {
        this.reset();
        this.selected = index;
        this.keybinders[index].setText("Press a Key", alt);
        this.alt = alt;
    }

    public void onCancel(Callable _delay) {
        // this.keybinds = (Keybinds) Keybinds.keybinds.Duplicate();
        for (int i = 0; i < actions.Count; ++i) {
            InputMap.ActionEraseEvents(actions[i] + "_temp");
            var events = InputMap.ActionGetEvents(actions[i]);
            InputMap.ActionAddEvent(actions[i] + "_temp", events[0]);
            InputMap.ActionAddEvent(actions[i] + "_temp", events[1]);
            this.keybinders[i].setText(( (InputEventKey) events[0] ).AsTextPhysicalKeycode(), false);
            this.keybinders[i].setText(( (InputEventKey) events[1] ).AsTextPhysicalKeycode(), true);
        }
    }

    public void onSave(Callable _delay) {
        KeybindResource resource = new();
        foreach (string action in actions) {
            resource.vals[action] = InputMap.ActionGetEvents(action + "_temp");
        }
        string dir = ProjectSettings.GlobalizePath("user://settings");
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        ResourceSaver.Save(resource, "user://settings/keybinds.tres");
        // Keybinds.save(this.keybinds);
        // Keybinds.keybinds = (Keybinds) this.keybinds.Duplicate();
    }

    // public static InputEventKey getKey(int index, bool alt) {
    //     return getKey(Keybinds.keybinds, index, alt);
    // }

    // public static InputEventKey getKey(Keybinds keybinds, int index, bool alt) {
    //     return (InputEventKey) ( alt ? inputs_alt : inputs )[index].GetValue(keybinds);
    // }

}
