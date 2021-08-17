#include "pch.h"
#include "Brush.h"
#include "Texture.h"
#include "World.h"
#include "BaseFace.h"
#include "Transform.h"

Brush::Brush( glm::vec3 mins, glm::vec3 maxs, Texture **textures, GLuint TextureLength, World *world ) :
	BaseEntity( new BaseFace *[ 6 ]{ NULL }, 6, new Transform( glm::vec3( 0 ), glm::vec3( 1 ), glm::mat4( 1 ) ), mins, maxs, world )
{
	_ASSERTE( TextureLength == 1 || TextureLength == 6 );
	bool bSameTexture = TextureLength == 1;
	//have to declare the vertices for each side seperately since they have different UV coords (mins has 0,0 in xz but 1,0 in yz)
	//yz xmins
	float vertices1[] =
	{	//worldspace				//uv
		mins.x, mins.y, mins.z,		0.0f, 0.0f,
		mins.x, mins.y, maxs.z,		1.0f, 0.0f,
		mins.x, maxs.y, maxs.z,		1.0f, 1.0f,
		mins.x, maxs.y, mins.z,		0.0f, 1.0f,
	};
	//yz xmaxs
	float vertices2[] =
	{	//worldspace				//uv
		maxs.x, mins.y, mins.z,		1.0f, 0.0f,
		maxs.x, mins.y, maxs.z,		0.0f, 0.0f,
		maxs.x, maxs.y, maxs.z,		0.0f, 1.0f,
		maxs.x, maxs.y, mins.z,		1.0f, 1.0f,
	};
	//xy zmins
	float vertices3[] =
	{	//worldspace				//uv
		mins.x, mins.y, mins.z,		1.0f, 0.0f,
		mins.x, maxs.y, mins.z,		1.0f, 1.0f,
		maxs.x, maxs.y, mins.z,		0.0f, 1.0f,
		maxs.x, mins.y, mins.z,		0.0f, 0.0f,
	};
	//xy zmaxs
	float vertices4[] =
	{	//worldspace				//uv
		mins.x, mins.y, maxs.z,		0.0f, 0.0f,
		mins.x, maxs.y, maxs.z,		0.0f, 1.0f,
		maxs.x, maxs.y, maxs.z,		1.0f, 1.0f,
		maxs.x, mins.y, maxs.z,		1.0f, 0.0f,
	};
	//xz ymins
	float vertices5[] =
	{	//worldspace				//uv
		mins.x, mins.y, mins.z,		1.0f, 1.0f,
		mins.x, mins.y, maxs.z,		1.0f, 0.0f,
		maxs.x, mins.y, maxs.z,		0.0f, 0.0f,
		maxs.x, mins.y, mins.z,		0.0f, 1.0f,
	};
	//xz ymaxs
	float vertices6[] =
	{	//worldspace				//uv
		mins.x, maxs.y, mins.z,		0.0f, 1.0f,
		mins.x, maxs.y, maxs.z,		0.0f, 0.0f,
		maxs.x, maxs.y, maxs.z,		1.0f, 0.0f,
		maxs.x, maxs.y, mins.z,		1.0f, 1.0f,
	};
	float *vertices[] =
	{
		vertices1,
		vertices2,
		vertices3,
		vertices4,
		vertices5,
		vertices6,
	};
	GLuint indices[] =
	{
		0, 1, 3,
		1, 2, 3
	};
	for ( int i = 0; i < 6; ++i )
	{
		EntFaces[ i ] = new BaseFace( 20, vertices[ i ], sizeof( indices ) / sizeof( float ), indices, bSameTexture?textures[ 0 ]:textures[ i ], GL_DYNAMIC_DRAW );
	}
}
intptr_t InitBrush( glm::vec3 mins, glm::vec3 maxs, intptr_t *textures, unsigned int TextureLength, intptr_t world )
{
	return (intptr_t) new Brush( mins, maxs, (Texture **) textures, TextureLength, (World *) world );
}
void DestructBrush( intptr_t brushptr )
{
	delete (Brush *) brushptr;
}