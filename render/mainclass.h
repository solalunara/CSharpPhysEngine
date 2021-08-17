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

struct Shader;
struct Camera;
struct World;

enum
{
	WINDOW_MOVE = 1 << 5,
};
typedef unsigned int Move;


//callback functions
extern "C" RENDER_API void WindowSizeChanged( GLFWwindow *window, int width, int height );

extern "C" RENDER_API void InputMGR( GLFWwindow *window, int key, int scancode, int action, int mods );
struct InputData
{
	bool action;
	int key;
	int scancode;
	int act;
	int mods;
	intptr_t window;
};

extern "C" RENDER_API void SetFlag( unsigned int *ToSet, unsigned int val, bool bVal );

//render related functions
extern "C" RENDER_API void Init( intptr_t *window, intptr_t *shader, intptr_t *camera, intptr_t *world );
extern "C" RENDER_API void RenderLoop( intptr_t window, intptr_t shader, intptr_t camera, intptr_t world, bool bMouseControl );
extern "C" RENDER_API void Terminate( intptr_t window, intptr_t shader, intptr_t camera, intptr_t world );

extern "C" RENDER_API bool ShouldTerminate( intptr_t window );

typedef int (*fptr)( intptr_t window, int key, int scancode, int act, int mods );
extern "C" RENDER_API void SetInputCallback( intptr_t fn );

#endif