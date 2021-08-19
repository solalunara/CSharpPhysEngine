#include "pch.h"
#include "BaseEntity.h"
#include "BaseFace.h"
#include "World.h"
#include "Transform.h"

#include <algorithm>
#include <iterator>
#include <vector>


//plane functions
intptr_t InitPlane( glm::vec3 vNormal, float fDist )
{
    return (intptr_t) new Plane( vNormal, fDist );
}
void GetPlaneVals( intptr_t p, glm::vec3 *norm, float *dist )
{
    if ( !norm )
        norm = new glm::vec3();
    if ( !dist )
        dist = new float();
    *norm = ((Plane *) p)->vNormal;
    *dist = ((Plane *) p)->fDist;
}
float DistanceFromPointToPlane( intptr_t p, glm::vec3 pt )
{
    _ASSERTE( (Plane *) p );
	return glm::dot( ( (Plane *) p)->vNormal, pt ) - ( (Plane *) p)->fDist;
}
void ClosestPointOnPlane( intptr_t p, glm::vec3 pt, glm::vec3 *out )
{
    _ASSERTE( (Plane *) p );
    //this does NOT cause a memory leak, since Vector is a marshalled type on client side
    if ( !out )
        out = new glm::vec3();
	*out = pt - DistanceFromPointToPlane( p, pt ) * ( (Plane *) p)->vNormal;
}
void DestructPlane( intptr_t ptr )
{
    _ASSERTE( (Plane *) ptr );
    delete (Plane *) ptr;
}

//AABB functions
intptr_t InitAABB( glm::vec3 mins, glm::vec3 maxs )
{
    return (intptr_t) new BoundingBox( mins, maxs );
}
void GetAABBPoints( intptr_t BBox, glm::vec3 *mins, glm::vec3 *maxs )
{
    if ( !mins )
        mins = new glm::vec3();
    if ( !maxs )
        maxs = new glm::vec3();
    *mins = ((BoundingBox *) BBox)->mins;
    *maxs = ((BoundingBox *) BBox)->maxs;
}
bool TestCollisionAABB( intptr_t b1, intptr_t b2, glm::vec3 ptB1, glm::vec3 ptB2 )
{
    BoundingBox *bbox1 = (BoundingBox *) b1;
    BoundingBox *bbox2 = (BoundingBox *) b2;
    _ASSERTE( bbox1 && bbox2 );
    glm::vec3 ptWorldMins1 = bbox1->mins + ptB1;
    glm::vec3 ptWorldMaxs1 = bbox1->maxs + ptB1;
    glm::vec3 ptWorldMins2 = bbox2->mins + ptB2;
    glm::vec3 ptWorldMaxs2 = bbox2->maxs + ptB2;
    bool bCollisionX = ptWorldMins1.x <= ptWorldMaxs2.x && ptWorldMaxs1.x >= ptWorldMins2.x;
    bool bCollisionY = ptWorldMins1.y <= ptWorldMaxs2.y && ptWorldMaxs1.y >= ptWorldMins2.y;
    bool bCollisionZ = ptWorldMins1.z <= ptWorldMaxs2.z && ptWorldMaxs1.z >= ptWorldMins2.z;
    return bCollisionX && bCollisionY && bCollisionZ;
}
bool TestCollisionPoint( glm::vec3 pt, intptr_t b, glm::vec3 ptB )
{
    bool bShouldCollide = false;
    BoundingBox *bbox = (BoundingBox *) b;
    _ASSERTE( bbox );
    for ( int i = 0; i < 3; ++i )
        if ( pt[ i ] > bbox->mins[ i ] + ptB[ i ] && pt[ i ] < bbox->maxs[ i ] + ptB[ i ] )
            bShouldCollide = true;
    return bShouldCollide;
}
intptr_t GetCollisionPlane( glm::vec3 pt, intptr_t b, glm::vec3 ptB )
{
    _ASSERTE( ( (BoundingBox *) b ) );
    glm::vec3 ptWorldMins = ( (BoundingBox *) b)->mins + ptB;
    glm::vec3 ptWorldMaxs = ( (BoundingBox *) b)->maxs + ptB;

    Plane planes[] =
    {
        Plane( glm::vec3( 0, 0, 1 ), glm::dot( glm::vec3( 0, 0, 1 ), ptWorldMaxs ) ),
        Plane( glm::vec3( 0, 0,-1 ), glm::dot( glm::vec3( 0, 0,-1 ), ptWorldMins ) ),
        Plane( glm::vec3( 0, 1, 0 ), glm::dot( glm::vec3( 0, 1, 0 ), ptWorldMaxs ) ),
        Plane( glm::vec3( 0,-1, 0 ), glm::dot( glm::vec3( 0,-1, 0 ), ptWorldMins ) ),
        Plane( glm::vec3( 1, 0, 0 ), glm::dot( glm::vec3( 1, 0, 0 ), ptWorldMaxs ) ),
        Plane( glm::vec3( -1, 0, 0 ), glm::dot( glm::vec3( -1, 0, 0 ), ptWorldMins ) ),
    };

    float fPlaneDists[] =
    {
        glm::dot( planes[ 0 ].vNormal, pt ) - planes[ 0 ].fDist,
        glm::dot( planes[ 1 ].vNormal, pt ) - planes[ 1 ].fDist,
        glm::dot( planes[ 2 ].vNormal, pt ) - planes[ 2 ].fDist,
        glm::dot( planes[ 3 ].vNormal, pt ) - planes[ 3 ].fDist,
        glm::dot( planes[ 4 ].vNormal, pt ) - planes[ 4 ].fDist,
        glm::dot( planes[ 5 ].vNormal, pt ) - planes[ 5 ].fDist,
    };

    float fMaxDist = fPlaneDists[ 0 ];
    int iMaxIndex = 0;
    for ( int i = 0; i < 6; ++i )
    {
        if ( fPlaneDists[ i ] > fMaxDist )
        {
            iMaxIndex = i;
            fMaxDist = fPlaneDists[ i ];
        }
    }

    return (intptr_t) InitPlane( planes[ iMaxIndex ].vNormal, planes[ iMaxIndex ].fDist );
}
void GetCollisionNormal( glm::vec3 pt, intptr_t b, glm::vec3 *normal, glm::vec3 ptB )
{
    Plane *p = (Plane *) GetCollisionPlane( pt, b, ptB );
    _ASSERTE( p );
    //this does NOT cause a memory leak, since Vector is a marshalled type on client side
    if ( !normal )
        normal = new glm::vec3();
    *normal = glm::vec3( p->vNormal.x, p->vNormal.y, p->vNormal.z );
    delete p;
}
void DestructAABB( intptr_t boxptr )
{
    _ASSERTE( (BoundingBox *) boxptr );
    delete (BoundingBox *) boxptr;
}

//baseentity functions
BaseEntity::BaseEntity( BaseFace **EntFaces, GLuint FaceLength, Transform *transform, glm::vec3 mins, glm::vec3 maxs, World *world ) :
    AABB( new BoundingBox( mins, maxs ) ), EntFaces( EntFaces ), FaceLength( FaceLength ), transform( transform ), world( world )
{
	this->EntIndex = AddEntToWorld( (intptr_t) world, (intptr_t) this );
}
BaseEntity::BaseEntity( const BaseEntity &e )
{
    FaceLength = e.FaceLength;
    AABB = new BoundingBox( *e.AABB );
    transform = new Transform( *e.transform );
    world = e.world;
    EntIndex = e.EntIndex;
    std::vector<BaseFace *> faces;
    for ( int i = 0; i < FaceLength; ++i )
    {
        faces.push_back( new BaseFace( *e.EntFaces[ i ] ) );
    }
    EntFaces = faces.data();
}
BaseEntity::~BaseEntity()
{
	delete EntFaces;
    delete AABB;
	delete transform;
}
intptr_t InitBaseEntity( intptr_t *EntFaces, unsigned int FaceLength, intptr_t transform, glm::vec3 mins, glm::vec3 maxs, intptr_t world )
{
    return (intptr_t) new BaseEntity( (BaseFace **) EntFaces, FaceLength, (Transform *) transform, mins, maxs, (World *) world );
}
intptr_t GetEntTransform( intptr_t ent )
{
    return (intptr_t) new Transform( *((BaseEntity *) ent)->transform );
}
intptr_t GetEntBBox( intptr_t ent )
{
    return (intptr_t) new BoundingBox( *((BaseEntity *) ent)->AABB );
}
intptr_t GetEntWorld( intptr_t ent )
{
    return (intptr_t) new World( *((BaseEntity *) ent)->world );
}
void DestructBaseEntity( intptr_t entptr )
{
    delete (BaseEntity *) entptr;
}

//camear functions
Camera::Camera( Transform *transform, glm::mat4 perspective, World *world ) :
    BaseEntity( NULL, 0, transform, glm::vec3( -.5f, -1.5f, -.5f ), glm::vec3( .5f, .5f, .5f ), world ),
    m_Perspective( perspective )
{
};
intptr_t MakePerspective( float fov, float aspect, float nearclip, float farclip )
{
    glm::mat4 *perspective = new glm::mat4( glm::perspective( fov, aspect, nearclip, farclip ) );
    return (intptr_t) perspective;
}
intptr_t InitCamera( intptr_t transform, intptr_t perspective, intptr_t world )
{
    return (intptr_t) new Camera( (Transform *) transform, *((glm::mat4 *)perspective), (World *) world );
}
void DestructCamera( intptr_t camptr )
{
    delete (Camera *) camptr;
}