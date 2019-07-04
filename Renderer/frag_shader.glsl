#version 440 core

out vec4 color;

in vec2 texture_coord; 

uniform sampler2D wall_textures;
uniform sampler2DArray billboard_textures;

uniform vec2 screen_resolution;

uniform vec3 top_color;
uniform vec3 bottom_color;

struct Draw_data
{
	int texture_id;
	float scale;
	float texture_U;
};

struct Draw_billboard_data
{
	int texture_id;
    float scale;
    int left_visible;
    int right_visible;
};

layout(std430, binding = 0) buffer DRAWDATAPERCOLUMN
{
	Draw_data[] draw_data_per_column; 
};

uniform int draw_billboard_data_length;

layout(std430, binding = 1) buffer DRAWBILLBOARDDATA
{
	Draw_billboard_data[] draw_billboard_data; 
};

void main()
{
	// Walls.
	Draw_data curr_draw_data = draw_data_per_column[int(texture_coord.x * screen_resolution.x)];

	float scaled_y = (texture_coord.y - 0.5) / curr_draw_data.scale + 0.5;

	color = texture2D(wall_textures, vec2(curr_draw_data.texture_U, scaled_y));

	if(scaled_y > 1)
		color = vec4(top_color, 1);

	if(scaled_y < 0)
		color = vec4(bottom_color, 1);

	// Billboards.
	for(int i = 0; i < draw_billboard_data_length; i++)
	{
		if(draw_billboard_data[i].scale < curr_draw_data.scale)
			 continue;

		if(draw_billboard_data[i].left_visible > texture_coord.x * screen_resolution.x || draw_billboard_data[i].right_visible < texture_coord.x * screen_resolution.x)
			continue;

		scaled_y = (texture_coord.y - 0.5) / draw_billboard_data[i].scale + 0.5;

		if(scaled_y > 1 || scaled_y < 0)
			continue;

		vec4 new_color = texture2DArray(billboard_textures, 
		vec3(	(texture_coord.x * screen_resolution.x - draw_billboard_data[i].left_visible) / (draw_billboard_data[i].right_visible - draw_billboard_data[i].left_visible), 
				scaled_y, 
				draw_billboard_data[i].texture_id));

		if(new_color.a != 0)
			color = new_color;
	}
}