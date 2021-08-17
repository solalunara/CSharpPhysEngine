#ifndef SHADER_H
#define SHADER_H

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
#include <iostream>
#include <fstream>
#include <sstream>

#pragma once
struct RENDER_API Shader
{
	Shader( const char *VertPath, const char *FragPath );

	GLuint ID = 0;
};

extern "C" RENDER_API void UseShader( intptr_t s );

extern "C" RENDER_API void SetBool( intptr_t s, const std::string & name, bool value );
extern "C" RENDER_API void SetInt( intptr_t s, const std::string & name, int value );
extern "C" RENDER_API void SetFloat( intptr_t s, const std::string & name, float value );
extern "C" RENDER_API void SetMatrix( intptr_t s, const std::string &name, intptr_t value );

#endif