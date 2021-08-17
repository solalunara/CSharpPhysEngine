#ifndef BASEENT_H
#define BASEENT_H

#ifdef RENDER_EXPORTS
#define RENDER_API __declspec(dllexport)
#else
#define RENDER_API __declspec(dllimport)
#endif

#pragma once
#pragma warning( disable: 4251 )

#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>


struct BaseFace;
struct World;
struct Transform;


struct RENDER_API Plane
{
	Plane( glm::vec3 vNormal, float fDist ) :
		vNormal( vNormal ), fDist( fDist )
	{
	}
	Plane() :
		vNormal( glm::vec3( 0 ) ), fDist( 0 )
	{
	}

	glm::vec3 vNormal;
	float fDist;
};
extern "C" RENDER_API intptr_t InitPlane( glm::vec3 vNormal, float fDist );
extern "C" RENDER_API float DistanceFromPointToPlane( intptr_t p, glm::vec3 pt );
extern "C" RENDER_API void ClosestPointOnPlane( intptr_t p, glm::vec3 pt, glm::vec3 *out );
extern "C" RENDER_API void DestructPlane( intptr_t ptr );

struct RENDER_API BoundingBox
{
	BoundingBox( glm::vec3 mins, glm::vec3 maxs ) :
		mins( mins ), maxs( maxs )
	{
	}

	glm::vec3 mins;
	glm::vec3 maxs;
};
extern "C" RENDER_API intptr_t InitAABB( glm::vec3 mins, glm::vec3 maxs );
extern "C" RENDER_API bool TestCollisionPoint( glm::vec3 pt, intptr_t b, glm::vec3 ptB  ); //tests a point against an AABB at a location
extern "C" RENDER_API bool TestCollisionAABB( intptr_t b1, intptr_t b2, glm::vec3 ptB1, glm::vec3 ptB2 ); //test an aabb against another aabb
extern "C" RENDER_API intptr_t GetCollisionPlane( glm::vec3 pt, intptr_t b, glm::vec3 ptB ); //gets the plane of the aabb that a point is colliding against
extern "C" RENDER_API void GetCollisionNormal( glm::vec3 pt, intptr_t b, glm::vec3 *normal, glm::vec3 ptB ); //gets the normal of the collision
extern "C" RENDER_API void DestructAABB( intptr_t boxptr );


struct RENDER_API BaseEntity
{
public:
	BaseEntity( BaseFace **EntFaces, GLuint FaceLength, Transform *transform, glm::vec3 mins, glm::vec3 maxs, World *world );
	~BaseEntity();

	BaseFace **EntFaces;
	GLuint FaceLength;

	World *world;
	GLuint EntIndex;

	Transform *transform;
	BoundingBox AABB;

};
extern "C" RENDER_API intptr_t InitBaseEntity( intptr_t *EntFaces, unsigned int FaceLength, intptr_t transform, glm::vec3 mins, glm::vec3 maxs, intptr_t world );
extern "C" RENDER_API intptr_t GetEntMatrix( intptr_t b );
extern "C" RENDER_API intptr_t GetEntTransform( intptr_t ent );
extern "C" RENDER_API void DestructBaseEntity( intptr_t entptr );

struct RENDER_API Camera : public BaseEntity
{
	Camera( Transform *transform, glm::mat4 perspective, World *world );

	glm::mat4 m_Perspective;
};
extern "C" RENDER_API intptr_t InitCamera( intptr_t transform, intptr_t perspective, intptr_t world );
extern "C" RENDER_API void DestructCamera( intptr_t camptr );

#endif