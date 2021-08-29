#ifndef TEXTURE_H
#define TEXTURE_H

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

#pragma once
struct RENDER_API Texture
{
	Texture( const char *FilePath, GLenum Unit = GL_TEXTURE0, GLenum WrapStyle = 0, GLenum FilterStyleMin = 0, GLenum FilterStyleMag = 0, float *BorderColor = 0 );
	Texture();

	bool bInitialized;

	GLuint ID;
	GLenum Unit;
};
extern "C" RENDER_API void InitTexture( const char *FilePath, Texture *pTex );
extern "C" RENDER_API void DestructTexture( Texture *pTex );

#endif