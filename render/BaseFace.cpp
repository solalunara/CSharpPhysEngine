#include "pch.h"
#include "BaseFace.h"
#include "Texture.h"

#include <vector>


BaseFace::BaseFace( int VertLength, float *vertices, int IndLength, int *indices, Texture texture, glm::vec3 vNormal, GLenum DrawType ) :
	VertLength( VertLength ), IndLength( IndLength ), texture( texture ), vNormal( vNormal ),
	VBO( 0 ), VAO( 0 ), EBO( 0 )
{
	for ( int i = 0; i < VertLength; ++i )
		this->vertices[ i ] = vertices[ i ];
	for ( int i = 0; i < IndLength; ++i )
		this->indices[ i ] = indices[ i ];

	glGenBuffers( 1, &EBO );
	glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, EBO );
	glBufferData( GL_ELEMENT_ARRAY_BUFFER, sizeof( int ) * IndLength, indices, DrawType );

	glGenVertexArrays( 1, &VAO );
	glBindVertexArray( VAO );

	//generate a buffer object in the gpu and store vertex data in it
	glGenBuffers( 1, &VBO );
	glBindBuffer( GL_ARRAY_BUFFER, VBO );
	glBufferData( GL_ARRAY_BUFFER, sizeof( float ) * VertLength, vertices, DrawType );

	//world space vertex data
	glVertexAttribPointer( 0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof( float ), (void *) 0 );
	glEnableVertexAttribArray( 0 );
	//UV map vertex data
	glVertexAttribPointer( 1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof( float ), (void *) ( 3 * sizeof( float ) ) );
	glEnableVertexAttribArray( 1 );
}
BaseFace::BaseFace() :
	VertLength( 0 ), IndLength( 0 ), VBO( 0 ), VAO( 0 ), EBO( 0 ), texture(), vNormal(), vertices(), indices()
{
}

void DestructBaseFace( BaseFace *face )
{
	glDeleteBuffers( 1, &face->VBO );
	glDeleteVertexArrays( 1, &face->VAO );
	glDeleteBuffers( 1, &face->EBO );
}

void InitBaseFace( int Vertlength, float *vertices, int IndLength, int *indices, Texture texture, glm::vec3 vNormal, BaseFace *pFace )
{
	_ASSERTE( pFace );
	*pFace = BaseFace( Vertlength, vertices, IndLength, indices, texture, vNormal, GL_DYNAMIC_DRAW );
}

float GetVertAtIndex( BaseFace face, int index )
{
	return face.vertices[ index ];
}
int GetIndAtIndex( BaseFace face, int index )
{
	return face.indices[ index ];
}