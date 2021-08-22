#include "pch.h"
#include "BaseEntity.h"

#include <algorithm>
#include <iterator>
#include <vector>


//baseentity functions
BaseEntity::BaseEntity( BaseFace *EntFaces, GLuint FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs ) :
    AABB( BoundingBox( mins, maxs ) ), EntFaces( EntFaces ), FaceLength( FaceLength ), transform( transform )
{
}
BaseEntity::BaseEntity():
    EntFaces( NULL ), FaceLength( 0 )
{
}
BaseEntity::BaseEntity( glm::vec3 mins, glm::vec3 maxs, Texture *textures, GLuint TextureLength ) :
    AABB( BoundingBox( mins, maxs ) ), FaceLength( 6 ), transform( Transform( glm::vec3( 0 ), glm::vec3( 1 ), glm::mat4( 1 ) ) )
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

    EntFaces = new BaseFace[ 6 ]
    {
        BaseFace( 20, vertices[ 0 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 0 ], GL_DYNAMIC_DRAW ),
        BaseFace( 20, vertices[ 1 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 1 ], GL_DYNAMIC_DRAW ),
        BaseFace( 20, vertices[ 2 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 2 ], GL_DYNAMIC_DRAW ),
        BaseFace( 20, vertices[ 3 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 3 ], GL_DYNAMIC_DRAW ),
        BaseFace( 20, vertices[ 4 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 4 ], GL_DYNAMIC_DRAW ),
        BaseFace( 20, vertices[ 5 ], 6, indices, bSameTexture?textures[ 0 ]:textures[ 5 ], GL_DYNAMIC_DRAW ),
    };
}
void InitBaseEntity( BaseFace *EntFaces, unsigned int FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs, BaseEntity *pEnt )
{
    if ( !pEnt )
        pEnt = new BaseEntity();
    *pEnt = BaseEntity( EntFaces, FaceLength, transform, mins, maxs );
}
void InitBrush( glm::vec3 mins, glm::vec3 maxs, Texture *textures, unsigned int TextureLength, BaseEntity *pEnt )
{
    if ( !pEnt )
        pEnt = new BaseEntity();
    *pEnt = BaseEntity( mins, maxs, textures, TextureLength );
}

//camear functions
Camera::Camera( Transform transform, glm::mat4 perspective ) :
    LinkedEnt( BaseEntity( NULL, 0, transform, glm::vec3( -.5f, -1.5f, -.5f ), glm::vec3( .5f, .5f, .5f ) ) ),
    m_Perspective( perspective )
{
};
Camera::Camera() :
    m_Perspective( glm::mat4( 1 ) )
{
}
void MakePerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat )
{
    if ( !pMat )
        pMat = new glm::mat4();
    *pMat = glm::mat4( glm::perspective( fov, aspect, nearclip, farclip ) );
}
void InitCamera( Transform transform, glm::mat4 perspective, Camera *pCam )
{
    if ( !pCam )
        pCam = new Camera();
    *pCam = Camera( transform, perspective );
}