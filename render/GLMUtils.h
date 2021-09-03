#ifndef GLMUTILS_H
#define GLMUTILS_H

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

extern "C" RENDER_API void GLMPerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat );
extern "C" RENDER_API void GLMRotMatrix( float degrees, glm::vec3 axis, glm::mat4 *pMat );
extern "C" RENDER_API void GLMMultiplyMatrix( glm::mat4 *pMultiply, glm::mat4 multiplier );
extern "C" RENDER_API void GLMMultMatrixVector( glm::mat4 matrix, glm::vec4 *vector );
extern "C" RENDER_API void GLMInvertMatrix( glm::mat4 *matrix );

extern "C" RENDER_API void GLMScale( glm::vec3 scale, glm::mat4 *m );
extern "C" RENDER_API void GLMTranslate( glm::vec3 pt, glm::mat4 *m );

#endif