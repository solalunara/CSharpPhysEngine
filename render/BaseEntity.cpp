#include "pch.h"
#include "BaseEntity.h"


//baseentity functions
BaseEntity::BaseEntity( BaseFace *EntFaces, int FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs ) :
    AABB( BoundingBox( mins, maxs ) ), FaceLength( FaceLength ), transform( transform ), EntFaces()
{
    for ( int i = 0; i < FaceLength; ++i )
        this->EntFaces[ i ] = EntFaces[ i ];
}
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
void InitBaseEntity( BaseFace *EntFaces, int FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs, BaseEntity *pEnt )
{
    _ASSERTE( pEnt );
    *pEnt = BaseEntity( EntFaces, FaceLength, transform, mins, maxs );
}
void InitBrush( glm::vec3 mins, glm::vec3 maxs, Texture *textures, int TextureLength, BaseEntity *pEnt )
{
    _ASSERTE( pEnt );
    *pEnt = BaseEntity( mins, maxs, textures, TextureLength );
}

void GetBaseFaceAtIndex( BaseEntity ent, BaseFace *pFace, int index )
{
    _ASSERTE( pFace );
    *pFace = ent.EntFaces[ index ];
}

void DestructBaseEntity( BaseEntity *ent )
{
    for ( int i = 0; i < ent->FaceLength; ++i )
        DestructBaseFace( &ent->EntFaces[ i ] );
}

void MakePerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat )
{
    _ASSERTE( pMat );
    *pMat = glm::mat4( glm::perspective( glm::radians( fov ), aspect, nearclip, farclip ) );
}
void MultiplyMatrix( glm::mat4 *pMultiply, glm::mat4 multiplier )
{
    _ASSERTE( pMultiply );
    *pMultiply *= multiplier;
}