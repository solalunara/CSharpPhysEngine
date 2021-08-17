#ifndef TRANSFORM_H
#define TRANSFORM_H

#ifdef RENDER_EXPORTS
#define RENDER_API __declspec(dllexport)
#else
#define RENDER_API __declspec(dllimport)
#endif

#pragma once

#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>


struct RENDER_API Transform
{
	Transform( glm::vec3 position, glm::vec3 scale, glm::mat4 rotation );

	glm::mat4 m_ThisToWorld;
	glm::mat4 m_WorldToThis;

	glm::vec3 m_Position;
	glm::vec3 m_Scale;
	glm::mat4 m_Rotation;

};
extern "C" RENDER_API intptr_t InitTransform( glm::vec3 position, glm::vec3 scale, intptr_t rotation );
extern "C" RENDER_API void UpdateTransform( intptr_t tptr );
extern "C" RENDER_API void AddToPos( intptr_t tptr, glm::vec3 v );
extern "C" RENDER_API void SetPos( intptr_t tptr, glm::vec3 v );
extern "C" RENDER_API void AddToScale( intptr_t tptr, glm::vec3 v );
extern "C" RENDER_API void SetScale( intptr_t tptr, glm::vec3 v );
extern "C" RENDER_API void AddToRotation( intptr_t tptr, glm::mat4 m );
extern "C" RENDER_API void SetRotation( intptr_t tptr, glm::mat4 m );
extern "C" RENDER_API void GetRight( intptr_t tptr, glm::vec3 *v );
extern "C" RENDER_API void GetUp( intptr_t tptr, glm::vec3 *v );
extern "C" RENDER_API void GetForward( intptr_t tptr, glm::vec3 *v );
extern "C" RENDER_API void TransformDirection( intptr_t tptr, glm::vec3 *dir );
extern "C" RENDER_API void TransformPoint( intptr_t tptr, glm::vec3 *pt );
extern "C" RENDER_API void InverseTransformDirection( intptr_t tptr, glm::vec3 *dir );
extern "C" RENDER_API void InverseTransformPoint( intptr_t tptr, glm::vec3 *pt );
extern "C" RENDER_API void DestructTransform( intptr_t tptr );

extern "C" RENDER_API intptr_t InitMatrix( float *values1, float *values2, float *values3, float *values4 );
extern "C" RENDER_API void DestructMatrix( intptr_t mptr );

#endif