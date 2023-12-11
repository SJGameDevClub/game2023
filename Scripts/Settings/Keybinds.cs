// Using InputMap instead (file will be removed on next commit)
// using Godot;
// using System;
// using System.IO;
// using System.Threading.Tasks;

// public partial class Keybinds : Resource {

//     public static Keybinds keybinds = new();
//     public static InputEventKey DEFAULT_MOVE_LEFT {get;} = new() {
//         PhysicalKeycode = Key.A
//     };

//     public static InputEventKey DEFAULT_MOVE_LEFT_ALT {get;} = new() {
//         PhysicalKeycode = Key.Left
//     };

//     public static InputEventKey DEFAULT_MOVE_RIGHT {get;} = new() {
//         PhysicalKeycode = Key.D
//     };

//     public static InputEventKey DEFAULT_MOVE_RIGHT_ALT {get;} = new() {
//         PhysicalKeycode = Key.Right
//     };

//     public static InputEventKey DEFAULT_MOVE_UP {get;} = new() {
//         PhysicalKeycode = Key.W
//     };

//     public static InputEventKey DEFAULT_MOVE_UP_ALT {get;} = new() {
//         PhysicalKeycode = Key.Up
//     };

//     public static InputEventKey DEFAULT_MOVE_DOWN {get;} = new() {
//         PhysicalKeycode = Key.S
//     };

//     public static InputEventKey DEFAULT_MOVE_DOWN_ALT {get;} = new() {
//         PhysicalKeycode = Key.Down
//     };

//     public static InputEventKey DEFAULT_JUMP {get;} = new() {
//         PhysicalKeycode = Key.Space
//     };

//     public static InputEventKey DEFAULT_JUMP_ALT {get;} = new() {
//         PhysicalKeycode = Key.None
//     };

//     public static InputEventKey DEFAULT_ATTACK {get;} = new() {
//         PhysicalKeycode = Key.Enter
//     };

//     public static InputEventKey DEFAULT_ATTACK_ALT {get;} = new() {
//         PhysicalKeycode = Key.None
//     };

//     public static InputEventKey DEFAULT_INTERACT {get;} = new() {
//         PhysicalKeycode = Key.E
//     };

//     public static InputEventKey DEFAULT_INTERACT_ALT {get;} = new() {
//         PhysicalKeycode = Key.None
//     };

//     [Export]
//     public InputEventKey move_up = DEFAULT_MOVE_UP;

//     [Export]
//     public InputEventKey move_up_alt = DEFAULT_MOVE_UP_ALT;

//     [Export]
//     public InputEventKey move_down = DEFAULT_MOVE_DOWN;

//     [Export]
//     public InputEventKey move_down_alt = DEFAULT_MOVE_DOWN_ALT;

//     [Export]
//     public InputEventKey move_left = DEFAULT_MOVE_LEFT;

//     [Export]
//     public InputEventKey move_left_alt = DEFAULT_MOVE_LEFT_ALT;

//     [Export]
//     public InputEventKey move_right = DEFAULT_MOVE_RIGHT;

//     [Export]
//     public InputEventKey move_right_alt = DEFAULT_MOVE_RIGHT_ALT;

//     [Export]
//     public InputEventKey jump = DEFAULT_JUMP;

//     [Export]
//     public InputEventKey jump_alt = DEFAULT_JUMP_ALT;

//     [Export]
//     public InputEventKey attack = DEFAULT_ATTACK;

//     [Export]
//     public InputEventKey attack_alt = DEFAULT_ATTACK_ALT;

//     [Export]
//     public InputEventKey interact = DEFAULT_INTERACT;

//     [Export]
//     public InputEventKey interact_alt = DEFAULT_INTERACT_ALT;

//     public bool isPressed(string key) {
//         try {
//             InputEventKey normal = (InputEventKey) GetType().GetField(key).GetValue(this);
//             InputEventKey alt = (InputEventKey) GetType().GetField(key + "_alt").GetValue(this);
//             return normal.IsPressed() || alt.IsPressed();
//         } catch {
//             GD.PushWarning("Action: ", key, " is not defined");
//             return false;
//         }
//     }

//     static Keybinds() {
//         Task.Run(load);
//     }

//     public static Keybinds load() {
//         Keybinds _keybinds;
//         if (ResourceLoader.Exists("user://settings/keybinds.tres")) {
//             _keybinds = ResourceLoader.Load<Keybinds>("user://settings/keybinds.tres");
//         } else {
//             _keybinds = new Keybinds();
//         }

//         keybinds = _keybinds;
//         return _keybinds;
//     }

//     public static void save(Keybinds keybinds) {
//         string dir = ProjectSettings.GlobalizePath("user://settings");
//         if (!Directory.Exists(dir)) {
//             Directory.CreateDirectory(dir);
//         }
//         // ProjectSettings.Singleton.SetSetting()
//         ResourceSaver.Save(keybinds, "user://settings/keybinds.tres");
//     }
// }
