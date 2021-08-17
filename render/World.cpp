#include "pch.h"
#include "World.h"


World::World()
{
	iCurrentIndex = 0;
	EntList = new BaseEntity * [ 999 ]{ 0 };
}
World::~World()
{
	for ( unsigned int i = 0; i < iCurrentIndex; ++i )
		if ( EntList[ i ] ) delete EntList[ i ];
	delete[] EntList;
}
intptr_t InitWorld()
{
	return (intptr_t) new World();
}
unsigned int AddEntToWorld( intptr_t w, intptr_t pEnt )
{
	((World *) w)->EntList[ ((World *) w)->iCurrentIndex ] = (BaseEntity *) pEnt;
	return ((World *) w)->iCurrentIndex++;
}
intptr_t GetEntAtWorldIndex( intptr_t w, unsigned int index )
{
	return (intptr_t) ((World *) w)->EntList[ index ];
}
unsigned int GetWorldSize( intptr_t w )
{
	return ((World *) w)->iCurrentIndex;
}
void DestructWorld( intptr_t wptr )
{
	delete (World *) wptr;
}