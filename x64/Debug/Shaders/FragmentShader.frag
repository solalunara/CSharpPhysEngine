#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
uniform vec4 SetColor;
uniform sampler2D TextureToRender;
void main()
{
	FragColor = texture( TextureToRender, TexCoord );
}