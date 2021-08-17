#ifndef WORLD_H
#define WORLD_H

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


struct RENDER_API World
{
	World();
	~World();

	GLuint iCurrentIndex;
	BaseEntity **EntList;
};
extern "C" RENDER_API intptr_t InitWorld();
extern "C" RENDER_API unsigned int AddEntToWorld( intptr_t w, intptr_t pEnt );
extern "C" RENDER_API intptr_t GetEntAtWorldIndex( intptr_t w, unsigned int index );
extern "C" RENDER_API unsigned int GetWorldSize( intptr_t w );
extern "C" RENDER_API void DestructWorld( intptr_t wptr );
#endif