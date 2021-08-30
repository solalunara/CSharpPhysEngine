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
        public static extern void RenderLoop( IntPtr window, Shader shader, Transform camera, Matrix perspective, BaseEntity[] EntsToRender, int EntToRenderLength );
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
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void MultiplyVector( Matrix matrix, ref Vector vector );
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void InvertMatrix( ref Matrix matrix );

        public static Matrix MultiplyMatrix( Matrix multiplied, Matrix multiplier )
        {
            MultiplyMatrix( ref multiplied, multiplier );
            return multiplied;
        }
        public static Vector MultiplyVector( Matrix matrix, Vector vector )
        {
            MultiplyVector( matrix, ref vector );
            return vector;
        }
        public static Matrix InvertMatrix( Matrix matrix )
        {
            InvertMatrix( ref matrix );
            return matrix;
        }
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
        public static readonly BaseFace[] PLAYER_NORMAL_FACES = new EHandle( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, BLANK_TEXTURE ).ent.EntFaces;
        public static readonly BaseFace[] PLAYER_CROUCH_FACES = new EHandle( PLAYER_CROUCH_BBOX.mins, PLAYER_CROUCH_BBOX.maxs, BLANK_TEXTURE ).ent.EntFaces;
        public const float PLAYER_MASS = 50.0f;
        public Player( Matrix Perspective, Vector Gravity, Vector Coeffs, float Mass )
        {
            this.Perspective = Perspective;
            _crouched = false;
            Body = new PhysicsObject( new EHandle( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, BLANK_TEXTURE ), Gravity, Coeffs, Mass );
            Head = new EHandle( new BaseFace[ 0 ], new THandle( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ), new Vector(), new Vector() );
            Head.Parent = Body.LinkedEnt;
            Head.Transform.SetLocalPos( EYE_CENTER_OFFSET );
        }
        private bool _crouched;
        public Matrix Perspective;
        public PhysicsObject Body;
        public EHandle Head;
        public void Crouch()
        {
            if ( !_crouched )
            {
                _crouched = true;
                Body.LinkedEnt.SetEntFaces( PLAYER_CROUCH_FACES );
                Body.LinkedEnt.SetEntBBox( PLAYER_CROUCH_BBOX );
                Head.Transform.SetLocalPos( new Vector() );
            }
        }
        public void UnCrouch()
        {
            if ( _crouched )
            {
                _crouched = false;
                Body.LinkedEnt.SetEntFaces( PLAYER_NORMAL_FACES );
                Body.LinkedEnt.SetEntBBox( PLAYER_NORMAL_BBOX );
                Head.Transform.SetLocalPos( EYE_CENTER_OFFSET );
            }
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct BaseFace
    {
        public BaseFace( float[] vertices, int[] indices, Texture tex, Vector Normal ) :
            this( vertices, vertices.Length, indices, indices.Length, tex, Normal )
        {
        }
        public BaseFace( float[] vertices, int vertlength, int[] indices, int indlength, Texture tex, Vector Normal )
        {
            InitBaseFace( vertlength, vertices, indlength, indices, tex, Normal, out this );
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

        //member methods
        public float GetVertAtIndex( int i ) => GetVertAtIndex( this, i );
        public int GetIndAtIndex( int i ) => GetIndAtIndex( this, i );
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

        //api init
        [DllImport( "render", CallingConvention = CallingConvention.Cdecl )]
        public static extern void InitBaseFace( int VertLength, float[] vertices, int IndLength, int[] indices, Texture textureptr, Vector Normal, out BaseFace face );
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
        public BaseEntity( BaseFace[] EntFaces, Transform transform, Vector mins, Vector maxs ) :
            this( EntFaces, EntFaces.Length, transform, mins, maxs )
        {
        }
        public BaseEntity( BaseFace[] EntFaces, int FaceLength, Transform transform, Vector mins, Vector maxs )
        {
            InitBaseEntity( EntFaces, FaceLength, transform, mins, maxs, out this );
        }
        public BaseEntity( Vector mins, Vector maxs, Texture[] textures )
        {
            InitBrush( mins, maxs, textures, textures.Length, out this );
        }

        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct )]
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
        }
        public EHandle( Vector mins, Vector maxs, Texture[] textures )
        {
            _ent = new BaseEntity( mins, maxs, textures );
            this.Transform = new THandle( _ent.transform );
        }
        public EHandle( BaseEntity CloneEnt )
        {
            _ent = CloneEnt;
        }


        protected BaseEntity _ent;
        public BaseEntity ent
        {
            get
            {
                _ent.transform = Transform.Data;
                return _ent;
            }
        }

        private EHandle _Parent;
        public EHandle Parent
        { 
            get 
            { 
                return _Parent; 
            } 
            set 
            {
                Vector AbsPos = Transform.Position;
                Transform.Parent = value.Transform;
                Transform.Position = AbsPos;
                _Parent = value; 
            } 
        }
        public THandle Transform;

        public Plane GetCollisionPlane( Vector pt )
        {
            Plane[] planes = new Plane[ _ent.FaceLength ];
            for ( int i = 0; i < planes.Length; ++i )
            {
                Vector WorldPoint = Transform.Position + ( new Vector( _ent.EntFaces[ i ].Verts[ 0 ], _ent.EntFaces[ i ].Verts[ 1 ], _ent.EntFaces[ i ].Verts[ 2 ] ) ) * Transform.Scale;
                planes[ i ] = new Plane( _ent.EntFaces[ i ].Normal, Vector.Dot( _ent.EntFaces[ i ].Normal, WorldPoint ) );
            }

            float[] PlaneDists = new float[ planes.Length ];
            for ( int i = 0; i < PlaneDists.Length; ++i )
            {
                PlaneDists[ i ] = Vector.Dot( planes[ i ].Normal, pt ) - planes[ i ].Dist;
            }

            float MaxDist = PlaneDists[ 0 ];
            int MaxIndex = 0;
            for ( int i = 0; i < PlaneDists.Length; ++i )
            {
                if ( PlaneDists[ i ] > MaxDist )
                {
                    MaxIndex = i;
                    MaxDist = PlaneDists[ i ];
                }
            }

            return planes[ MaxIndex ];
        }
        public Vector GetCollisionNormal( Vector pt )
        {
            return GetCollisionPlane( pt ).Normal;
        }
        public Vector[] GetVerts()
        {
            HashSet<Vector> Verts = new HashSet<Vector>();
            for ( int i = 0; i < _ent.FaceLength; ++i )
            {
                Verts.UnionWith( _ent.EntFaces[ i ].GetVerts() );
            }
            return Verts.ToArray();
        }
        public Vector[] GetWorldVerts()
        {
            Vector[] ret = GetVerts();
            for ( int i = 0; i < ret.Length; ++i )
            {
                ret[i] = Transform.TransformPoint( ret[ i ] );
            }
            return ret;
        }

        public void SetEntFaces( BaseFace[] Faces )
        {
            System.Diagnostics.Debug.Assert( Faces.Length == 20 );
            _ent.EntFaces = Faces;
        }
        public void SetEntBBox( BBox b )
        {
            _ent.AABB = b;
        }

        public bool TestCollision( Vector pt )
        {
            Vector[] Points1 = GetWorldVerts();
            Vector[] Points2 = { pt };
            for ( int i = 0; i < ent.FaceLength; ++i )
            {
                if ( !TestCollision( ent.EntFaces[ i ].Normal, Points1, Points2 ) )
                    return false;
            }
            return true;
        }
        public static bool TestCollision( params EHandle[] ents )
        {
            System.Diagnostics.Debug.Assert( ents.Length == 2 );

            Vector[] Points1 = ents[ 0 ].GetWorldVerts();
            Vector[] Points2 = ents[ 1 ].GetWorldVerts();

            for ( int EntIndex = 0; EntIndex < 2; ++EntIndex )
            {
                EHandle Ent = ents[ EntIndex ];
                for ( int i = 0; i < Ent.ent.FaceLength; ++i )
                {
                    if ( !TestCollision( Ent.ent.EntFaces[ i ].Normal, Points1, Points2 ) )
                        return false;
                }
            }
            return true;
        }
        private static bool TestCollision( Vector Normal, Vector[] Points1, Vector[] Points2 )
        {
            if ( Points1.Length == 0 || Points2.Length == 0 )
                return false;

            float[] ProjectedPoints1 = new float[ Points1.Length ];
            float[] ProjectedPoints2 = new float[ Points2.Length ];

            for ( int i = 0; i < Points1.Length; ++i )
                ProjectedPoints1[ i ] = Vector.Dot( Points1[ i ], Normal );
            for ( int i = 0; i < Points2.Length; ++i )
                ProjectedPoints2[ i ] = Vector.Dot( Points2[ i ], Normal );

            float Small1 = ProjectedPoints1.Min();
            float Large1 = ProjectedPoints1.Max();

            float Small2 = ProjectedPoints2.Min();
            float Large2 = ProjectedPoints2.Max();

            if ( Small1 > Large2 || Small2 > Large1 )
                return false;

            return true;
        }
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
        { 
            get
            {
                return new Transform( Position, Scale, Rotation );
            }
        }

        public THandle Parent;

        public Vector Position
        { 
            get 
            {
                if ( Parent != null )
                    return Parent.TransformPoint( t.Position );
                else
                    return t.Position; 
            } 
            set 
            {
                if ( Parent != null )
                    t.Position = Parent.InverseTransformPoint( value );
                else
                    t.Position = value; 
                Transform.UpdateTransform( ref t ); 
            } 
        }
        public void SetLocalPos( Vector pt )
        {
            t.Position = pt;
        }
        public Vector GetLocalPos() => t.Position;
        public Vector Scale
        { 
            get 
            {
                return t.Scale; 
            } 
            set 
            { 
                t.Scale = value; 
                Transform.UpdateTransform( ref t ); 
            } 
        }
        public Matrix Rotation
        { 
            get 
            {
                if ( Parent != null )
                    return Util.MultiplyMatrix( Parent.Rotation, t.Rotation );
                else
                    return t.Rotation; 
            } 
            set 
            {
                if ( Parent != null )
                    t.Rotation = Util.MultiplyMatrix( value, Util.InvertMatrix( Parent.Rotation ) );
                else
                    t.Rotation = value;
                Transform.UpdateTransform( ref t );
            } 
        }
        public void SetLocalRot( Matrix r )
        {
            t.Rotation = r;
        }
        public Matrix GetLocalRot() => t.Rotation;

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
