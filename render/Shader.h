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
#include <vector>

struct RENDER_API Shader
{
	Shader( const char *VertPath, const char *FragPath );
	Shader();

	GLuint ID = 0;
};

extern "C" RENDER_API void InitShader( const char *VertPath, const char *FragPath, Shader *pShader );

extern "C" RENDER_API void UseShader( Shader s );

extern "C" RENDER_API void SetBool( Shader s, const std::string &name, bool value );
extern "C" RENDER_API void SetInt( Shader s, const std::string &name, int value );
extern "C" RENDER_API void SetFloat( Shader s, const std::string &name, float value );
extern "C" RENDER_API void SetMatrix( Shader s, const std::string &name, glm::mat4 value );
extern "C" RENDER_API void SetVec4Array( Shader s, const std::string &name, std::vector<glm::vec4> values );
extern "C" RENDER_API void SetFloatArray( Shader s, const std::string &name, std::vector<float> values );

#endif