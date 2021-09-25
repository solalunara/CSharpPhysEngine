using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    class BoxEnt : BaseEntity
    {
        public BoxEnt( Vector mins, Vector maxs, Texture[] tx, bool NormalizeBox = true ) :
            base( new FaceMesh[ 6 ], new Transform( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ) )
        {
            if ( NormalizeBox )
            {
                LocalTransform.Position = ( mins + maxs ) / 2;
                mins -= LocalTransform.Position;
                maxs -= LocalTransform.Position;
            }

            AABB = new BBox( mins, maxs );

            int[] inds =
            {
                0, 1, 3,
                1, 2, 3
            };

            float[] ZMins =
            {
                mins.x, mins.y, mins.z, 1.0f, 0.0f,
                maxs.x, mins.y, mins.z, 0.0f, 0.0f,
                maxs.x, maxs.y, mins.z, 0.0f, 1.0f,
                mins.x, maxs.y, mins.z, 1.0f, 1.0f,
            };
            float[] ZMaxs =
            {
                mins.x, mins.y, maxs.z, 0.0f, 0.0f,
                maxs.x, mins.y, maxs.z, 1.0f, 0.0f,
                maxs.x, maxs.y, maxs.z, 1.0f, 1.0f,
                mins.x, maxs.y, maxs.z, 0.0f, 1.0f,
            };
            float[] YMins =
            {
                mins.x, mins.y, mins.z, 0.0f, 0.0f,
                maxs.x, mins.y, mins.z, 1.0f, 0.0f,
                maxs.x, mins.y, maxs.z, 1.0f, 1.0f,
                mins.x, mins.y, maxs.z, 0.0f, 1.0f,
            };
            float[] YMaxs =
            {
                mins.x, maxs.y, mins.z, 1.0f, 0.0f,
                maxs.x, maxs.y, mins.z, 0.0f, 0.0f,
                maxs.x, maxs.y, maxs.z, 0.0f, 1.0f,
                mins.x, maxs.y, maxs.z, 1.0f, 1.0f,
            };
            float[] XMins =
            {
                mins.x, mins.y, mins.z, 0.0f, 0.0f,
                mins.x, maxs.y, mins.z, 0.0f, 1.0f,
                mins.x, maxs.y, maxs.z, 1.0f, 1.0f,
                mins.x, mins.y, maxs.z, 1.0f, 0.0f,
            };
            float[] XMaxs =
            {
                maxs.x, mins.y, mins.z, 1.0f, 0.0f,
                maxs.x, maxs.y, mins.z, 1.0f, 1.0f,
                maxs.x, maxs.y, maxs.z, 0.0f, 1.0f,
                maxs.x, mins.y, maxs.z, 0.0f, 0.0f,
            };

            float[][] Verts = { ZMins, ZMaxs, YMins, YMaxs, XMins, XMaxs };

            Vector[] Normals =
            {
                new Vector( 0, 0,-1 ),
                new Vector( 0, 0, 1 ),
                new Vector( 0,-1, 0 ),
                new Vector( 0, 1, 0 ),
                new Vector(-1, 0, 0 ),
                new Vector( 1, 0, 0 )
            };

            switch ( tx.Length )
            {
                case 0:
                    for ( int i = 0; i < 6; ++i )
                        Meshes[ i ] = new FaceMesh( Verts[ i ], inds, new Texture(), Normals[ i ] );
                    break;
                case 1:
                    for ( int i = 0; i < 6; ++i )
                        Meshes[ i ] = new FaceMesh( Verts[ i ], inds, tx[ 0 ], Normals[ i ] );
                    break;
                case 6:
                    for ( int i = 0; i < 6; ++i )
                        Meshes[ i ] = new FaceMesh( Verts[ i ], inds, tx[ i ], Normals[ i ] );
                    break;
                default:
                    Assert( false );
                    break;
            }


        }

        public BBox AABB;
    }

    class Camera : BaseEntity
    {
        public Camera( Matrix Perspective ) : base( Array.Empty<FaceMesh>(), new( new(), new( 1, 1, 1 ), Matrix.IdentityMatrix() ) )
        {
            this.Perspective = Perspective;
        }
        public Matrix Perspective;
    }

    abstract class Player
    {
        public Camera camera;
        public PhysObj Body;
    }

    class Player2D : Player
    {
        public Player2D()
        {
            Body = new PhysObj( new Dim2Box( new( -.05f, -.1f ), new( 0.05f, 0.1f ), new() ), PhysObj.Default_Coeffs, 50, float.PositiveInfinity, new() );
            camera = new( Matrix.Ortho( -10.0f, 10.0f, -10.0f, 10.0f, 0.01f, 1000.0f ) )
            {
                Parent = Body.LinkedEnt
            };
            camera.SetLocalOrigin( new( 0, 0, 10 ) );
        }
    }
    class Player3D : Player
    {
        public static readonly Vector EYE_CENTER_OFFSET = new( 0, 0.5f, 0 );
        public static readonly BBox PLAYER_NORMAL_BBOX = new( new Vector( -.5f, -1.0f, -.5f ), new Vector( .5f, 1.0f, .5f ) );
        public static readonly BBox PLAYER_CROUCH_BBOX = new( new Vector( -.5f, -0.5f, -.5f ), new Vector( .5f, 0.5f, .5f ) );
        //depending on how the compiler works, this may cause a memory leak. Prob won't though
        public static readonly FaceMesh[] PLAYER_NORMAL_FACES = new BoxEnt( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, Array.Empty<Texture>() ).Meshes;
        public static readonly FaceMesh[] PLAYER_CROUCH_FACES = new BoxEnt( PLAYER_CROUCH_BBOX.mins, PLAYER_CROUCH_BBOX.maxs, Array.Empty<Texture>() ).Meshes;
        public const float PLAYER_MASS = 50.0f;
        public const float PLAYER_ROTI = float.PositiveInfinity;

        public Player3D( Matrix Perspective, Vector Coeffs, float Mass, float RotI )
        {
            _crouched = false;
            Body = new PhysObj( new BoxEnt( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, Array.Empty<Texture>() ), Coeffs, Mass, RotI, new() );
            camera = new Camera( Perspective )
            {
                Parent = Body.LinkedEnt
            };
            camera.SetLocalOrigin( EYE_CENTER_OFFSET );
        }

        private bool _crouched;
        public BaseEntity HeldEnt;
        public void Crouch()
        {
            if ( !_crouched )
            {
                _crouched = true;
                Body.LinkedEnt.Meshes = PLAYER_CROUCH_FACES;
                camera.SetLocalOrigin( new Vector() );
            }
        }
        public void UnCrouch()
        {
            if ( _crouched )
            {
                _crouched = false;
                Body.LinkedEnt.Meshes = PLAYER_NORMAL_FACES;
                camera.SetLocalOrigin( EYE_CENTER_OFFSET );
            }
        }
    }

    class BBox
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
                new(new Vector(0, 0, -1), Vector.Dot(new Vector(0, 0, -1), ptWorldMins)),
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
}
