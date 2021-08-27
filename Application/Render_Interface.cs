using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PhysEngine
{
    public class Renderer
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetFlag( ref uint ToSet, uint flag, bool val );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void Init( out IntPtr window );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void RenderLoop( IntPtr window, Shader shader, BaseEntity camera, Matrix perspective, BaseEntity[] EntsToRender, int EntToRenderLength );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void Terminate();
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern bool ShouldTerminate( IntPtr window );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetWindowSize( IntPtr window, out int x, out int y );
   
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern float GetTime();
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetInputCallback( IntPtr fn );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetWindowMoveCallback( IntPtr fn );
        
    }
    public class Mouse
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GetMouseOffset( IntPtr window, out double x, out double y );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void MoveMouseToCenter( IntPtr window );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void HideMouse( IntPtr window );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void ShowMouse( IntPtr window );
    }
    public class Util
    {
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void MakeRotMatrix( float degrees, Vector axis, out Matrix rot );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void MakePerspective( float fov, float aspect, float nearclip, float farclip, out Matrix persp );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void MultiplyMatrix( ref Matrix multiplied, Matrix multiplier );
        public static Matrix MultiplyMatrix( Matrix multiplied, Matrix multiplier )
        {
            MultiplyMatrix( ref multiplied, multiplier );
            return multiplied;
        }
        public static byte[] ToCString( string s )
        {
            return Encoding.UTF8.GetBytes( s );
        }
    }

    public class Player : EHandle
    {
        public static readonly BBox PLAYER_NORMAL_BBOX = new BBox( new Vector( -.5f, -1.5f, -.5f ), new Vector( .5f, .5f, .5f ) );
        public static readonly BBox PLAYER_CROUCH_BBOX = new BBox( new Vector( -.5f, -0.5f, -.5f ), new Vector( .5f, .5f, .5f ) );
        public Player( THandle transform, Matrix Perspective ) : base( new BaseFace[ 0 ], transform, PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs ) //persp is width/height
        {
            this.Perspective = Perspective;
            _crouched = false;
        }
        private bool _crouched;
        public Matrix Perspective;
        public void Crouch()
        {
            if ( !_crouched )
            {
                _crouched = true;
                this.AABB = new BHandle( PLAYER_CROUCH_BBOX );
            }
        }
        public void UnCrouch()
        {
            if ( _crouched )
            {
                _crouched = false;
                this.AABB = new BHandle( PLAYER_NORMAL_BBOX );
            }
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct BaseFace
    {
        public BaseFace( float[] vertices, int[] indices, Texture tex )
        {
            InitBaseFace( vertices.Length, vertices, indices.Length, indices, tex, out this );
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public float[] Verts;
        public int VertLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public int[] Inds;
        public int IndLength;

        public uint VBO;
        public uint VAO;
        public uint EBO;

        [MarshalAs(UnmanagedType.Struct)]
        public Texture texture;

        //member methods
        public float GetVertAtIndex( int i ) => GetVertAtIndex( this, i );
        public int GetIndAtIndex( int i ) => GetIndAtIndex( this, i );

        //api init
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void InitBaseFace( int VertLength, float[] vertices, int IndLength, int[] indices, Texture textureptr, out BaseFace face );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern float GetVertAtIndex( BaseFace face, int index );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern int GetIndAtIndex( BaseFace face, int index );
    }
    [StructLayout( LayoutKind.Sequential )]
    public struct Shader
    {
        public Shader( uint ID )
        {
            this.ID = ID;
        }
        public Shader( string VertPath, string FragPath )
        {
            InitShader( Util.ToCString( VertPath ), Util.ToCString( FragPath ), out Shader s );
            this.ID = s.ID;
        }
        public uint ID;

        public void SetAmbientLight( float f ) => SetAmbientLight( this, f );
        public void SetLights( Light[] pointlights ) => SetLights( this, pointlights, pointlights.Length );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitShader( byte[] VertPath, byte[] FragPath, out Shader s );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetLights( Shader s, Light[] pointlights, int lightlength );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetAmbientLight( Shader shader, float value );
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct Texture
    {
        public Texture( string filepath )
        {
            InitTexture( Util.ToCString( filepath ), out Texture tex );
            ID = tex.ID;
            Unit = tex.Unit;
        }

        public uint ID;
        public uint Unit;

        public void Close() => DestructTexture( ref this );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitTexture( byte[] FilePath, out Texture tex );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructTexture( ref Texture pTex );

    }
    public class TextureHandle
    {
        public TextureHandle( string filepath )
        {
            this.TextureName = filepath;
            this.texture = new Texture( filepath );
        }
        public Texture texture;
        public string TextureName;
    }
    [StructLayout( LayoutKind.Sequential )]
    public struct Vector4
    {
        public Vector4( float x, float y, float z, float w )
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public Vector4( params float[] values )
        {
            x = y = z = w = 0;
            if ( values.Length > 0 )
                this.x = values[ 0 ];
            if ( values.Length > 1 )
                this.y = values[ 1 ];
            if ( values.Length > 2 )
                this.z = values[ 2 ];
            if ( values.Length > 3 )
                this.w = values[ 3 ];
        }

        public float x;
        public float y;
        public float z;
        public float w;

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
                    case 3:
                        return w;
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
                    case 3:
                        w = value;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert( false, "tried to set vector element out of bounds" );
                        break;
                }
            }
        }

        public static implicit operator Vector4 ( Vector v ) => new Vector4( v.x, v.y, v.z, 1.0f );
    }
    [StructLayout( LayoutKind.Sequential )]
    public struct Matrix
    {
        public Matrix( float[,] values )
        {
            float[] values1 = new float[ 4 ] { values[ 0, 0 ], values[ 0, 1 ], values[ 0, 2 ], values[ 0, 3 ] };
            float[] values2 = new float[ 4 ] { values[ 1, 0 ], values[ 1, 1 ], values[ 1, 2 ], values[ 1, 3 ] };
            float[] values3 = new float[ 4 ] { values[ 2, 0 ], values[ 2, 1 ], values[ 2, 2 ], values[ 2, 3 ] };
            float[] values4 = new float[ 4 ] { values[ 3, 0 ], values[ 3, 1 ], values[ 3, 2 ], values[ 3, 3 ] };
            Columns = new Vector4[ 4 ];
            Columns[ 0 ] = new Vector4( values1 );
            Columns[ 1 ] = new Vector4( values2 );
            Columns[ 2 ] = new Vector4( values3 );
            Columns[ 3 ] = new Vector4( values4 );
        }
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public Vector4[] Columns;

        public static Matrix IdentityMatrix()
        {
            float[,] values =
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };
            return new Matrix( values );
        }

        public Vector GetRight()
        {
            return new Vector( Columns[ 0 ][ 0 ], Columns[ 1 ][ 0 ], Columns[ 2 ][ 0 ] );
        }
        public Vector GetUp()
        {
            return new Vector( Columns[ 0 ][ 1 ], Columns[ 1 ][ 1 ], Columns[ 2 ][ 1 ] );
        }
        public Vector GetForward()
        {
            return new Vector( -Columns[ 0 ][ 2 ], -Columns[ 1 ][ 2 ], -Columns[ 2 ][ 2 ] );
        }
    }


    [StructLayout( LayoutKind.Sequential )]
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
        public float x;
        public float y;
        public float z;

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
        public float LengthSqr() => x * x + y * y + z * z;
        public float Length() => (float) Math.Sqrt( LengthSqr() );
        public Vector Normalized() => this / Length();
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct Plane
    {
        public Plane( Vector Normal, float Dist )
        {
            this.Normal = Normal;
            this.Dist = Dist;
        }
        [MarshalAs( UnmanagedType.Struct )]
        public Vector Normal;
        public float Dist;

        //member functions
        public float DistanceFromPointToPlane( Vector pt )
        {
            return Vector.Dot( Normal, pt ) - Dist;
        }
        public Vector ClosestPointOnPlane( Vector pt )
        {
            return pt - DistanceFromPointToPlane( pt ) * Normal;
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct BaseEntity
    {
        public BaseEntity( BaseFace[] EntFaces, Transform transform, Vector mins, Vector maxs )
        {
            InitBaseEntity( EntFaces, EntFaces.Length, transform, mins, maxs, out this );
        }
        public BaseEntity( Vector mins, Vector maxs, Texture[] textures )
        {
            InitBrush( mins, maxs, textures, textures.Length, out this );
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public BaseFace[] EntFaces;
        public int FaceLength;

        
        [MarshalAs( UnmanagedType.Struct )]
        public Transform transform;
        [MarshalAs( UnmanagedType.Struct )]
        public BBox AABB;

        
        public BaseFace GetFaceAtIndex( int i )
        {
            GetBaseFaceAtIndex( this, out BaseFace ret, i );
            return ret;
        }

        public void Close() => DestructBaseEntity( ref this );


        //api methods
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitBaseEntity( BaseFace[] EntFaces, int FaceLength, Transform transform, Vector mins, Vector maxs, out BaseEntity b );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitBrush( Vector mins, Vector maxs, Texture[] textures, int TextureLength, out BaseEntity b );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetBaseFaceAtIndex( BaseEntity ent, out BaseFace face, int index );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructBaseEntity( ref BaseEntity ent );
    }
    public class EHandle
    {
        public EHandle( BaseFace[] EntFaces, THandle transform, Vector mins, Vector maxs )
        {
            _ent = new BaseEntity( EntFaces, transform.Data, mins, maxs );
            this.Transform = transform;
            this.AABB = new BHandle( _ent.AABB );
        }
        public EHandle( Vector mins, Vector maxs, Texture[] textures )
        {
            _ent = new BaseEntity( mins, maxs, textures );
            this.Transform = new THandle( _ent.transform );
            this.AABB = new BHandle( _ent.AABB );
        }
        public EHandle( BaseEntity CloneEnt )
        {
            _ent = CloneEnt;
        }
        
        private BaseEntity _ent;
        public BaseEntity ent 
        { 
            get 
            {
                _ent.transform = Transform.Data;
                _ent.AABB = AABB.Data;
                return _ent; 
            } 
        }

        public THandle Transform;
        public BHandle AABB;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct Transform
    {
        public Transform( Vector position, Vector scale, Matrix rotation )
        {
            InitTransform( position, scale, rotation, out Transform t );
            ThisToWorld = t.ThisToWorld;
            WorldToThis = t.WorldToThis;
            Position = t.Position;
            Scale = t.Scale;
            Rotation = t.Rotation;
        }
        [MarshalAs( UnmanagedType.Struct )]
        public Matrix ThisToWorld;
        [MarshalAs( UnmanagedType.Struct )]
        public Matrix WorldToThis;

        [MarshalAs( UnmanagedType.Struct )]
        public Vector Position;
        [MarshalAs( UnmanagedType.Struct )]
        public Vector Scale;
        [MarshalAs( UnmanagedType.Struct )]
        public Matrix Rotation;

        public Vector GetRight() { GetRight( this, out Vector v ); return v; }
        public Vector GetUp() { GetUp( this, out Vector v ); return v; }
        public Vector GetForward() { GetForward( this, out Vector v ); return v; }
        public Vector TransformDirection( Vector v ) { TransformDirection( this, ref v ); return v; }
        public Vector TransformPoint( Vector pt ) { TransformPoint( this, ref pt ); return pt; }
        public Vector InverseTransformDirection( Vector v ) { InverseTransformDirection( this, ref v ); return v; }
        public Vector InverseTransformPoint( Vector pt ) { InverseTransformPoint( this, ref pt ); return pt; }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void UpdateTransform( ref Transform t );
        [ DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitTransform( Vector position, Vector scale, Matrix rotation, out Transform t );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetRight( Transform tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetUp( Transform tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetForward( Transform tptr, out Vector v );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void TransformDirection( Transform tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void TransformPoint( Transform tptr, ref Vector pt );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InverseTransformDirection( Transform tptr, ref Vector dir );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InverseTransformPoint( Transform tptr, ref Vector pt );
    }
    public class THandle
    {
        public THandle( Vector position, Vector scale, Matrix rotation )
        {
            t = new Transform( position, scale, rotation );
        }
        public THandle( Transform tCopy )
        {
            t = tCopy;
        }

        private Transform t;
        public Transform Data
        { get => t; }

        public Vector Position
        { get { return t.Position; } set { t.Position = value; Transform.UpdateTransform( ref t ); } }
        public Vector Scale
        { get { return t.Scale; } set { t.Scale = value; Transform.UpdateTransform( ref t ); } }
        public Matrix Rotation
        { get { return t.Rotation; } set { t.Rotation = value; Transform.UpdateTransform( ref t ); } }

        public Vector GetRight() => t.GetRight();
        public Vector GetUp() => t.GetUp();
        public Vector GetForward() => t.GetForward();
        public Vector TransformDirection( Vector v ) => t.TransformDirection( v );
        public Vector TransformPoint( Vector pt ) => t.TransformPoint( pt );
        public Vector InverseTransformDirection( Vector v ) => t.InverseTransformDirection( v );
        public Vector InverseTransformPoint( Vector pt ) => t.InverseTransformPoint( pt );
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct BBox
    {
        public BBox( Vector mins, Vector maxs )
        {
            this.mins = mins;
            this.maxs = maxs;
        }
        [MarshalAs( UnmanagedType.Struct )]
        public Vector mins;
        [MarshalAs( UnmanagedType.Struct )]
        public Vector maxs;

        //member methods
        public bool TestCollisionAABB( BBox bbox, Vector ptThis, Vector ptB2 )
        {
            Vector ptWorldMins1 = this.mins + ptThis;
            Vector ptWorldMaxs1 = this.maxs + ptThis;
            Vector ptWorldMins2 = bbox.mins + ptB2;
            Vector ptWorldMaxs2 = bbox.maxs + ptB2;
            bool bCollisionX = ptWorldMins1.x <= ptWorldMaxs2.x && ptWorldMaxs1.x >= ptWorldMins2.x;
            bool bCollisionY = ptWorldMins1.y <= ptWorldMaxs2.y && ptWorldMaxs1.y >= ptWorldMins2.y;
            bool bCollisionZ = ptWorldMins1.z <= ptWorldMaxs2.z && ptWorldMaxs1.z >= ptWorldMins2.z;
            return bCollisionX && bCollisionY && bCollisionZ;
        }
        public bool TestCollisionPoint( Vector pt, Vector ptThis )
        {
            bool bShouldCollide = true;
            for ( int i = 0; i < 3; ++i )
                if ( !( pt[ i ] > mins[ i ] + ptThis[ i ] && pt[ i ] < maxs[ i ] + ptThis[ i ] ) )
                    bShouldCollide = false;
            return bShouldCollide;
        }
        public Plane GetCollisionPlane( Vector pt, Vector ptB )
        {
            Vector ptWorldMins = mins + ptB;
            Vector ptWorldMaxs = maxs + ptB;

            Plane[] planes =
            {
                new Plane( new Vector( 0, 0, 1 ), Vector.Dot( new Vector( 0, 0, 1 ), ptWorldMaxs ) ),
                new Plane( new Vector( 0, 0,-1 ), Vector.Dot( new Vector( 0, 0,-1 ), ptWorldMins ) ),
                new Plane( new Vector( 0, 1, 0 ), Vector.Dot( new Vector( 0, 1, 0 ), ptWorldMaxs ) ),
                new Plane( new Vector( 0,-1, 0 ), Vector.Dot( new Vector( 0,-1, 0 ), ptWorldMins ) ),
                new Plane( new Vector( 1, 0, 0 ), Vector.Dot( new Vector( 1, 0, 0 ), ptWorldMaxs ) ),
                new Plane( new Vector( -1, 0, 0 ), Vector.Dot( new Vector( -1, 0, 0 ), ptWorldMins ) ),
            };

            float[] fPlaneDists =
            {
                Vector.Dot( planes[ 0 ].Normal, pt ) - planes[ 0 ].Dist,
                Vector.Dot( planes[ 1 ].Normal, pt ) - planes[ 1 ].Dist,
                Vector.Dot( planes[ 2 ].Normal, pt ) - planes[ 2 ].Dist,
                Vector.Dot( planes[ 3 ].Normal, pt ) - planes[ 3 ].Dist,
                Vector.Dot( planes[ 4 ].Normal, pt ) - planes[ 4 ].Dist,
                Vector.Dot( planes[ 5 ].Normal, pt ) - planes[ 5 ].Dist,
            };

            float fMaxDist = fPlaneDists[ 0 ];
            int iMaxIndex = 0;
            for ( int i = 0; i < 6; ++i )
            {
                if ( fPlaneDists[ i ] > fMaxDist )
                {
                    iMaxIndex = i;
                    fMaxDist = fPlaneDists[ i ];
                }
            }

            return planes[ iMaxIndex ];
        }
        public Vector GetCollisionNormal( Vector pt, Vector ptB )
        {
            Plane p = GetCollisionPlane( pt, ptB );
            return p.Normal;
        }
    }
    public class BHandle
    {
        public BHandle( Vector mins, Vector maxs )
        {
            AABB = new BBox( mins, maxs );
        }
        public BHandle( BBox bCopy )
        {
            AABB = bCopy;
        }
        private BBox AABB;
        public BBox Data { get { return AABB; } }
        public Vector Mins
        { get { return AABB.mins; } set { AABB.mins = value; } }
        public Vector Maxs
        { get { return AABB.maxs; } set { AABB.maxs = value; } }
        public bool TestCollision( Vector ThisLocation, BHandle bOther,  Vector OtherLocation ) => AABB.TestCollisionAABB( bOther.AABB, ThisLocation, OtherLocation );
        public bool TestCollision( Vector pt, Vector ThisLocation ) => AABB.TestCollisionPoint( pt, ThisLocation );
        public Plane GetCollisionPlane( Vector pt, Vector ThisLocation ) => AABB.GetCollisionPlane( pt, ThisLocation );
        public Vector GetCollisionNormal( Vector pt, Vector ThisLocation ) => AABB.GetCollisionNormal( pt, ThisLocation );
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct Light
    {
        public Light( Vector4 Position, Vector4 Color, float Intensity )
        {
            this.Position = Position;
            this.Color = Color;
            this.Intensity = Intensity;
        }
        public Vector4 Position;
        public Vector4 Color;
        public float Intensity;
    }

}
