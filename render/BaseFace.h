#ifndef BASEFACE_H
#define BASEFACE_H

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

#include "Texture.h"

#define VRT_MAX_SIZE 100
#define IND_MAX_SIZE 50


struct RENDER_API BaseFace
{
	BaseFace( int VertLength, float *vertices, int IndLength, int *indices, Texture texture, GLenum DrawType );
	BaseFace();

	float vertices[ VRT_MAX_SIZE ];
	int VertLength;

	int indices[ IND_MAX_SIZE ];
	int IndLength;

	GLuint VBO;
	GLuint VAO;
	GLuint EBO;

	Texture texture;
};
extern "C" RENDER_API void InitBaseFace( int Vertlength, float *vertices, int IndLength, int *indices, Texture texture, BaseFace *pFace );

extern "C" RENDER_API void DestructBaseFace( BaseFace *face );

extern "C" RENDER_API float GetVertAtIndex( BaseFace face, int index );
extern "C" RENDER_API int GetIndAtIndex( BaseFace face, int index );

#endif