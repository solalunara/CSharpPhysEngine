#version 330 core

in vec2 TexCoord;
in vec4 WorldPos;

out vec4 FragColor;

const int MAX_POINT_LIGHTS = 100;

uniform sampler2D TextureToRender;
uniform float AmbientLight;
uniform vec4 PointLights[ MAX_POINT_LIGHTS ];
uniform vec4 LightColors[ MAX_POINT_LIGHTS ];
uniform float LightIntensities[ MAX_POINT_LIGHTS ];

void main()
{
	vec4 TexColor = texture( TextureToRender, TexCoord );
	vec4 LitTexture = TexColor * AmbientLight;
	for ( int i = 0; i < MAX_POINT_LIGHTS; ++i )
	{
		float LightDist = length( WorldPos - PointLights[i] ) + 1;
		LitTexture += LightColors[i] * TexColor * LightIntensities[i] / ( LightDist * LightDist );
	}
	FragColor = vec4( LitTexture.rgb, TexColor.a );
}