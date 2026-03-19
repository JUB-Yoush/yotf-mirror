@tool
extends EditorScript

func _run() -> void:
	var terrain: Terrain3D = get_scene().find_children("*", "Terrain3D", true, false)[0]
	
	# Load your existing NoiseTexture2D
	var noise_texture: NoiseTexture2D = load("res://terrain/noise-texture.tres")
	
	# NoiseTexture2D generates asynchronously, so force it to finish
	await noise_texture.changed
	var img: Image = noise_texture.get_image()
	
	var region_size: int = 1024
	var height_scale: float = 100.0
	
	# Resize to match region size if needed
	if img.get_width() != region_size or img.get_height() != region_size:
		img.resize(region_size, region_size, Image.INTERPOLATE_LANCZOS)
	
	# Remap pixel values to height
	var height_img := Image.create(region_size, region_size, false, Image.FORMAT_RF)
	for y in region_size:
		for x in region_size:
			var h: float = img.get_pixel(x, y).r * height_scale
			height_img.set_pixel(x, y, Color(h, 0, 0, 1))
	
	var region := Terrain3DRegion.new()
	region.height_range = Vector2(0.0, height_scale)
	region.set_height_map(height_img)
	terrain.data.set_region(Vector2i(0, 0), region)
	terrain.data.save()
