#ifndef BRUSH_H
#define BRUSH_H

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
#include "BaseEntity.h"
#include "CRTDBG.h"

struct Texture;

struct RENDER_API Brush : public BaseEntity
{
	Brush( glm::vec3 mins, glm::vec3 maxs, Texture **textures, GLuint TextureLength, World *world );
};
extern "C" RENDER_API intptr_t InitBrush( glm::vec3 mins, glm::vec3 maxs, intptr_t *textures, unsigned int TextureLength, intptr_t world );
extern "C" RENDER_API void DestructBrush( intptr_t brushptr );

#endif