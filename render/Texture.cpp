#include "pch.h"
#include "Texture.h"

#include "CRTDBG.h"

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

Texture::Texture( const char *FilePath, GLenum Unit, GLenum WrapStyle, GLenum FilterStyleMin, GLenum FilterStyleMag, float *BorderColor ) :
	Unit( Unit )
{
	stbi_set_flip_vertically_on_load( true );

	if ( WrapStyle != 0 )
	{
		glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, WrapStyle );
		glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, WrapStyle );
		if ( WrapStyle == GL_CLAMP_TO_BORDER )
		{
			_ASSERTE( BorderColor );
			glTexParameterfv( GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, BorderColor );
		}
	}

	if ( FilterStyleMin != 0 )
		glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, FilterStyleMin );
	if ( FilterStyleMag != 0 )
		glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, FilterStyleMag );

	int width, height, nrChannels;
	unsigned char *data = stbi_load( FilePath, &width, &height, &nrChannels, 0 );

	_ASSERTE( data );

	ID = 0;
	glGenTextures( 1, &ID );
	glBindTexture( GL_TEXTURE_2D, ID );
	glTexImage2D( GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data );
	glGenerateMipmap( GL_TEXTURE_2D );
	stbi_image_free( data );
	glBindTexture( GL_TEXTURE_2D, 0 );

}
Texture::Texture() :
	ID( 0 ), Unit( GL_TEXTURE0 )
{
}
void InitTexture( const char *FilePath, Texture *pTex )
{
	_ASSERTE( pTex );
	*pTex = Texture( FilePath );
}
void DestructTexture( Texture *pTex )
{
	_ASSERTE( pTex );
	glDeleteTextures( 1, &pTex->ID );
}