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

extern "C" RENDER_API void InitBaseEntity( BaseFace *EntFaces, int FaceLength, Transform transform, glm::vec3 mins, glm::vec3 maxs, BaseEntity *pEnt );
extern "C" RENDER_API void InitBrush( glm::vec3 mins, glm::vec3 maxs, Texture *textures, int TextureLength, BaseEntity *pEnt );

extern "C" RENDER_API void GetBaseFaceAtIndex( BaseEntity ent, BaseFace *pFace, int index );

extern "C" RENDER_API void DestructBaseEntity( BaseEntity *ent );

//matrix utils
extern "C" RENDER_API void MakePerspective( float fov, float aspect, float nearclip, float farclip, glm::mat4 *pMat );
extern "C" RENDER_API void MakeRotMatrix( float degrees, glm::vec3 axis, glm::mat4 * pMat );
extern "C" RENDER_API void MultiplyMatrix( glm::mat4 *pMultiply, glm::mat4 multiplier );

#endif