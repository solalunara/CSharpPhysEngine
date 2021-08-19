#include "pch.h"
#include "Transform.h"
#include "CRTDBG.h"

Transform::Transform( glm::vec3 position, glm::vec3 scale, glm::mat4 rotation ) :
	m_Position( position ), m_Scale( scale ), m_Rotation( rotation )
{
	UpdateTransform( (intptr_t) this );
}
intptr_t InitTransform( glm::vec3 position, glm::vec3 scale, intptr_t rotation )
{
	return (intptr_t) new Transform( position, scale, *(glm::mat4 *) rotation );
}
void GetTransformVals( intptr_t transform, glm::vec3 *position, glm::vec3 *scale, intptr_t *rotation )
{
	if ( !position )
		position = new glm::vec3();
	if ( !scale )
		scale = new glm::vec3();
	if ( !rotation )
		rotation = new intptr_t();

	*position = ((Transform *) transform)->m_Position;
	*scale = ((Transform *) transform)->m_Scale;
	*rotation = (intptr_t) new glm::mat4( ((Transform *) transform)->m_Rotation );
}
intptr_t GetThisToWorld( intptr_t transform )
{
	return (intptr_t) new glm::mat4( ((Transform *) transform)->m_ThisToWorld );
}
intptr_t GetWorldToThis( intptr_t transform )
{
	return (intptr_t) new glm::mat4( ((Transform *) transform)->m_WorldToThis );
}
void UpdateTransform( intptr_t tptr )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_ThisToWorld = glm::translate( glm::mat4( 1.0f ), t->m_Position ) * t->m_Rotation * glm::scale( glm::mat4( 1.0f ), t->m_Scale );
	t->m_WorldToThis = glm::inverse( t->m_ThisToWorld );
}
void AddToPos( intptr_t tptr, glm::vec3 v )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Position += v;
	UpdateTransform( tptr );
}
void SetPos( intptr_t tptr, glm::vec3 v )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Position = v;
	UpdateTransform( tptr );
}
void AddToScale( intptr_t tptr, glm::vec3 v )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Scale += v;
	UpdateTransform( tptr );
}
void SetScale( intptr_t tptr, glm::vec3 v )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Scale = v;
	UpdateTransform( tptr );
}
void AddToRotation( intptr_t tptr, glm::mat4 m )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Rotation *= m; 
	UpdateTransform( tptr );
}
void SetRotation( intptr_t tptr, glm::mat4 m )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t );

	t->m_Rotation = m;
	UpdateTransform( tptr );
}
void GetRight( intptr_t tptr, glm::vec3 *v )
{
	Transform *t = (Transform *) tptr;
	if ( !v )
		v = new glm::vec3();

	v->x = t->m_ThisToWorld[ 0 ][ 0 ];
	v->y = t->m_ThisToWorld[ 0 ][ 1 ];
	v->z = t->m_ThisToWorld[ 0 ][ 2 ];
}
void GetUp( intptr_t tptr, glm::vec3 *v )
{
	Transform *t = (Transform *) tptr;
	if ( !v )
		v = new glm::vec3();

	v->x = t->m_ThisToWorld[ 1 ][ 0 ];
	v->y = t->m_ThisToWorld[ 1 ][ 1 ];
	v->z = t->m_ThisToWorld[ 1 ][ 2 ];
}
void GetForward( intptr_t tptr, glm::vec3 *v )
{
	Transform *t = (Transform *) tptr;
	if ( !v )
		v = new glm::vec3();

	v->x = t->m_ThisToWorld[ 2 ][ 0 ];
	v->y = t->m_ThisToWorld[ 2 ][ 1 ];
	v->z = t->m_ThisToWorld[ 2 ][ 2 ];
}
void TransformDirection( intptr_t tptr, glm::vec3 *dir )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t && dir );

	glm::vec3 vNewDir = t->m_ThisToWorld * glm::vec4( *dir, 0.0f );

	dir->x = vNewDir.x;
	dir->y = vNewDir.y;
	dir->z = vNewDir.z;
}
void TransformPoint( intptr_t tptr, glm::vec3 *pt )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t && pt );

	glm::vec3 vNewPt = t->m_ThisToWorld * glm::vec4( *pt, 1.0f );

	pt->x = vNewPt.x;
	pt->y = vNewPt.y;
	pt->z = vNewPt.z;
}
void InverseTransformDirection( intptr_t tptr, glm::vec3 *dir )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t && dir );

	glm::vec3 vNewDir = t->m_WorldToThis * glm::vec4( *dir, 0.0f );

	dir->x = vNewDir.x;
	dir->y = vNewDir.y;
	dir->z = vNewDir.z;
}
void InverseTransformPoint( intptr_t tptr, glm::vec3 *pt )
{
	Transform *t = (Transform *) tptr;
	_ASSERTE( t && pt );

	glm::vec3 vNewPt = t->m_WorldToThis * glm::vec4( *pt, 1.0f );

	pt->x = vNewPt.x;
	pt->y = vNewPt.y;
	pt->z = vNewPt.z;
}

intptr_t InitMatrix( float *values1, float *values2, float *values3, float *values4 )
{
	glm::mat4 *ret = new glm::mat4();
	float *values[] = { values1, values2, values3, values4 };
	for ( int r = 0; r < 4; ++r )
	{
		for ( int c = 0; c < 4; ++c )
		{
			( *ret )[ r ][ c ] = values[ r ][ c ];
		}
	}
	return (intptr_t) ret;
}
void DestructMatrix( intptr_t mptr )
{
	delete (glm::mat4 *) mptr;
}