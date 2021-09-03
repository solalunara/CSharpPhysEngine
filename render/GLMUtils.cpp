#include "pch.h"
#include "GLMUtils.h"

/*
BaseEntity::BaseEntity( glm::vec3 mins, glm::vec3 maxs, Texture *textures, int TextureLength ) :
    AABB( BoundingBox( mins, maxs ) ), FaceLength( 6 ), transform( Transform( glm::vec3( 0 ), glm::vec3( 1 ), glm::mat4( 1 ) ) ), EntFaces()
{
    _ASSERTE( TextureLength == 1 || TextureLength == 6 );
    bool bSameTexture = TextureLength == 1;

    glm::vec3 ptCenter = ( mins + maxs ) / 2.0f;
    transform.m_Position = ptCenter;
    UpdateTransform( &transform );

    mins -= ptCenter;
    maxs -= ptCenter;

    AABB = BoundingBox( mins, maxs );

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
    int indices[] =
    {
        0, 1, 3,
        1, 2, 3
    };

    glm::vec3 normals[] =
    {
        glm::vec3(-1, 0, 0 ),
        glm::vec3( 1, 0, 0 ),
        glm::vec3( 0, 0,-1 ),
        glm::vec3( 0, 0, 1 ),
        glm::vec3( 0,-1, 0 ),
        glm::vec3( 0, 1, 0 )
    };

    for ( int i = 0; i < FaceLength; ++i )
        EntFaces[ i ] = BaseFace( 20, vertices[ i ], 6, indices, bSameTexture?textures[ 0 ]:textures[ i ], normals[i], GL_DYNAMIC_DRAW );
}
*/

void GLMPerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat )
{
    _ASSERTE( pMat );
    *pMat = glm::mat4( glm::perspective( glm::radians( fov ), aspect, nearclip, farclip ) );
}
void GLMRotMatrix( float degrees, glm::vec3 axis, glm::mat4 *pMat )
{
    _ASSERTE( pMat );
    *pMat = glm::rotate( glm::mat4( 1 ), glm::radians( degrees ), axis );
}
void GLMMultiplyMatrix( glm::mat4 *pMultiply, glm::mat4 multiplier )
{
    _ASSERTE( pMultiply );
    *pMultiply = multiplier * *pMultiply;
}
void GLMMultMatrixVector( glm::mat4 matrix, glm::vec4 *vector )
{
    _ASSERTE( vector );
    *vector = matrix * *vector;
}
void GLMInvertMatrix( glm::mat4 *matrix )
{
    _ASSERTE( matrix );
    *matrix = glm::inverse( *matrix );
}

void GLMScale( glm::vec3 scale, glm::mat4 *m )
{
    _ASSERTE( m );
    *m = glm::scale( glm::mat4( 1 ), scale );
}
void GLMTranslate( glm::vec3 pt, glm::mat4 *m )
{
    _ASSERTE( m );
    *m = glm::translate( glm::mat4( 1 ), pt );
}