#include "pch.h"
#include "mainclass.h"
#include "CRTDBG.h"
#include "Texture.h"
#include <vector>

#define INITIAL_WIDTH 1000
#define INITIAL_HEIGHT 720

#define LKSPD 20.0f

static fptr Callback = NULL;
static fptrw WindowMoveCallback = NULL;

void WindowSizeChanged( GLFWwindow *window, int width, int height )
{
	glViewport( 0, 0, width, height );
	//camera.m_Perspective = glm::perspective( 45.0f, (float) width / height, 0.1f, 1000.0f );
	if ( WindowMoveCallback )
		WindowMoveCallback( (intptr_t) window, width, height );
}
void InputMGR( GLFWwindow *window, int key, int scancode, int action, int mods )
{
	if ( Callback )
		Callback( (intptr_t) window, key, scancode, action, mods );
}
void SetFlag( uint *ToSet, unsigned int val, bool bVal )
{
	if ( bVal )
		*ToSet |= val;
	else
		*ToSet &= ~val;
}
void Init( intptr_t *window )
{
	_ASSERTE( window );

	glfwInit();
	glfwWindowHint( GLFW_CONTEXT_VERSION_MAJOR, 3 );
	glfwWindowHint( GLFW_CONTEXT_VERSION_MINOR, 3 );
	glfwWindowHint( GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE );
	//initialize the rendering window
	*window = (intptr_t) glfwCreateWindow( INITIAL_WIDTH, INITIAL_HEIGHT, "testwindow", NULL, NULL );
	if ( *window == NULL )
	{
		_ASSERTE( false );
		glfwTerminate();
		return;
	}
	glfwMakeContextCurrent( (GLFWwindow *) *window );
	if ( !gladLoadGLLoader( (GLADloadproc) glfwGetProcAddress ) )
	{
		_ASSERTE( false );
		return;
	}
	//set the viewport to render to
	glViewport( 0, 0, INITIAL_WIDTH, INITIAL_HEIGHT );

	//callback function for window size changed
	glfwSetFramebufferSizeCallback( (GLFWwindow *) *window, WindowSizeChanged );
	//callback function for key pressed
	glfwSetKeyCallback( (GLFWwindow *) *window, InputMGR );

	glEnable( GL_DEPTH_TEST ); 
	glEnable( GL_FRAMEBUFFER_SRGB );
	glDepthFunc( GL_LESS );
}

void StartFrame( intptr_t window )
{
	glClearColor( .1f, .2f, .7f, 1.0f );
	glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT );
}
void SetCameraValues( Shader shader, glm::mat4 perspective, glm::mat4 WorldToThis )
{
	UseShader( shader );
	SetMatrix( shader, "Perspective", perspective );
	SetMatrix( shader, "CameraTransform", WorldToThis );
}
void SetRenderValues( Shader shader, glm::mat4 m )
{
	UseShader( shader );
	SetMatrix( shader, "transform", m );
}
void RenderMesh( Shader shader, FaceMesh face )
{
	UseShader( shader );
	_ASSERTE( face.texture.bInitialized );
	glBindVertexArray( face.VAO );
	glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, face.EBO );
	glBindTexture( GL_TEXTURE_2D, face.texture.ID );
	glDrawElements( GL_TRIANGLES, face.IndLength, GL_UNSIGNED_INT, NULL );
}
void EndFrame( intptr_t window )
{
	glfwSwapBuffers( (GLFWwindow *) window );
	glfwPollEvents();
}

void Terminate()
{
	glfwTerminate();
}
bool ShouldTerminate( intptr_t window )
{
	return glfwWindowShouldClose( (GLFWwindow *) window );
}
float GetTime()
{
	return (float) glfwGetTime();
}
void GetWindowSize( intptr_t window, int *x, int *y )
{
	_ASSERTE( x && y );
	glfwGetWindowSize( (GLFWwindow *) window, x, y );
}
void GetMouseOffset( intptr_t window, double *x, double *y )
{
	int width, height;
	glfwGetWindowSize( (GLFWwindow *) window, &width, &height );

	_ASSERTE( x && y );
	//get the distance from the center of the screen and create a 
	double xpos, ypos;
	glfwGetCursorPos( (GLFWwindow *) window, &xpos, &ypos );
	*x = xpos - ( width / 2 );
	*y = ypos - ( height / 2 );
}
void MoveMouseToCenter( intptr_t window )
{
	int width, height;
	glfwGetWindowSize( (GLFWwindow *) window, &width, &height );
	glfwSetCursorPos( (GLFWwindow *) window, width / 2, height / 2 );
}
void HideMouse( intptr_t window )
{
	glfwSetInputMode( (GLFWwindow *) window, GLFW_CURSOR, GLFW_CURSOR_HIDDEN );
}
void ShowMouse( intptr_t window )
{
	glfwSetInputMode( (GLFWwindow *) window, GLFW_CURSOR, GLFW_CURSOR_NORMAL );
}

void SetInputCallback( intptr_t fn )
{
	Callback = (fptr) fn;
}
void SetWindowMoveCallback( intptr_t fn )
{
	WindowMoveCallback = (fptrw) fn;
}