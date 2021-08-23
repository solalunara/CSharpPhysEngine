#include "pch.h"
#include "mainclass.h"
#include "CRTDBG.h"
#include "Texture.h"
#include "Transform.h"
#include "BaseFace.h"

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
void SetFlag( unsigned int *ToSet, unsigned int val, bool bVal )
{
	if ( bVal )
		*ToSet |= val;
	else
		*ToSet &= ~val;
}
void Init( intptr_t *window, Shader *shader )
{
	if ( !window )
		window = new intptr_t();
	if ( !shader )
		shader = new Shader();

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

	//gather the source for the shaders from their respective files
	*shader = Shader( "Shaders/VertexShader.vert", "Shaders/FragmentShader.frag" );

	glEnable( GL_DEPTH_TEST );
	glDepthFunc( GL_LESS );
}


void RenderLoop( intptr_t window, Shader shader, BaseEntity camera, glm::mat4 perspective, BaseEntity *pRenderEnts, int iRenderEntLength )
{
	glClearColor( .1f, .2f, .7f, 1.0f );
	glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT );

	int width, height;
	glfwGetWindowSize( (GLFWwindow *) window, &width, &height );

	UseShader( shader );

	//tell the vertex shader about the camera position and perspective matrix
	SetMatrix( shader, "CameraTransform", camera.transform.m_WorldToThis );
	SetMatrix( shader, "Perspective", perspective );

	bool bCameraCollision = false;
	//traverse the world for entities
	for ( int i = 0; i < iRenderEntLength; ++i )
	{
		BaseEntity enti = pRenderEnts[i];
		if ( enti.FaceLength == 0 )
			continue; //nothing to render
		//tell the vertex shader about where the entity is in world space
		SetMatrix( shader, "transform", enti.transform.m_ThisToWorld );
		//traverse the entity for faces
		for ( unsigned int i = 0; i < enti.FaceLength; ++i )
		{
			BaseFace pFace = enti.EntFaces[ i ];
			glBindVertexArray( pFace.VAO );
			glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, pFace.EBO );
			glBindTexture( GL_TEXTURE_2D, pFace.texture.ID );
			glDrawElements( GL_TRIANGLES, pFace.IndLength, GL_UNSIGNED_INT, 0 );
		}
	}

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
	if ( !x )
		x = new int();
	if ( !y )
		y = new int();
	glfwGetWindowSize( (GLFWwindow *) window, x, y );
}

void GetMouseOffset( intptr_t window, double *x, double *y )
{
	int width, height;
	glfwGetWindowSize( (GLFWwindow *) window, &width, &height );

	if ( !x )
		x = new double();
	if ( !y )
		y = new double();
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

void MakeRotMatrix( float degrees, glm::vec3 axis, glm::mat4 *pMat )
{
	if ( !pMat )
		pMat = new glm::mat4( 1 );
	*pMat = glm::rotate( glm::mat4( 1 ), glm::radians( degrees ), axis );
}

void SetInputCallback( intptr_t fn )
{
	Callback = (fptr) fn;
}
void SetWindowMoveCallback( intptr_t fn )
{
	WindowMoveCallback = (fptrw) fn;
}