#include "pch.h"
#include "BaseFace.h"
#include "Texture.h"

#include <vector>


BaseFace::BaseFace( GLuint VertLength, float *vertices, GLuint IndLength, GLuint *indices, Texture texture, GLenum DrawType ) :
	vertices( vertices ), VertLength( VertLength ), indices( indices ), IndLength( IndLength ), texture( texture ),
	VBO( 0 ), VAO( 0 ), EBO( 0 )
{
	glGenBuffers( 1, &EBO );
	glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, EBO );
	glBufferData( GL_ELEMENT_ARRAY_BUFFER, sizeof( float ) * IndLength, indices, DrawType );

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

	glBindBuffer( GL_ARRAY_BUFFER, 0 );
	glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, 0 );
}

void InitBaseFace( unsigned int Vertlength, float *vertices, unsigned int IndLength, unsigned int *indices, Texture texture, BaseFace *pFace )
{
	if ( !pFace )
		pFace = new BaseFace( Vertlength, vertices, IndLength, indices, texture, GL_DYNAMIC_DRAW );
	else
		*pFace = BaseFace( Vertlength, vertices, IndLength, indices, texture, GL_DYNAMIC_DRAW );
}