using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RenderInterface
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
        public static extern void SetInputCallback( IntPtr window, IntPtr fn );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetWindowMoveCallback( IntPtr window, IntPtr fn );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void SetMouseButtonCallback( IntPtr window, IntPtr fn );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void WindowSizeChanged( int width, int height );
    }

    public struct Point2
    {
        public Point2( float x, float y )
        {
            this.x = x;
            this.y = y;
        }
        public Point2( double x, double y )
        {
            this.x = (float) x;
            this.y = (float) y;
        }
        public float x;
        public float y;
    }
    public class Mouse
    {
        public static Point2 GetMouseOffset( IntPtr window )
        {
            GetMouseOffset( window, out double x, out double y );
            return new( x, y );
        }
        public static Point2 GetMouseNormalizedPos( IntPtr window )
        {
            GetMouseNormalizedPos( window, out double x, out double y );
            return new( x, y );
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetMouseOffset( IntPtr window, out double x, out double y );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GetMouseNormalizedPos( IntPtr window, out double x, out double y );
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

        public void Render( Shader shader ) => RenderMesh( shader, this );

        public byte[] ToBytes()
        {
            int Size = Marshal.SizeOf( typeof( FaceMesh ) );
            byte[] ret = new byte[ Size ];
            IntPtr Ptr = Marshal.AllocHGlobal( Size );
            Marshal.StructureToPtr( this, Ptr, false );
            Marshal.Copy( Ptr, ret, 0, Size );
            Marshal.FreeHGlobal( Ptr );
            return ret;
        }
        public static FaceMesh FromBytes( byte[] Bytes )
        {
            int Size = Marshal.SizeOf( typeof( FaceMesh ) );
            GCHandle Ptr = GCHandle.Alloc( Bytes, GCHandleType.Pinned );
            FaceMesh Mesh = Marshal.PtrToStructure<FaceMesh>( Ptr.AddrOfPinnedObject() );
            Ptr.Free();
            return Mesh;
        }

        //api init
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void InitMesh( int VertLength, float[] vertices, int IndLength, int[] indices, Texture textureptr, Vector Normal, out FaceMesh face );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void UpdateMesh( ref FaceMesh mesh );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DestructMesh( FaceMesh mesh );

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void RenderMesh( Shader shader, FaceMesh face );
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
                return i switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    3 => w,
                    _ => throw new ArgumentOutOfRangeException( nameof( i ), "i was out of range" )
                };
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

        public static explicit operator Vector4( (Vector, float) a ) => new( a.Item1.x, a.Item1.y, a.Item1.z, a.Item2 );
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

        public static Matrix IdentityMatrix() =>
        new( 
            new float[,] 
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            } 
        );

        public Vector GetRight() => new( Columns[ 0 ][ 0 ], Columns[ 1 ][ 0 ], Columns[ 2 ][ 0 ] );
        public Vector GetUp() => new( Columns[ 0 ][ 1 ], Columns[ 1 ][ 1 ], Columns[ 2 ][ 1 ] );
        public Vector GetForward() => new( -Columns[ 0 ][ 2 ], -Columns[ 1 ][ 2 ], -Columns[ 2 ][ 2 ] );

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

        public static Matrix Perspective( float fov, float aspect, float nearclip, float farclip )
        {
            GLMPerspective( fov, aspect, nearclip, farclip, out Matrix persp );
            return persp;
        }
        public static Matrix Ortho( float left, float right, float bottom, float top, float near, float far )
        {
            GLMOrtho( left, right, bottom, top, near, far, out Matrix persp );
            return persp;
        }
        public static Matrix RotMatrix( float degrees, Vector axis )
        {
            GLMRotMatrix( degrees, axis, out Matrix rot );
            return rot;
        }

        public static Matrix Scale( Vector s )
        {
            GLMScale( s, out Matrix scale );
            return scale;
        }
        public static Matrix Translate( Vector pt )
        {
            GLMTranslate( pt, out Matrix translation );
            return translation;
        }

        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMPerspective( float fov, float aspect, float nearclip, float farclip, out Matrix pMat );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMOrtho( float left, float right, float bottom, float top, float n, float f, out Matrix pMat );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMRotMatrix( float degrees, Vector axis, out Matrix pMat );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMMultiplyMatrix( ref Matrix pMultiply, Matrix multiplier );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMMultMatrixVector( Matrix matrix, ref Vector4 vector );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMInvertMatrix( ref Matrix matrix );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMScale( Vector s, out Matrix scale );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        private static extern void GLMTranslate( Vector pt, out Matrix translation );
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
                return i switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new ArgumentOutOfRangeException( nameof( i ), "Vector accessor out of range" )
                };
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
        public static Vector operator -( Vector a ) => new( -a.x, -a.y, -a.z );
        public static Vector operator +( Vector a, Vector b ) => new( a.x + b.x, a.y + b.y, a.z + b.z );
        public static Vector operator -( Vector a, Vector b ) => a + -b;
        public static Vector operator *( Vector a, float b ) => new( a.x * b, a.y * b, a.z * b );
        public static Vector operator *( float b, Vector a ) => a * b;
        public static Vector operator /( Vector a, float b ) => new( a.x / b, a.y / b, a.z / b );
        public static Vector operator /( float b, Vector a ) => a / b;
        public static Vector operator *( Vector a, Vector b ) => new( a.x * b.x, a.y * b.y, a.z * b.z );
        public static bool operator ==( Vector a, Vector b ) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=( Vector a, Vector b ) => !( a == b );
        public static float Dot( Vector a, Vector b ) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector Cross( Vector a, Vector b ) => new( a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x );
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
        public static explicit operator Vector( Vector4 v ) => new( v.x, v.y, v.z );
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct Plane
    {
        public Plane( Vector Normal, float Dist )
        {
            this.Normal = Normal;
            this.Dist = Dist;
        }
        public Plane( Vector pt1, Vector pt2, Vector pt3 )
        {
            pt2 -= pt1;
            pt3 -= pt1;
            Normal = Vector.Cross( pt2, pt3 );
            Dist = Vector.Dot( Normal, pt1 );
        }
        public Plane( Vector Normal, Vector PassThrough )
        {
            this.Normal = Normal.Normalized();
            Dist = Vector.Dot( this.Normal, PassThrough );
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
