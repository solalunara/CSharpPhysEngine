using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Application
{
    public class Plane
    {
        public Plane( Vector normal, float dist )
        {
            _LinkedPlane = InitPlane( normal, dist );
        }
        public Plane( IntPtr plane )
        {
            _LinkedPlane = plane;
        }
        ~Plane()
        {
            DestructPlane( _LinkedPlane );
        }

        private IntPtr _LinkedPlane;
        public IntPtr LinkedPlane
        {
            get => _LinkedPlane;
            set { if ( _LinkedPlane != IntPtr.Zero ) DestructPlane( _LinkedPlane ); _LinkedPlane = value; }
        }

        public Vector Normal
        { get { GetPlaneVals( LinkedPlane, out Vector v, out _ ); return v; } }
        public float Dist
        { get { GetPlaneVals( LinkedPlane, out _, out float d ); return d; } }

        public float PointPlaneDist( Vector pt )
        { return DistanceFromPointToPlane( LinkedPlane, pt ); }
        public Vector ClosestPointOnPlane( Vector pt )
        { ClosestPointOnPlane( LinkedPlane, pt, out Vector ptp ); return ptp; }

        //api methods
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitPlane( Vector vNormal, float fDist );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetPlaneVals( IntPtr Plane, out Vector Normal, out float Dist );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern float DistanceFromPointToPlane( IntPtr plane, Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void ClosestPointOnPlane( IntPtr plane, Vector point, out Vector PtOnPlane );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructPlane( IntPtr plane );
    }
    public class BaseFace
    {
        public BaseFace( float[] vertices, uint[] indices, Texture tex )
        {
            _LinkedFace = InitBaseFace( (uint) vertices.Length, vertices, (uint) indices.Length, indices, tex.LinkedTexture );
        }
        public BaseFace( IntPtr baseface )
        {
            _LinkedFace = baseface;
        }
        ~BaseFace()
        {
            DestructBaseFace( _LinkedFace );
        }

        private IntPtr _LinkedFace;
        public IntPtr LinkedFace
        {
            get => _LinkedFace;
            set { if ( _LinkedFace != IntPtr.Zero ) DestructBaseFace( _LinkedFace ); _LinkedFace = value; }
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitBaseFace( uint VertLength, float[] vertices, uint IndLength, uint[] indices, IntPtr textureptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructBaseFace( IntPtr faceptr );
    }
    public class Render_Interface
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern float GetTime();
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
    public class Texture
    {
        public Texture( string filepath )
        {
            _LinkedTexture = InitTexture( ToCString( filepath ) );
        }
        public Texture( IntPtr texture )
        {
            _LinkedTexture = texture;
        }
        ~Texture()
        {
            DestructTexture( _LinkedTexture );
        }

        private IntPtr _LinkedTexture;
        public IntPtr LinkedTexture
        {
            get => _LinkedTexture;
            set { if ( _LinkedTexture != IntPtr.Zero ) DestructTexture( _LinkedTexture ); _LinkedTexture = value; }
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitTexture( byte[] FilePath );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructTexture( IntPtr texptr );

        public static byte[] ToCString( string s )
        {
            return Encoding.UTF8.GetBytes( s );
        }
    }
    public class Matrix
    {
        public Matrix( float[,] values )
        {
            float[] values1 = new float[ 4 ] { values[ 0, 0 ], values[ 0, 1 ], values[ 0, 2 ], values[ 0, 3 ] };
            float[] values2 = new float[ 4 ] { values[ 1, 0 ], values[ 1, 1 ], values[ 1, 2 ], values[ 1, 3 ] };
            float[] values3 = new float[ 4 ] { values[ 2, 0 ], values[ 2, 1 ], values[ 2, 2 ], values[ 2, 3 ] };
            float[] values4 = new float[ 4 ] { values[ 3, 0 ], values[ 3, 1 ], values[ 3, 2 ], values[ 3, 3 ] };
            _LinkedMatrix = InitMatrix( values1, values2, values3, values4 );
        }
        public Matrix( IntPtr matrix )
        {
            _LinkedMatrix = matrix;
        }
        ~Matrix()
        {
            DestructMatrix( _LinkedMatrix );
        }

        private IntPtr _LinkedMatrix;
        public IntPtr LinkedMatrix
        {
            get => _LinkedMatrix;
            set { if ( _LinkedMatrix != IntPtr.Zero ) DestructMatrix( _LinkedMatrix ); _LinkedMatrix = value; }
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitMatrix( float[] values1, float[] values2, float[] values3, float[] values4 );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructMatrix( IntPtr mptr );
    }
    public class World
    {
        public World( IntPtr world )
        {
            _LinkedWorld = world;
        }
        ~World()
        {
            DestructWorld( _LinkedWorld );
        }

        private IntPtr _LinkedWorld;
        public IntPtr LinkedWorld
        {
            get => _LinkedWorld;
            set { if ( _LinkedWorld != IntPtr.Zero ) DestructWorld( _LinkedWorld ); _LinkedWorld = value; }
        }

        public uint Size
        { get { return GetWorldSize( LinkedWorld ); } }
        public IntPtr GetEntAtIndex( int i )
        { return GetEntAtWorldIndex( LinkedWorld, (uint) i ); }
        public void AddEntToWorld( BaseEntity b )
        { AddEntToWorld( LinkedWorld, b.LinkedEnt ); }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitWorld();
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern uint AddEntToWorld( IntPtr world, IntPtr ent );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetEntAtWorldIndex( IntPtr world, uint index );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern uint GetWorldSize( IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructWorld( IntPtr wptr );
    }


    [StructLayout( LayoutKind.Explicit )]
    public struct Vector
    {
        public Vector( float x, float y, float z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector( Vector v )
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
        [FieldOffset( 0 )] public float x;
        [FieldOffset( 4 )] public float y;
        [FieldOffset( 8 )] public float z;

        public override string ToString() => "x: " + x + " y: " + y + " z: " + z;
        public float this[ int i ]
        {
            get
            {
                switch ( i )
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        System.Diagnostics.Debug.Assert( false, "tried to access vector element out of bounds" );
                        return 0;
                }
            }
            set
            {
                switch ( i )
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert( false, "tried to set vector element out of bounds" );
                        break;
                }
            }
        }
        public static Vector operator +( Vector a ) => a;
        public static Vector operator -( Vector a ) => new Vector( -a.x, -a.y, -a.z );
        public static Vector operator +( Vector a, Vector b ) => new Vector( a.x + b.x, a.y + b.y, a.z + b.z );
        public static Vector operator -( Vector a, Vector b ) => a + -b;
        public static Vector operator *( Vector a, float b ) => new Vector( a.x * b, a.y * b, a.z * b );
        public static Vector operator *( float b, Vector a ) => a * b;
        public static Vector operator /( Vector a, float b ) => new Vector( a.x / b, a.y / b, a.z / b );
        public static Vector operator /( float b, Vector a ) => a / b;
        public static float Dot( Vector a, Vector b ) => a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public class BaseEntity : IDisposable
    {
        //auto-linking
        public BaseEntity( BaseFace[] EntFaces, Transform transform, Vector mins, Vector maxs, World world )
        {
            IntPtr[] EntFacePtrs = new IntPtr[ EntFaces.Length ];
            for ( int i = 0; i < EntFacePtrs.Length; ++i )
                EntFacePtrs[ i ] = EntFaces[ i ].LinkedFace;

            _LinkedEnt = InitBaseEntity( EntFacePtrs, (uint) EntFaces.Length, transform.LinkedTransform, mins, maxs, world.LinkedWorld );
            this.Brush = false;
            this.Player = false;
        }
        //manual-linking
        public BaseEntity( bool IsBrush, bool IsPlayer, IntPtr LinkedEnt )
        {
            this.Brush = IsBrush;
            this.Player = IsPlayer;
            _LinkedEnt = LinkedEnt;
        }
        
        ~BaseEntity()
        {
            if ( !Brush && !Player && _LinkedEnt != IntPtr.Zero ) //brushes and players have their own destructor handling
            {
                DestructBaseEntity( _LinkedEnt );
                _LinkedEnt = IntPtr.Zero;
            }
        }

        //api methods
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitBaseEntity( IntPtr[] EntFaces, uint FaceLength, IntPtr transform, Vector mins, Vector maxs, IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetEntTransform( IntPtr entptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetEntBBox( IntPtr entptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetEntWorld( IntPtr entptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructBaseEntity( IntPtr entptr );

        private bool Brush;
        private bool Player;
        private IntPtr _LinkedEnt;
        //care must be taken to ensure that if the linked entity is set to a new entity, the previous one is destructed
        public IntPtr LinkedEnt 
        { 
            get { return _LinkedEnt; } 
            set { if ( _LinkedEnt != IntPtr.Zero ) DestructBaseEntity( _LinkedEnt ); _LinkedEnt = value; } 
        }

        public Transform Transform { get { return new Transform( GetEntTransform( LinkedEnt ) ); } }
        public BBox BBox { get { return new BBox( GetEntBBox( LinkedEnt ) ); } }
        public World World { get { return new World( GetEntWorld( LinkedEnt ) ); } }

        public bool IsBrush()
        {
            return Brush;
        }
        public bool IsPlayer()
        {
            return Player;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    public class Brush : BaseEntity
    {
        public Brush( Vector mins, Vector maxs, Texture[] textures, World world ) : base( true, false, IntPtr.Zero )
        {
            IntPtr[] TexPtrs = new IntPtr[ textures.Length ];
            for ( int i = 0; i < TexPtrs.Length; ++i )
                TexPtrs[ i ] = textures[ i ].LinkedTexture;

            LinkedEnt = InitBrush( mins, maxs, TexPtrs, (uint) textures.Length, world.LinkedWorld );
        }
        ~Brush()
        {
            DestructBrush( LinkedEnt );
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr InitBrush( Vector mins, Vector maxs, IntPtr[] textures, uint TextureLength, IntPtr world );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void DestructBrush( IntPtr brushptr );
    }
    public class Player : BaseEntity
    {
        public Player( Transform transform, World world ) : base( false, true, IntPtr.Zero )
        {
            IntPtr perspective = MakePerspective( 45, 1, 0.01f, 5000f );
            LinkedEnt = InitCamera( transform.LinkedTransform, perspective, world.LinkedWorld );
        }
        public Player( IntPtr player ) : base( false, true, player )
        {
        }
        ~Player()
        {
            DestructCamera( LinkedEnt );
        }


        //Camera
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr MakePerspective( float fov, float aspect, float nearclip, float farclip );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitCamera( IntPtr transformptr, IntPtr perspectiveptr, IntPtr worldptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructCamera( IntPtr camptr );
    }
    public class Transform
    {
        public Transform( Vector position, Vector scale, Matrix rotation )
        {
            _LinkedTransform = InitTransform( position, scale, rotation.LinkedMatrix );
        }
        public Transform( IntPtr transform )
        {
            _LinkedTransform = transform;
        }

        private IntPtr _LinkedTransform;
        public IntPtr LinkedTransform 
        { 
            get { return _LinkedTransform; } 
            set { _LinkedTransform = value; }
        }

        public Vector Position
        {
            get { GetTransformVals( LinkedTransform, out Vector pos, out _, out _ ); return pos; }
        }
        public Vector Scale
        {
            get { GetTransformVals( LinkedTransform, out _, out Vector scl, out _ ); return scl; }
        }
        public Matrix Rotation
        {
            get { GetTransformVals( LinkedTransform, out _, out _, out IntPtr rot ); return new Matrix( rot ); }
        }

        public Matrix WorldToThis
        { get => new Matrix( GetWorldToThis( LinkedTransform ) ); }
        public Matrix ThisToWorld
        { get => new Matrix( GetThisToWorld( LinkedTransform ) ); }

        public void AddPosition( Vector v ) { AddToPos( LinkedTransform, v ); }
        public void SetPosition( Vector v ) { SetPos( LinkedTransform, v ); }
        public void AddScale( Vector v ) { AddToScale( LinkedTransform, v ); }
        public void SetScale( Vector v ) { SetScale( LinkedTransform, v ); }
        public void Rotate( Matrix r ) { AddToRotation( LinkedTransform, r.LinkedMatrix ); }
        public void SetRotation( Matrix r ) { SetRotation( LinkedTransform, r.LinkedMatrix ); }
        public Vector GetRight() { GetRight( LinkedTransform, out Vector v ); return v; }
        public Vector GetUp() { GetUp( LinkedTransform, out Vector v ); return v; }
        public Vector GetForward() { GetForward( LinkedTransform, out Vector v ); return v; }
        public Vector TransformDirection( Vector v ) { TransformDirection( LinkedTransform, ref v ); return v; }
        public Vector TransformPoint( Vector pt ) { TransformPoint( LinkedTransform, ref pt ); return pt; }
        public Vector InverseTransformDirection( Vector v ) { InverseTransformDirection( LinkedTransform, ref v ); return v; }
        public Vector InverseTransformPoint( Vector pt ) { InverseTransformPoint( LinkedTransform, ref pt ); return pt; }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitTransform( Vector position, Vector scale, IntPtr rotation );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetTransformVals( IntPtr transform, out Vector position, out Vector scale, out IntPtr rotation );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetThisToWorld( IntPtr tptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetWorldToThis( IntPtr tptr );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void AddToPos( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetPos( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void AddToScale( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetScale( IntPtr tptr, Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void AddToRotation( IntPtr tptr, IntPtr rot );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetRotation( IntPtr tptr, IntPtr rot );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetRight( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetUp( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetForward( IntPtr tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void TransformDirection( IntPtr tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void TransformPoint( IntPtr tptr, ref Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InverseTransformDirection( IntPtr tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InverseTransformPoint( IntPtr tptr, ref Vector pt );
    }
    public class BBox
    {
        public BBox( Vector mins, Vector maxs )
        {
            _LinkedBBox = InitAABB( mins, maxs );
        }
        public BBox( IntPtr bbox )
        {
            _LinkedBBox = bbox;
        }
        ~BBox()
        {
            DestructAABB( _LinkedBBox );
        }

        private IntPtr _LinkedBBox;
        public IntPtr LinkedBBox
        {
            get { return _LinkedBBox; }
            set { if ( _LinkedBBox != IntPtr.Zero ) DestructAABB( _LinkedBBox ); _LinkedBBox = value; }
        }

        public Vector mins
        { get { GetAABBPoints( LinkedBBox, out Vector mins, out _ ); return mins; } }
        public Vector maxs
        { get { GetAABBPoints( LinkedBBox, out _, out Vector maxs ); return maxs; } }

        public bool TestPoint( Vector pt, Vector BBoxLocation )
        { return TestCollisionPoint( pt, LinkedBBox, BBoxLocation ); }
        public bool TestCollisionAABB( BBox bOther, Vector BBoxLocationThis, Vector BBoxLocationOther )
        { return TestCollisionAABB( LinkedBBox, bOther.LinkedBBox, BBoxLocationThis, BBoxLocationOther ); }

        public Plane GetCollisionPlane( Vector pt, Vector BBoxLocation )
        { return new Plane( GetCollisionPlane( pt, LinkedBBox, BBoxLocation ) ); }
        public Vector GetCollisionNormal( Vector pt, Vector BBoxLocation )
        { GetCollisionNormal( pt, LinkedBBox, out Vector v, BBoxLocation ); return v; }

        //api methods
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr InitAABB( Vector mins, Vector maxs );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetAABBPoints( IntPtr bbox, out Vector mins, out Vector maxs );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool TestCollisionPoint( Vector pt, IntPtr AABB, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool TestCollisionAABB( IntPtr box1, IntPtr box2, Vector ptB1, Vector ptB2 );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetCollisionPlane( Vector pt, IntPtr AABB, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetCollisionNormal( Vector pt, IntPtr AABB, out Vector normal, Vector ptB );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructAABB( IntPtr boxptr );
    }
}
