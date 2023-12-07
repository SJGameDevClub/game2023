extends Resource
class_name Item

const BASE_MAX_STACK_SIZE: int = 15;
@export_group("Identifiers")
@export var name: String = "unknown";
@export_multiline var description: String = "unknown";
@export var id: String = "";
@export_group("Other")
@export var max_stack_size: int = BASE_MAX_STACK_SIZE;
@export var texture: Texture2D = preload("res://Assets/missing.png");

