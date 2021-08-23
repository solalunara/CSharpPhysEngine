#include "pch.h"
#include "Transform.h"
#include "CRTDBG.h"

Transform::Transform( glm::vec3 position, glm::vec3 scale, glm::mat4 rotation ) :
	m_Position( position ), m_Scale( scale ), m_Rotation( rotation ), m_WorldToThis( glm::mat4( 1 ) ), m_ThisToWorld( glm::mat4( 1 ) )
{
	UpdateTransform( this );
}
Transform::Transform():
	m_Position( glm::vec3() ), m_Scale( glm::vec3( 1 ) ), m_Rotation( glm::mat4( 1 ) ), m_WorldToThis( glm::mat4( 1 ) ), m_ThisToWorld( glm::mat4( 1 ) )
{
}
void InitTransform( glm::vec3 position, glm::vec3 scale, glm::mat4 rotation, Transform *pTransform )
{
	if ( !pTransform )
		pTransform = new Transform();
	*pTransform = Transform( position, scale, rotation );
}
void GetThisToWorld( Transform transform, glm::mat4 *pMat )
{
	if ( !pMat )
		pMat = new glm::mat4();
	*pMat = glm::mat4( transform.m_ThisToWorld );
}
void GetWorldToThis( Transform transform, glm::mat4 *pMat )
{
	if ( !pMat )
		pMat = new glm::mat4();
	*pMat = glm::mat4( transform.m_WorldToThis );
}
void UpdateTransform( Transform *tptr )
{
	_ASSERTE( tptr );
	tptr->m_ThisToWorld = glm::translate( glm::mat4( 1.0f ), tptr->m_Position ) * tptr->m_Rotation * glm::scale( glm::mat4( 1.0f ), tptr->m_Scale );
	tptr->m_WorldToThis = glm::inverse( tptr->m_ThisToWorld );
}
void GetRight( Transform tptr, glm::vec3 *v )
{
	if ( !v )
		v = new glm::vec3();
	v->x = tptr.m_ThisToWorld[ 0 ][ 0 ];
	v->y = tptr.m_ThisToWorld[ 0 ][ 1 ];
	v->z = tptr.m_ThisToWorld[ 0 ][ 2 ];
}
void GetUp( Transform tptr, glm::vec3 *v )
{
	if ( !v )
		v = new glm::vec3();
	v->x = tptr.m_ThisToWorld[ 1 ][ 0 ];
	v->y = tptr.m_ThisToWorld[ 1 ][ 1 ];
	v->z = tptr.m_ThisToWorld[ 1 ][ 2 ];
}
void GetForward( Transform tptr, glm::vec3 *v )
{
	if ( !v )
		v = new glm::vec3();
	v->x = tptr.m_ThisToWorld[ 2 ][ 0 ];
	v->y = tptr.m_ThisToWorld[ 2 ][ 1 ];
	v->z = tptr.m_ThisToWorld[ 2 ][ 2 ];
}
void TransformDirection( Transform tptr, glm::vec3 *dir )
{
	glm::vec3 vNewDir = tptr.m_ThisToWorld * glm::vec4( *dir, 0.0f );

	dir->x = vNewDir.x;
	dir->y = vNewDir.y;
	dir->z = vNewDir.z;
}
void TransformPoint( Transform tptr, glm::vec3 *pt )
{
	glm::vec3 vNewPt = tptr.m_ThisToWorld * glm::vec4( *pt, 1.0f );

	pt->x = vNewPt.x;
	pt->y = vNewPt.y;
	pt->z = vNewPt.z;
}
void InverseTransformDirection( Transform tptr, glm::vec3 *dir )
{
	glm::vec3 vNewDir = tptr.m_WorldToThis * glm::vec4( *dir, 0.0f );

	dir->x = vNewDir.x;
	dir->y = vNewDir.y;
	dir->z = vNewDir.z;
}
void InverseTransformPoint( Transform tptr, glm::vec3 *pt )
{
	glm::vec3 vNewPt = tptr.m_WorldToThis * glm::vec4( *pt, 1.0f );

	pt->x = vNewPt.x;
	pt->y = vNewPt.y;
	pt->z = vNewPt.z;
}