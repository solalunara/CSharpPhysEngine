using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Application
{
    public class BaseEnt_Interface
    {
        //Plane
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitPlane( Vector vNormal, float fDist );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern float DistanceFromPointToPlane( IntPtr plane, Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void ClosestPointOnPlane( IntPtr plane, Vector point, out Vector PtOnPlane );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructPlane( IntPtr plane );

        //BoundingBox
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitAABB( Vector mins, Vector maxs );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool TestCollisionPoint( Vector pt, IntPtr AABB, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool TestCollisionAABB( IntPtr box1, IntPtr box2, Vector ptB1, Vector ptB2 );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr GetCollisionPlane( Vector pt, IntPtr AABB, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetCollisionNormal( Vector pt, IntPtr AABB, out Vector normal, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructAABB( IntPtr boxptr );

        //BaseEntity
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitBaseEntity( IntPtr[] EntFaces, uint FaceLength, IntPtr transform, Vector mins, Vector maxs, IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr GetEntMatrix( IntPtr entptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr GetEntTransform( IntPtr entptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructBaseEntity( IntPtr entptr );

        //Camera
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitCamera( IntPtr transformptr, IntPtr perspectiveptr, IntPtr worldptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructCamera( IntPtr camptr );
    }
    public class BaseFace_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitBaseFace( uint VertLength, float[] vertices, uint IndLength, uint[] indices, IntPtr textureptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructBaseFace( IntPtr faceptr );
    }
    public class Brush_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitBrush( Vector mins, Vector maxs, IntPtr[] textures, uint TextureLength, IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructBrush( IntPtr brushptr );
    }
    public class Render_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetInputCallback( IntPtr fn );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetFlag( ref uint ToSet, uint flag, bool val );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void Init( out IntPtr window, out IntPtr shader, out IntPtr camera, out IntPtr world, out IntPtr inputdata );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void RenderLoop( IntPtr window, IntPtr shader, IntPtr camera, IntPtr world, bool bMouseControl );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void Terminate( IntPtr window, IntPtr shader, IntPtr camera, IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool ShouldTerminate( IntPtr window );
    }
    public class Texture_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitTexture( byte[] FilePath );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructTexture( IntPtr texptr );

        public static byte[] ToCString( string s )
        {
            return Encoding.UTF8.GetBytes( s );
        }
    }
    public class Transform_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitTransform( Vector position, Vector scale, IntPtr rotation );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void Updatetransform( IntPtr tptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void AddToPos( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetPos( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void AddToScale( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetScale( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void AddToRotation( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetRotation( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetRight( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetUp( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetForward( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void TransformDirection( IntPtr tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void TransformPoint( IntPtr tptr, ref Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void InverseTransformDirection( IntPtr tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void InverseTransformPoint( IntPtr tptr, ref Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructTransform( IntPtr tptr );
    }
    public class Matrix_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitMatrix( float[] values1, float[] values2, float[] values3, float[] values4 );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructMatrix( IntPtr mptr );
    }
    public class World_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitWorld();
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern uint AddEntToWorld( IntPtr world, IntPtr ent );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr GetEntAtWorldIndex( IntPtr world, uint index );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern uint GetWorldSize( IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructWorld( IntPtr wptr );
    }


    [StructLayout(LayoutKind.Explicit)] 
    public struct Vector
    {
        public Vector( float x, float y, float z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float z;
    }
}
