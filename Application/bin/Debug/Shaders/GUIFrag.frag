#version 330 core

out vec4 FragColor;

const int MAX_POINT_LIGHTS = 100;

uniform sampler2D TextureToRender;
uniform float AmbientLight;
uniform vec4 PointLights[ MAX_POINT_LIGHTS ];
uniform vec4 LightColors[ MAX_POINT_LIGHTS ];
uniform float LightIntensities[ MAX_POINT_LIGHTS ];

void main()
{
	FragColor = vec4( 1.0f, 1.0f, 1.0f, 1.0f );
}