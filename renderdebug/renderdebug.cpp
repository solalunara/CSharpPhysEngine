// renderdebug.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>
#include "BaseEntity.h"
#include "mainclass.h"
#include "Texture.h"
#include "Brush.h"



int main()
{
	intptr_t window, shader, camera, world;
	Init( &window, &shader, &camera, &world );
	intptr_t texture[] = { InitTexture( "themasterpiece.png" ) };
	InitBrush( glm::vec3( -10, -10, -11 ), glm::vec3( 10, 10, -9 ), texture, 1, world );
	while ( !ShouldTerminate( window ) )
	{
		RenderLoop( window, shader, camera, world, false );
	}
	DestructTexture( texture[ 0 ] );
	Terminate( window, shader, camera, world );
}
