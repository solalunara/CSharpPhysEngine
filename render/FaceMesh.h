#ifndef FACEMESH_H
#define FACEMESH_H

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


struct RENDER_API FaceMesh
{
	FaceMesh( int VertLength, float *vertices, int IndLength, int *indices, Texture texture, glm::vec3 vNormal, GLenum DrawType );

	float vertices[ VRT_MAX_SIZE ];
	int VertLength;

	int indices[ IND_MAX_SIZE ];
	int IndLength;

	GLuint VBO;
	GLuint VAO;
	GLuint EBO;

	Texture texture;

	glm::vec3 vNormal;
};
extern "C" RENDER_API void InitMesh( int Vertlength, float *vertices, int IndLength, int *indices, Texture texture, glm::vec3 vNormal, FaceMesh *mesh );
extern "C" RENDER_API void UpdateMesh( FaceMesh *mesh );
extern "C" RENDER_API void DestructMesh( FaceMesh mesh );

#endif