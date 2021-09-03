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
        public static extern void StartFrame( IntPtr window );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetCameraValues( Shader s, Matrix persp, Matrix CamWorldToThis );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetRenderValues( Shader s, Matrix m );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void RenderMesh( IntPtr window, Shader shader, FaceMesh face );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void EndFrame( IntPtr window );

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
        public static byte[] ToCString( string s )
        {
            return Encoding.UTF8.GetBytes( s );
        }
    }

    public class Player
    {
        public static readonly Vector EYE_CENTER_OFFSET = new Vector( 0, 0.5f, 0 );
        public static readonly BBox PLAYER_NORMAL_BBOX = new BBox( new Vector( -.5f, -1.0f, -.5f ), new Vector( .5f, 1.0f, .5f ) );
        public static readonly BBox PLAYER_CROUCH_BBOX = new BBox( new Vector( -.5f, -0.5f, -.5f ), new Vector( .5f, 0.5f, .5f ) );
        public static readonly Texture[] BLANK_TEXTURE = { new Texture() };
        //depending on how the compiler works, this may cause a memory leak. Prob won't though
        public static readonly FaceMesh[] PLAYER_NORMAL_FACES = new BoxEnt( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, BLANK_TEXTURE, Space.SELF ).Meshes;
        public static readonly FaceMesh[] PLAYER_CROUCH_FACES = new BoxEnt( PLAYER_CROUCH_BBOX.mins, PLAYER_CROUCH_BBOX.maxs, BLANK_TEXTURE, Space.SELF ).Meshes;
        public const float PLAYER_MASS = 50.0f;
        public const float PLAYER_ROTI = 1.0f;
        public Player( Matrix Perspective, Vector Gravity, Vector Coeffs, float Mass, float RotI )
        {
            this.Perspective = Perspective;
            _crouched = false;
            Body = new PhysicsObject( new BoxEnt( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, BLANK_TEXTURE, Space.SELF ), Gravity, Coeffs, Mass, RotI );
            Head = new BaseEntity( new FaceMesh[ 0 ], new Transform( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ) )
            {
                Parent = Body.LinkedEnt
            };
            Head.SetLocalOrigin( EYE_CENTER_OFFSET );
        }
        private bool _crouched;
        public Matrix Perspective;
        public PhysicsObject Body;
        public BaseEntity Head;
        public BaseEntity HeldEnt;
        public void Crouch()
        {
            if ( !_crouched )
            {
                _crouched = true;
                Body.LinkedEnt.Meshes = PLAYER_CROUCH_FACES;
                Head.SetLocalOrigin( new Vector() );
            }
        }
        public void UnCrouch()
        {
            if ( _crouched )
            {
                _crouched = false;
                Body.LinkedEnt.Meshes = PLAYER_NORMAL_FACES;
                Head.SetLocalOrigin( EYE_CENTER_OFFSET );
            }
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct FaceMesh
    {
        public FaceMesh( float[] vertices, int[] indices, Texture tex, Vector Normal ) :
            this( vertices, vertices.Length, indices, indices.Length, tex, Normal )
        {
        }
        public FaceMesh( float[] vertices, int vertlength, int[] indices, int indlength, Texture tex, Vector Normal )
        {
            InitMesh( vertlength, vertices, indlength, indices, tex, Normal, out this );
        }

        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 100 )]
        public float[] Verts;
        public int VertLength;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 50 )]
        public int[] Inds;
        public int IndLength;

        public uint VBO;
        public uint VAO;
        public uint EBO;

        [MarshalAs( UnmanagedType.Struct )]
        public Texture texture;

        [MarshalAs( UnmanagedType.Struct )]
        public Vector Normal;

        public Vector[] GetVerts()
        {
            Vector[] ret = new Vector[ VertLength / 5 ];
            int j = 0;
            for ( int i = 0; i < ret.Length; ++i )
            {
                ret[ i ] = new Vector( Verts[ j ], Verts[ j + 1 ], Verts[ j + 2 ] );
                j += 5;
            }
            return ret;
        }

        public void Update() => UpdateMesh( ref this );
        public void Close() => DestructMesh( this );

        //api init
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitMesh( int VertLength, float[] vertices, int IndLength, int[] indices, Texture textureptr, Vector Normal, out FaceMesh face );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void UpdateMesh( ref FaceMesh mesh );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructMesh( FaceMesh mesh );
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
            InitTexture( Util.ToCString( filepath ), out this );
        }

        public bool Initialized;
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
        public Vector4( Vector v, float w )
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.w = w;
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

        public static implicit operator Vector4( Vector v ) => new Vector4( v.x, v.y, v.z, 1.0f );
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
        public static Matrix InvertZMatrix()
        {
            float[,] values =
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0,-1, 0 },
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

        public static Matrix operator *( Matrix a, Matrix b )
        {
            GLMMultiplyMatrix( ref b, a );
            return b;
        }
        public static Matrix operator -( Matrix a )
        {
            GLMInvertMatrix( ref a );
            return a;
        }
        public static Vector4 operator *( Matrix a, Vector4 b )
        {
            GLMMultMatrixVector( a, ref b );
            return b;
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GLMPerspective( float fov, float aspect, float nearclip, float farclip, out Matrix pMat );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GLMRotMatrix( float degrees, Vector axis, out Matrix pMat );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMMultiplyMatrix( ref Matrix pMultiply, Matrix multiplier );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMMultMatrixVector( Matrix matrix, ref Vector4 vector );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMInvertMatrix( ref Matrix matrix );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GLMScale( Vector s, out Matrix scale );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void GLMTranslate( Vector pt, out Matrix translation );
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
        public override bool Equals( object obj ) => obj.GetType() == typeof( Vector ) && (Vector) obj == this;
        public override int GetHashCode() => Tuple.Create( x, y, z ).GetHashCode();
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
        public static Vector operator *( Vector a, Vector b ) => new Vector( a.x * b.x, a.y * b.y, a.z * b.z );
        public static bool operator ==( Vector a, Vector b ) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=( Vector a, Vector b ) => !( a == b );
        public static float Dot( Vector a, Vector b ) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector Cross( Vector a, Vector b ) => new Vector( a.y * b.z - a.z - b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x );
        public float LengthSqr() => x * x + y * y + z * z;
        public float Length() => 1 / FastInvSqrt( LengthSqr() );
        public Vector Normalized() => this * FastInvSqrt( LengthSqr() );
        private static unsafe float FastInvSqrt( float n )
        {
            int i;
            float x2, y;
            x2 = n * 0.5f;
            y = n;
            i = *(int*) &y;
            i = 0x5f3759df - ( i >> 1 );
            y = *(float*) &i;
            y *= ( 1.5f - ( x2 * y * y ) );
            return y;
        }
        public static explicit operator Vector( Vector4 v ) => new Vector( v.x, v.y, v.z );
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


    public class BBox
    {
        public BBox( Vector mins, Vector maxs )
        {
            this.mins = mins;
            this.maxs = maxs;
        }
        public Vector mins;
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
