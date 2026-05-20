shader_type canvas_item;

uniform vec2 scroll_speed = vec2(0.0,0.2);

void fragment() {
	vec2 uv = UV  + TIME * scroll_speed;
	COLOR = texture(TEXTURE,uv);
}
