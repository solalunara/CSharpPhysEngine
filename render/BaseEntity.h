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

#include "Transform.h"
#include "BaseFace.h"


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

struct RENDER_API BoundingBox
{
	BoundingBox( glm::vec3 mins, glm::vec3 maxs ) :
		mins( mins ), maxs( maxs )
	{
	}
	BoundingBox() :
		mins( glm::vec3() ), maxs( glm::vec3() )
	{
	}

	glm::vec3 mins;
	glm::vec3 maxs;
};


struct RENDER_API BaseEntity
{
public:
	BaseEntity( BaseFace *EntFaces, GLuint FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs );
	BaseEntity( glm::vec3 mins, glm::vec3 maxs, Texture *textures, GLuint TextureLength ); //brush init
	BaseEntity();

	BaseFace *EntFaces;
	GLuint FaceLength;

	Transform transform;
	BoundingBox AABB;

};
extern "C" RENDER_API void InitBaseEntity( BaseFace *EntFaces, unsigned int FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs, BaseEntity *pEnt );
extern "C" RENDER_API void InitBrush( glm::vec3 mins, glm::vec3 maxs, Texture *textures, unsigned int TextureLength, BaseEntity *pEnt );

//matrix utils
extern "C" RENDER_API void MakePerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat );
extern "C" RENDER_API void MakeRotMatrix( float degrees, glm::vec3 axis, glm::mat4 * pMat );
extern "C" RENDER_API void MultiplyMatrix( glm::mat4 *pMultiply, glm::mat4 multiplier );

#endif