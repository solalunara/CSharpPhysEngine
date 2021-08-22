// c# can't debug a dll loaded at runtime, so this is a c++ program that allows breakpoints to be hit in the dll

#include "BaseEntity.h"
#include "mainclass.h"
#include "Texture.h"
#include "Shader.h"



int main()
{
	
	intptr_t window;
	Shader shader;
	Camera camera;
	Init( &window, &shader, &camera );
	Texture t = Texture( "themasterpiece.png" );
	Texture tex[] = { t };
	BaseEntity ents[ 1 ] = { BaseEntity( glm::vec3( -10, -10, -11 ), glm::vec3( 10, 10, -9 ), tex, 1 ) };
	while ( !ShouldTerminate( window ) )
	{
		AddToRotation( camera.LinkedEnt.transform, glm::rotate( glm::mat4( 1 ), 0.1f, glm::vec3( 0, 1, 0 ) ) );
		RenderLoop( window, shader, &camera, ents, 1, false );
	}
	Terminate();
}