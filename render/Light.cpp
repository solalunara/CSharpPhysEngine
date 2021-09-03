#include "Light.h"
#include "Shader.h"

void SetLights( Shader shader, Light *PointLights, int LightLength )
{
	UseShader( shader );

	std::vector<glm::vec4> LightLocations;
	for ( int i = 0; i < LightLength; ++i )
		LightLocations.push_back( PointLights[ i ].Position );

	std::vector<glm::vec4> LightColors;
	for ( int i = 0; i < LightLength; ++i )
		LightColors.push_back( PointLights[ i ].Color );

	std::vector<float> LightIntensities;
	for ( int i = 0; i < LightLength; ++i )
		LightIntensities.push_back( PointLights[ i ].Intensity );

	SetVec4Array( shader, "PointLights", LightLocations );
	SetVec4Array( shader, "LightColors", LightColors );
	SetFloatArray( shader, "LightIntensities", LightIntensities );
}
void SetAmbientLight( Shader shader, float value )
{
	UseShader( shader );

	SetFloat( shader, "AmbientLight", value );
}