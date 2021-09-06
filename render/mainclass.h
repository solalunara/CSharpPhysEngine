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
#include "FaceMesh.h"

typedef unsigned int uint;

//callback functions
extern "C" RENDER_API void WindowSizeChanged( GLFWwindow *window, int width, int height );
extern "C" RENDER_API void InputMGR( GLFWwindow *window, int key, int scancode, int action, int mods );
extern "C" RENDER_API void SetFlag( uint *ToSet, unsigned int val, bool bVal );

//render related functions
extern "C" RENDER_API void Init( intptr_t *window );
extern "C" RENDER_API void StartFrame( intptr_t window );
extern "C" RENDER_API void SetCameraValues( Shader shader, glm::mat4 perspective, glm::mat4 WorldToThis );
extern "C" RENDER_API void SetRenderValues( Shader shader, glm::mat4 m );
extern "C" RENDER_API void RenderMesh( Shader shader, FaceMesh face );
extern "C" RENDER_API void EndFrame( intptr_t window );

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



#endif