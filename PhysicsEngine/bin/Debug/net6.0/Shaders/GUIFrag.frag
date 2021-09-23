#version 330 core

out vec4 FragColor;

in vec2 TexCoord;

const int MAX_POINT_LIGHTS = 100;

uniform sampler2D TextureToRender;
uniform float AmbientLight;
uniform vec4 PointLights[ MAX_POINT_LIGHTS ];
uniform vec4 LightColors[ MAX_POINT_LIGHTS ];
uniform float LightIntensities[ MAX_POINT_LIGHTS ];

void main()
{
	vec4 TexColor = texture( TextureToRender, TexCoord );
	if ( TexColor.a < 0.1 )
		discard;
	FragColor = TexColor;
}