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


struct RENDER_API BaseFace
{
	BaseFace( GLuint VertLength, float *vertices, GLuint IndLength, GLuint *indices, Texture texture, GLenum DrawType );

	float *vertices;
	unsigned int VertLength;

	GLuint *indices;
	GLuint IndLength;

	GLuint VBO;
	GLuint VAO;
	GLuint EBO;

	Texture texture;
};
extern "C" RENDER_API void InitBaseFace( unsigned int Vertlength, float *vertices, unsigned int IndLength, unsigned int *indices, Texture texture, BaseFace *pFace );

#endif