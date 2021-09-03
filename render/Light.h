#ifndef LIGHT_H
#define LIGHT_H

#ifdef RENDER_EXPORTS
#define RENDER_API __declspec(dllexport)
#else
#define RENDER_API __declspec(dllimport)
#endif

#pragma once

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

struct Shader;

struct Light
{
	glm::vec4 Position;
	glm::vec4 Color;
	float Intensity;
};
extern "C" RENDER_API void SetLights( Shader shader, Light *PointLights, int LightLength );
extern "C" RENDER_API void SetAmbientLight( Shader shader, float value );

#endif