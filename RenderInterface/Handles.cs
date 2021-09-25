using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RenderInterface
{
    // Interfaces for projects that don't include PhysicsEngine
    public class BaseEntity
    {
        //constructor
        public BaseEntity( FaceMesh[] Meshes, Transform LocalTransform )
        {
            this.Meshes = Meshes;
            this.LocalTransform = LocalTransform;
        }

        //member data
        public FaceMesh[] Meshes;
        public Transform LocalTransform;

        //sort of member data
        private BaseEntity _Parent;
        public BaseEntity Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                Vector OldPos = GetAbsOrigin();
                Matrix OldRot = GetAbsRot();
                _Parent = value;
                SetAbsOrigin( OldPos );
                SetAbsRot( OldRot );
            }
        }

        //member methods
        public void Close()
        {
            foreach ( FaceMesh m in Meshes )
            {
                m.Close();
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
        public Vector TransformDirection( Vector dir ) => (Vector)( CalcEntMatrix() * new Vector4( dir, 0.0f ) );
        public Vector TransformPoint( Vector pt ) => (Vector)( CalcEntMatrix() * new Vector4( pt, 1.0f ) );
        public Vector InverseTransformDirection( Vector dir ) => (Vector)( -CalcEntMatrix() * new Vector4( dir, 0.0f ) );
        public Vector InverseTransformPoint( Vector pt ) => (Vector)( -CalcEntMatrix() * new Vector4( pt, 1.0f ) );
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
                if ( TestCollision( Meshes[ i ].Normal, Points1, Points2 ) == 0 )
                    return false;
            }
            return true;
        }
        public Plane GetCollisionPlane( Vector pt )
        {
            Plane[] planes = new Plane[ Meshes.Length ];
            for ( int i = 0; i < planes.Length; ++i )
            {
                Vector WorldPoint = TransformPoint( Meshes[ i ].GetVerts()[ 0 ] );
                planes[ i ] = new Plane( TransformDirection( Meshes[ i ].Normal ), Vector.Dot( TransformDirection( Meshes[ i ].Normal ), WorldPoint ) );
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

        public byte[] ToBytes()
        {
            byte[] TransformBytes = LocalTransform.ToBytes();
            List<byte[]> MeshBytes = new();
            for ( int i = 0; i < Meshes.Length; ++i )
            {
                MeshBytes.Add( Meshes[ i ].ToBytes() );
            }
            byte NumMeshes = (byte)Meshes.Length;
            List<byte> ret = new();
            ret.AddRange( TransformBytes );
            ret.Add( NumMeshes );
            for ( int i = 0; i < NumMeshes; ++i )
            {
                ret.AddRange( MeshBytes[ i ] );
            }
            if ( Parent != null )
                ret.AddRange( Parent.ToBytes() );
            return ret.ToArray();
        }
        public static BaseEntity FromBytes( byte[] Bytes )
        {
            Transform LT = Transform.FromBytes( Bytes );
            int TransformSize = 2 * Marshal.SizeOf( typeof( Vector ) ) + Marshal.SizeOf( typeof( Matrix ) );
            int MeshCount = Bytes[ TransformSize ];
            FaceMesh[] Meshes = new FaceMesh[ MeshCount ];
            int Index = TransformSize + 1;
            for ( int i = 0; i < MeshCount; ++i )
            {
                List<byte> bs = new();
                for ( int j = 0; j < Marshal.SizeOf( typeof( FaceMesh ) ); ++j )
                    bs.Add( Bytes[ Index + j ] );
                Meshes[ i ] = FaceMesh.FromBytes( bs.ToArray() );
                Index += Marshal.SizeOf( Meshes[ i ] );
            }
            if ( Index >= Bytes.Length)
                return new BaseEntity( Meshes, LT );
            else
            {
                BaseEntity b = new( Meshes, LT );
                List<byte> ParentBytes = new();
                while ( Index < Bytes.Length )
                {
                    ParentBytes.Add( Bytes[ Index ] );
                    ++Index;
                }
                b.Parent = FromBytes( ParentBytes.ToArray() );
                return b;
            }
        }

        //static members
        //Worth noting: this function gives an AXIS of collision, not a normal of collision
        //  what to do with the axis and how to orient it is up to implementers
        public static Vector TestCollision( BaseEntity ent1, BaseEntity ent2, Vector offset1 = default, Vector offset2 = default )
        {
            if ( !BinaryTestCollision( ent1, ent2, offset1, offset2 ) )
            {
                Debug.Assert( false, "Objects not colliding!" ); //don't make it quiet
                return new();
            }
            Vector[] Points1 = ent1.GetWorldVerts();
            Vector[] Points2 = ent2.GetWorldVerts();
            for ( int i = 0; i < Points1.Length; ++i )
                Points1[ i ] += offset1;
            for ( int i = 0; i < Points2.Length; ++i )
                Points2[ i ] += offset2;

            List<float> NormsEnt1 = new();
            List<float> NormsEnt2 = new();
            for ( int i = 0; i < ent1.Meshes.Length; ++i )
                NormsEnt1.Add( TestCollision( ent1.TransformDirection( ent1.Meshes[ i ].Normal ), Points1, Points2 ) );
            for ( int i = 0; i < ent2.Meshes.Length; ++i )
                NormsEnt2.Add( TestCollision( ent2.TransformDirection( ent2.Meshes[ i ].Normal ), Points1, Points2 ) );

            int NormEnt1 = NormsEnt1.IndexOf( NormsEnt1.Min() );
            int NormEnt2 = NormsEnt2.IndexOf( NormsEnt2.Min() );
            if ( NormEnt1 < NormEnt2 )
            {
                return ent1.TransformDirection( ent1.Meshes[ NormEnt1 ].Normal );
            }
            else
            {
                return ent2.TransformDirection( ent2.Meshes[ NormEnt2 ].Normal );
            }
        }
        public static float TestCollisionDepth( BaseEntity ent1, BaseEntity ent2, Vector offset1 = default, Vector offset2 = default )
        {
            if ( !BinaryTestCollision( ent1, ent2, offset1, offset2 ) )
            {
                Debug.Assert( false, "Objects not colliding!" ); //don't make it quiet
                return 0;
            }

            Vector[] Points1 = ent1.GetWorldVerts();
            Vector[] Points2 = ent2.GetWorldVerts();
            for ( int i = 0; i < Points1.Length; ++i )
                Points1[ i ] += offset1;
            for ( int i = 0; i < Points2.Length; ++i )
                Points2[ i ] += offset2;

            Vector Norm = TestCollision( ent1, ent2, offset1, offset2 );
            return TestCollision( Norm, Points1, Points2 );
        }
        public static bool BinaryTestCollision( BaseEntity ent1, BaseEntity ent2, Vector offset1 = new Vector(), Vector offset2 = new Vector() )
        {
            BaseEntity[] ents = { ent1, ent2 };

            Vector[] Points1 = ent1.GetWorldVerts();
            Vector[] Points2 = ent2.GetWorldVerts();
            for ( int i = 0; i < Points1.Length; ++i )
                Points1[ i ] += offset1;
            for ( int i = 0; i < Points2.Length; ++i )
                Points2[ i ] += offset2;

            for ( int EntIndex = 0; EntIndex < 2; ++EntIndex )
            {
                BaseEntity Ent = ents[ EntIndex ];
                for ( int i = 0; i < Ent.Meshes.Length; ++i )
                {
                    if ( TestCollision( Ent.TransformDirection( Ent.Meshes[ i ].Normal ), Points1, Points2 ) == 0 )
                        return false;
                }
            }
            return true;
        }
        public static float TestCollision( Vector Normal, Vector[] Points1, Vector[] Points2 )
        {
            if ( Points1.Length == 0 || Points2.Length == 0 )
                return 0;

            float[] ProjectedPoints1 = new float[ Points1.Length ];
            float[] ProjectedPoints2 = new float[ Points2.Length ];

            for ( int i = 0; i < Points1.Length; ++i )
                ProjectedPoints1[ i ] = Vector.Dot( Points1[ i ], Normal );
            for ( int i = 0; i < Points2.Length; ++i )
                ProjectedPoints2[ i ] = Vector.Dot( Points2[ i ], Normal );

            float amin = ProjectedPoints1.Min();
            float amax = ProjectedPoints1.Max();

            float bmin = ProjectedPoints2.Min();
            float bmax = ProjectedPoints2.Max();

            //if ( Small1 >= Large2 || Small2 >= Large1 )
            //    return 0;

            if ( amin < bmax && amin > bmin )
                return bmax - amin;
            else if ( bmin < amax && bmin > amin )
                return amax - bmin;
            else
                return 0;
        }
    }
    public class Transform
    {
        public Transform( Vector Position, Vector Scale, Matrix Rotation )
        {
            this._Position = Position;
            this._Scale = Scale;
            this._Rotation = Rotation;
            Update();
        }
        public Matrix ThisToWorld;
        public Matrix WorldToThis;

        private Vector _Position;
        public Vector Position
        {
            get => _Position;
            set
            {
                _Position = value;
                Update();
            }
        }

        private Vector _Scale;
        public Vector Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Update();
            }
        }

        private Matrix _Rotation;
        public Matrix Rotation
        {
            get => _Rotation;
            set
            {
                _Rotation = value;
                Update();
            }
        }

        public Vector QAngles
        {
            get
            {
                Vector angles = new();
                if ( Rotation.Columns[ 0 ][ 1 ] > 0.998 )
                { // singularity at north pole
                    angles.y = MathF.Atan2( Rotation.Columns[ 2 ][ 0 ], Rotation.Columns[ 2 ][ 2 ] ) * 180 / MathF.PI;
                    angles.x = 180;
                    angles.z = 0;
                    return angles;
                }
                if ( Rotation.Columns[ 0 ][ 1 ] < -0.998 )
                { // singularity at south pole
                    angles.y = MathF.Atan2( Rotation.Columns[ 2 ][ 0 ], Rotation.Columns[ 2 ][ 2 ] ) * 180 / MathF.PI;
                    angles.x = -180;
                    angles.z = 0;
                    return angles;
                }
                angles.x = MathF.Asin( Rotation.Columns[ 0 ][ 1 ] ) * 180 / MathF.PI;
                angles.y = MathF.Atan2( -Rotation.Columns[ 0 ][ 2 ], Rotation.Columns[ 0 ][ 0 ] ) * 180 / MathF.PI;
                angles.z = MathF.Atan2( -Rotation.Columns[ 2 ][ 1 ], Rotation.Columns[ 1 ][ 1 ] ) * 180 / MathF.PI;
                return angles;
            }
            set
            {
                Rotation = Matrix.RotMatrix( value.y, new Vector( 0, 1, 0 ) ) * Matrix.RotMatrix( value.x, new Vector( 1, 0, 0 ) ) * Matrix.RotMatrix( value.z, new Vector( 0, 0, 1 ) );
                Update();
            }
        }

        public void Update()
        {
            ThisToWorld = Matrix.Translate( Position ) * Rotation * Matrix.Scale( Scale );
            WorldToThis = -ThisToWorld;
        }

        public byte[] ToBytes()
        {
            int MatrixSize = Marshal.SizeOf( typeof( Matrix ) );
            int VectorSize = Marshal.SizeOf( typeof( Vector ) );
            byte[] ret = new byte[ 2 * VectorSize + MatrixSize ];
            IntPtr RotPtr = Marshal.AllocHGlobal( MatrixSize );
            IntPtr PosPtr = Marshal.AllocHGlobal( VectorSize );
            IntPtr SclPtr = Marshal.AllocHGlobal( VectorSize );
            Marshal.StructureToPtr( Rotation, RotPtr, false );
            Marshal.StructureToPtr( Position, PosPtr, false );
            Marshal.StructureToPtr( Scale, SclPtr, false );
            Marshal.Copy( PosPtr, ret, 0, VectorSize );
            Marshal.Copy( SclPtr, ret, MatrixSize, VectorSize );
            Marshal.Copy( RotPtr, ret, VectorSize * 2, MatrixSize );
            Marshal.FreeHGlobal( RotPtr );
            Marshal.FreeHGlobal( SclPtr );
            Marshal.FreeHGlobal( PosPtr );
            return ret;
        }
        public static Transform FromBytes( byte[] Bytes )
        {
            int MatrixSize = Marshal.SizeOf( typeof( Matrix ) );
            int VectorSize = Marshal.SizeOf( typeof( Vector ) );
            byte[] RotBytes = new byte[ MatrixSize ];

            for ( int i = 0; i < MatrixSize; ++i )
                RotBytes[ i ] = Bytes[ i ];
            byte[] PosBytes = new byte[ VectorSize ];
            for ( int i = 0; i < VectorSize; ++i )
                PosBytes[ i ] = Bytes[ i + MatrixSize ];
            byte[] SclBytes = new byte[ VectorSize ];
            for ( int i = 0; i < VectorSize; ++i )
                SclBytes[ i ] = Bytes[ i + MatrixSize + VectorSize ]; 

            GCHandle RotPtr = GCHandle.Alloc( RotBytes, GCHandleType.Pinned );
            GCHandle PosPtr = GCHandle.Alloc( PosBytes, GCHandleType.Pinned );
            GCHandle SclPtr = GCHandle.Alloc( SclBytes, GCHandleType.Pinned );
            Matrix Rot = Marshal.PtrToStructure<Matrix>( RotPtr.AddrOfPinnedObject() );
            Vector Scl = Marshal.PtrToStructure<Vector>( SclPtr.AddrOfPinnedObject() );
            Vector Pos = Marshal.PtrToStructure<Vector>( PosPtr.AddrOfPinnedObject() );
            RotPtr.Free();
            PosPtr.Free();
            SclPtr.Free();
            return new Transform( Pos, Scl, Rot );
        }
    }

    public class BaseWorld
    {
        public BaseWorld()
        {
            WorldEnts = new();
            PhysicsObjects = new();
        }
        public List<BaseEntity> WorldEnts;
        public List<BasePhysics> PhysicsObjects;
        public BaseEntity[] GetEntList() => WorldEnts.ToArray();
        public BasePhysics[] GetPhysObjList() => PhysicsObjects.ToArray();

        public BasePhysics GetEntPhysics( BaseEntity ent )
        {
            for ( int i = 0; i < PhysicsObjects.Count; ++i )
            {
                if ( PhysicsObjects[ i ].LinkedEnt == ent )
                    return PhysicsObjects[ i ];
            }
            return null;
        }
    }
    public class BasePhysics
    {
        public BasePhysics( BaseEntity LinkedEnt )
        {
            this.LinkedEnt = LinkedEnt;
            ForceChannels = new();
        }
        public readonly BaseEntity LinkedEnt;

        public Vector Momentum;
        public Vector Velocity
        { get => Momentum / Mass; set => Momentum = value * Mass; }
        public Vector AngularMomentum;
        public Vector AngularVelocity
        {
            get => AngularMomentum / RotInertia;
            set => AngularMomentum = value * RotInertia;
        }
        public Vector AirDragCoeffs;

        public Vector AirVelocity;
        public float Mass;
        public float RotInertia;

        public Vector NetForce;
        public Vector Torque;

        internal List<int> ForceChannels;
        public void ClearChannels() => ForceChannels.Clear();
        public void AddForce( Vector force, int Channel )
        {
            if ( !ForceChannels.Contains( Channel ) )
            {
                NetForce += force;
                ForceChannels.Add( Channel );
            }
        }
        public void TestCollision( BaseWorld world, out bool bCollision, out bool TopCollision )
        {
            bCollision = false;
            TopCollision = false;

            if ( LinkedEnt.Meshes.Length == 0 )
                return;

            for ( int i = 0; i < world.GetEntList().Length; ++i )
            {
                BaseEntity WorldEnt = world.GetEntList()[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions
                if ( WorldEnt.Meshes.Length == 0 )
                    continue; //nothing to collide with

                if ( BaseEntity.BinaryTestCollision( WorldEnt, LinkedEnt ) )
                {
                    bCollision = true;

                    Vector CollisionNormal = BaseEntity.TestCollision( LinkedEnt, WorldEnt );
                    if ( MathF.Abs( CollisionNormal.y ) > .7f )
                        TopCollision = true;
                }
            }
        }
    }
}
