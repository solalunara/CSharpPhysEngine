#ifndef MAINCLASS_H
#define MAINCLASS_H

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

#include "Shader.h"
#include "BaseEntity.h"

typedef unsigned int uint;

struct RENDER_API Light
{
	glm::vec4 Position;
	glm::vec4 Color;
	float Intensity;
};


//callback functions
extern "C" RENDER_API void WindowSizeChanged( GLFWwindow *window, int width, int height );
extern "C" RENDER_API void InputMGR( GLFWwindow *window, int key, int scancode, int action, int mods );
extern "C" RENDER_API void SetFlag( uint *ToSet, unsigned int val, bool bVal );

//render related functions
extern "C" RENDER_API void Init( intptr_t *window );
extern "C" RENDER_API void StartFrame( intptr_t window );
extern "C" RENDER_API void SetRenderValues( Shader shader, Transform t );
extern "C" RENDER_API void RenderFace( intptr_t window, Shader shader, BaseFace face );
extern "C" RENDER_API void EndFrame( intptr_t window );

//light functions
extern "C" RENDER_API void SetLights( Shader shader, Light *PointLights, int LightLength );
extern "C" RENDER_API void SetAmbientLight( Shader shader, float value );

extern "C" RENDER_API void Terminate();
extern "C" RENDER_API bool ShouldTerminate( intptr_t window );
extern "C" RENDER_API float GetTime();
extern "C" RENDER_API void GetWindowSize( intptr_t window, int *x, int *y );

//input callback
typedef intptr_t (*fptr)( intptr_t window, int key, int scancode, int act, int mods );
extern "C" RENDER_API void SetInputCallback( intptr_t fn );

//window move callback
typedef intptr_t (*fptrw)( intptr_t window, int width, int height );
extern "C" RENDER_API void SetWindowMoveCallback( intptr_t fn );


//mouse related functions
extern "C" RENDER_API void GetMouseOffset( intptr_t window, double *x, double *y );
extern "C" RENDER_API void MoveMouseToCenter( intptr_t window );
extern "C" RENDER_API void HideMouse( intptr_t window );
extern "C" RENDER_API void ShowMouse( intptr_t window );

extern "C" RENDER_API void InvertMatrix( glm::mat4 *matrix );
extern "C" RENDER_API void MultiplyVector( glm::mat4 matrix, glm::vec3 *vector );

#endif