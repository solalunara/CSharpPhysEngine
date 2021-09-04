using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    public class BaseEntity
    {
        public BaseEntity( FaceMesh[] Meshes, Transform LocalTransform )
        {
            this.Meshes = Meshes;
            this.LocalTransform = LocalTransform;
        }

        public void Close()
        {
            foreach ( FaceMesh m in Meshes )
            {
                m.Close();
            }
        }

        public FaceMesh[] Meshes;
        public Transform LocalTransform;

        private BaseEntity _Parent;
        public BaseEntity Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                Vector AbsPos = GetAbsOrigin();
                Matrix AbsRot = GetAbsRot();
                _Parent = value;
                SetAbsOrigin( AbsPos );
                SetAbsRot( AbsRot );
            }
        }

        public void SetLocalOrigin( Vector pt ) => LocalTransform.Position = pt;
        public void SetLocalRot( Matrix r ) => LocalTransform.Rotation = r;
        public void SetLocalScale( Vector s ) => LocalTransform.Scale = s;

        public Vector GetLocalOrigin() => LocalTransform.Position;
        public Matrix GetLocalRot() => LocalTransform.Rotation;
        public Vector GetLocalScale() => LocalTransform.Scale;

        public void SetAbsOrigin( Vector pt )
        {
            if ( Parent != null )
                LocalTransform.Position = Parent.InverseTransformPoint( pt );
            else
                LocalTransform.Position = pt;
        }
        public void SetAbsRot( Matrix r )
        {
            if ( Parent != null )
                LocalTransform.Rotation = -Parent.GetAbsRot() * r;
            else
                LocalTransform.Rotation = r;
        }
        public Vector GetAbsOrigin()
        {
            if ( Parent != null )
                return Parent.TransformPoint( LocalTransform.Position );
            else
                return LocalTransform.Position;
        }
        public Matrix GetAbsRot()
        {
            if ( Parent != null )
                return Parent.GetAbsRot() * LocalTransform.Rotation;
            else
                return LocalTransform.Rotation;
        }


        public Matrix CalcEntMatrix()
        {
            if ( Parent != null )
                return Parent.CalcEntMatrix() * LocalTransform.ThisToWorld;
            return LocalTransform.ThisToWorld;
        }

        public Vector TransformDirection( Vector dir ) => (Vector) ( CalcEntMatrix() * new Vector4( dir, 0.0f ) );
        public Vector TransformPoint( Vector pt ) => (Vector) ( CalcEntMatrix() * new Vector4( pt, 1.0f ) );
        public Vector InverseTransformDirection( Vector dir ) => (Vector) ( -CalcEntMatrix() * new Vector4( dir, 0.0f ) );
        public Vector InverseTransformPoint( Vector pt ) => (Vector) ( -CalcEntMatrix() * new Vector4( pt, 1.0f ) );


        public Plane GetCollisionPlane( Vector pt )
        {
            Plane[] planes = new Plane[ Meshes.Length ];
            for ( int i = 0; i < planes.Length; ++i )
            {
                Vector WorldPoint = (Vector) ( CalcEntMatrix() * Meshes[ i ].GetVerts()[ 0 ] );
                planes[ i ] = new Plane( Meshes[ i ].Normal, Vector.Dot( Meshes[ i ].Normal, WorldPoint ) );
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

        public Vector[] GetVerts()
        {
            HashSet<Vector> Verts = new();
            for ( int i = 0; i < Meshes.Length; ++i )
            {
                Verts.UnionWith( Meshes[ i ].GetVerts() );
            }
            return Verts.ToArray();
        }
        public Vector[] GetWorldVerts()
        {
            Vector[] ret = GetVerts();
            for ( int i = 0; i < ret.Length; ++i )
            {
                ret[ i ] = TransformPoint( ret[ i ] );
            }
            return ret;
        }
        public bool TestCollision( Vector pt )
        {
            Vector[] Points1 = GetWorldVerts();
            Vector[] Points2 = { pt };
            for ( int i = 0; i < Meshes.Length; ++i )
            {
                if ( !Collision.TestCollision( Meshes[ i ].Normal, Points1, Points2 ) )
                    return false;
            }
            return true;
        }
    }

    public enum Space
    {
        NONE      = 0,
        WORLD     = 1 << 0,
        SELF      = 1 << 1,
    }

    public class BoxEnt : BaseEntity
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

            bool SingleTexture = tx.Length == 1;

            for ( int i = 0; i < 6; ++i )
                Meshes[ i ] = new FaceMesh( Verts[ i ], inds, SingleTexture ? tx[ 0 ] : tx[ i ], Normals[ i ] );

        }

        public BBox AABB;
    }

}
