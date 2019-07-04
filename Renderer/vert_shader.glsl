#version 440 core
layout(location = 0) in vec2 vert_coord;
layout(location = 1) in vec2 tex_coord;

out vec2 texture_coord;

void main()
{
	gl_Position = vec4(vert_coord, 0, 1);

	texture_coord = tex_coord;
}