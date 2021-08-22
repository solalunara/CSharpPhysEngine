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
	Transform();

	glm::mat4 m_ThisToWorld;
	glm::mat4 m_WorldToThis;

	glm::vec3 m_Position;
	glm::vec3 m_Scale;
	glm::mat4 m_Rotation;

};
extern "C" RENDER_API void InitTransform( glm::vec3 position, glm::vec3 scale, glm::mat4 rotation, Transform *pTransform );
extern "C" RENDER_API void GetTransformVals( Transform transform, glm::vec3 &position, glm::vec3 &scale, glm::mat4 &rotation );
extern "C" RENDER_API void GetThisToWorld(Transform transform, glm::mat4 * pMat);
extern "C" RENDER_API void GetWorldToThis(Transform transform, glm::mat4 * pMat);
extern "C" RENDER_API void UpdateTransform( Transform &tptr );
extern "C" RENDER_API void AddToPos( Transform &tptr, glm::vec3 v );
extern "C" RENDER_API void SetPos( Transform &tptr, glm::vec3 v );
extern "C" RENDER_API void AddToScale( Transform &tptr, glm::vec3 v );
extern "C" RENDER_API void SetScale( Transform &tptr, glm::vec3 v );
extern "C" RENDER_API void AddToRotation( Transform &tptr, glm::mat4 m );
extern "C" RENDER_API void SetRotation( Transform &tptr, glm::mat4 m );
extern "C" RENDER_API void GetRight( Transform tptr, glm::vec3 &v );
extern "C" RENDER_API void GetUp( Transform tptr, glm::vec3 &v );
extern "C" RENDER_API void GetForward( Transform tptr, glm::vec3 &v );
extern "C" RENDER_API void TransformDirection( Transform tptr, glm::vec3 &dir );
extern "C" RENDER_API void TransformPoint( Transform tptr, glm::vec3 &pt );
extern "C" RENDER_API void InverseTransformDirection( Transform tptr, glm::vec3 &dir );
extern "C" RENDER_API void InverseTransformPoint( Transform tptr, glm::vec3 &pt );

#endif