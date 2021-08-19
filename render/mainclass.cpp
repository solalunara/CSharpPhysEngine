#include "pch.h"
#include "mainclass.h"
#include "CRTDBG.h"
#include "World.h"
#include "Texture.h"
#include "Transform.h"
#include "BaseEntity.h"
#include "BaseFace.h"
#include "Shader.h"
#include "Brush.h"

#define INITIAL_WIDTH 1000
#define INITIAL_HEIGHT 720

#define LKSPD 20.0f

static Move move = NULL;
static float lasttime;
static fptr Callback = NULL;

void WindowSizeChanged( GLFWwindow *window, int width, int height )
{
	glViewport( 0, 0, width, height );
	move |= WINDOW_MOVE;
}
void InputMGR( GLFWwindow *window, int key, int scancode, int action, int mods )
{
	if ( Callback )
		Callback( (intptr_t) window, key, scancode, action, mods );
	/*
	if ( key == GLFW_KEY_ESCAPE && action == GLFW_PRESS )
		move ^= ALLOW_MOUSE_TURN;
	*/
}
void SetFlag( unsigned int *ToSet, unsigned int val, bool bVal )
{
	if ( bVal )
		*ToSet |= val;
	else
		*ToSet &= ~val;
}
void Init( intptr_t *window, intptr_t *shader, intptr_t *camera, intptr_t *world )
{
	glfwInit();
	glfwWindowHint( GLFW_CONTEXT_VERSION_MAJOR, 3 );
	glfwWindowHint( GLFW_CONTEXT_VERSION_MINOR, 3 );
	glfwWindowHint( GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE );
	//initialize the rendering window
	if ( !window )
		window = new intptr_t();
	*window = (intptr_t) glfwCreateWindow( INITIAL_WIDTH, INITIAL_HEIGHT, "testwindow", NULL, NULL );
	if ( window == NULL )
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
	//if we're launching from visual studio then the relative paths are different than an executable, so change the paths based on if we're in debug mode
	if ( !shader )
	 shader = new intptr_t();
	*shader = (intptr_t) new Shader( "Shaders/VertexShader.vert", "Shaders/FragmentShader.frag" );
	if ( !world )
		world = new intptr_t();
	*world = (intptr_t) new World();

	Transform *CamTransform = new Transform( glm::vec3( 0, 0, 0 ), glm::vec3( 1, 1, 1 ), glm::mat4( 1 ) );


	int width, height;
	glfwGetWindowSize( (GLFWwindow *) *window, &width, &height );
	if ( !camera )
		camera = new intptr_t();
	*camera = (intptr_t) new Camera( CamTransform, glm::perspective( 45.0f, (float) width / height, 0.1f, 1000.0f ), (World *) *world );

	glEnable( GL_DEPTH_TEST );
	glDepthFunc( GL_LESS );


	lasttime = (float) glfwGetTime();
}
void RenderLoop( intptr_t window, intptr_t shader, intptr_t camera, intptr_t world, bool bMouseControl )
{
	//time stuff
	float frametime = (float) glfwGetTime() - lasttime;
	lasttime = (float) glfwGetTime();
	float fps = 1 / frametime;


	glClearColor( .1f, .2f, .7f, 1.0f );
	glClear( GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT );

	int width, height;
	glfwGetWindowSize( (GLFWwindow *) window, &width, &height );
	if ( bMouseControl )
	{
		glfwSetInputMode( (GLFWwindow *) window, GLFW_CURSOR, GLFW_CURSOR_HIDDEN );
		glm::vec2 vCenterDist( 0.0f, 0.0f );
		//try to avoid a jerk at startup when moving
		if ( lasttime > 1 )
		{
			//get the distance from the center of the screen and create a 
			double xpos, ypos;
			glfwGetCursorPos( (GLFWwindow *) window, &xpos, &ypos );
			vCenterDist = glm::vec2( xpos - ( width / 2 ), ypos - ( height / 2 ) );
		}
		//rotation axis are in local space, so use inverse transform to get the world up
		glm::vec3 Up( 0, 1.0f, 0 );
		InverseTransformDirection( (intptr_t) ((Camera *) camera)->transform, &Up );
		AddToRotation( (intptr_t) ((Camera *) camera)->transform, glm::rotate( glm::mat4( 1.0f ), glm::radians( LKSPD * frametime * -vCenterDist.x ), Up ) );
		AddToRotation( (intptr_t) ((Camera *) camera)->transform, glm::rotate( glm::mat4( 1.0f ), glm::radians( LKSPD * frametime * -vCenterDist.y ), glm::vec3( 1.0f, 0, 0 ) ) );
		//move the cursor to the middle of the screen
		glfwSetCursorPos( (GLFWwindow *) window, width / 2, height / 2 );
	}
	else
		glfwSetInputMode( (GLFWwindow *) window, GLFW_CURSOR, GLFW_CURSOR_NORMAL );
	//change the perspective matrix for the new aspect ratio if the window has been resized
	if ( move & WINDOW_MOVE )
	{
		((Camera *) camera)->m_Perspective = glm::perspective( 45.0f, (float) width / height, 0.1f, 1000.0f );
		move &= ~WINDOW_MOVE;
	}

	UseShader( shader );

	//tell the vertex shader about the camera position and perspective matrix
	//glUniformMatrix4fv( glGetUniformLocation( ((Shader *) shader)->ID, "CameraTransform" ), 1, GL_FALSE, glm::value_ptr( ((Camera *) camera)->transform->m_WorldToThis ) );
	//glUniformMatrix4fv( glGetUniformLocation( ((Shader *) shader)->ID, "Perspective" ), 1, GL_FALSE, glm::value_ptr( ((Camera *) camera)->m_Perspective ) );

	SetMatrix( shader, "CameraTransform", (intptr_t) &((Camera *) camera)->transform->m_WorldToThis );
	SetMatrix( shader, "Perspective", (intptr_t) &((Camera *) camera)->m_Perspective );

	bool bCameraCollision = false;
	//traverse the world for entities
	for ( GLuint i = 0; i < GetWorldSize( world ); ++i )
	{
		BaseEntity *enti = (BaseEntity *) GetEntAtWorldIndex( world, i );
		if ( enti == (BaseEntity *) camera )
			continue;
		//tell the vertex shader about where the entity is in world space
		SetMatrix( shader, "transform", (intptr_t) &enti->transform->m_ThisToWorld );
		//traverse the entity for faces
		for ( unsigned int i = 0; i < enti->FaceLength; ++i )
		{
			BaseFace *pFace = enti->EntFaces[ i ];
			if ( pFace == NULL )
				continue;
			glBindVertexArray( pFace->VAO );
			glBindBuffer( GL_ELEMENT_ARRAY_BUFFER, pFace->EBO );
			glBindTexture( GL_TEXTURE_2D, pFace->texture->ID );
			glDrawElements( GL_TRIANGLES, pFace->IndLength, GL_UNSIGNED_INT, 0 );
		}
	}

	glfwSwapBuffers( (GLFWwindow *) window );
	glfwPollEvents();
}
void Terminate( intptr_t window, intptr_t shader, intptr_t camera, intptr_t world )
{
	delete (Shader *) shader;
	delete (World *) world;

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
void SetInputCallback( intptr_t fn )
{
	Callback = (fptr) fn;
}