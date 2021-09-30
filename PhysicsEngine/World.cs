using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using static RenderInterface.SaveRestore;

namespace PhysEngine
{
    struct RayHitInfo
    {
        public RayHitInfo( Vector ptHit, Vector vNormal, BaseEntity HitEnt, int HitFace )
        {
            this.bHit = true;
            this.ptHit = ptHit;
            this.vNormal = vNormal;
            this.HitEnt = HitEnt;
            this.HitFace = HitFace;
        }
        public bool bHit;
        public Vector ptHit;
        public Vector vNormal;
        public BaseEntity HitEnt;
        public int HitFace;
    }
    class World : BaseWorld
    {
        public World( Vector Gravity, float PhysSimTime ) : base()
        {
            Environment = new( Gravity );
            Simulator = new( PhysSimTime, Environment, this );
        }

        public PhysicsEnvironment Environment;
        public PhysicsSimulator Simulator;

        private Player _Player;
        public Player player
        { 
            get => _Player; 
            set
            {
                _Player = value;
                if ( !Environment.PObjs.Contains( _Player.Body ) )
                    Add( _Player.Body );
                if ( !WorldEnts.Contains( _Player.camera ) )
                    WorldEnts.Add( _Player.camera );
            } 
        }

        public void Add( params BaseEntity[] ent )
        {
            WorldEnts.AddRange( ent );
        }
        public void Add( params PhysObj[] pobjs )
        {
            foreach ( PhysObj p in pobjs )
            {
                if ( !WorldEnts.Contains( p.LinkedEnt ) )
                {
                    WorldEnts.Add( p.LinkedEnt );
                }
            }
            Environment.PObjs.AddRange( pobjs );
        }
        public override void AddPhysicsObject( BasePhysics p ) => Add( (PhysObj)p );
        public override void RemovePhysicsObject( BasePhysics p )
        {
            Environment.PObjs.Remove( p );
            WorldEnts.Remove( p.LinkedEnt );
        }
        public void Close()
        {
            for ( int i = 0; i < WorldEnts.Count; ++i )
                WorldEnts[ i ].Close();
            Simulator.Close();
        }

        public RayHitInfo TraceRay( Vector ptStart, Vector ptEnd, params BaseEntity[] IgnoreEnts )
        {
            int TraceFidelity = 300;
            Vector vDirection = ( ptEnd - ptStart ) / TraceFidelity;
            while ( TraceFidelity > 0 )
            {
                for ( int i = 0; i < WorldEnts.Count; ++i )
                {
                    if ( IgnoreEnts.Contains( WorldEnts[ i ] ) )
                        continue;

                    if ( WorldEnts[ i ].TestCollision( ptStart ) )
                    {
                        Plane plane = WorldEnts[ i ].GetCollisionPlane( ptStart );
                        int j;
                        for ( j = 0; j < WorldEnts[ i ].Meshes.Length; ++j )
                        {
                            if ( plane.Normal == WorldEnts[ i ].Meshes[ j ].Item1.Normal )
                                break;
                        }
                        return new RayHitInfo( ptStart, plane.Normal, WorldEnts[ i ], j );
                    }
                }
                ptStart += vDirection;
                --TraceFidelity;
            }
            return new RayHitInfo();
        }

        public void ToFile( string filepath )
        {
            FileStream fs = new( filepath, FileMode.Create );
            BinaryWriter bw = new( fs );
            BaseEntity[] entlist = GetEntList();

            bw.Write( entlist.Length );
            for ( int i = 0; i < entlist.Length; ++i )
            {
                bool IsPlayerCamera = entlist[ i ] == player.camera;
                bool IsPlayerBody = entlist[ i ] == player.Body.LinkedEnt;
                bool HasPhysics = GetEntPhysics( entlist[ i ] ) is not null;
                bw.Write( !IsPlayerBody && !IsPlayerCamera && !HasPhysics );
                if ( !IsPlayerBody && !IsPlayerCamera && !HasPhysics )
                {
                    byte[] EntBytes = entlist[ i ].ToBytes();
                    bw.Write( EntBytes.Length );
                    bw.Write( EntBytes );
                }
            }


            bw.Write( Environment.PObjs.Count );
            for ( int i = 0; i < Environment.PObjs.Count; ++i )
            {
                bool IsPlayerBody = Environment.PObjs[ i ] == player.Body;
                bw.Write( !IsPlayerBody );
                if ( !IsPlayerBody )
                {
                    byte[] PhysBytes = Environment.PObjs[ i ].ToBytes();
                    bw.Write( PhysBytes.Length );
                    bw.Write( PhysBytes );
                }
            }

            byte[] CameraRot = StructToBytes( player.camera.GetLocalRot() );
            byte[] BodyPos = StructToBytes( player.Body.LinkedEnt.GetAbsOrigin() );
            byte[] BodyRot = StructToBytes( player.Body.LinkedEnt.GetAbsRot() );
            byte[] BodyVel = StructToBytes( player.Body.Velocity );
            bw.Write( CameraRot );
            bw.Write( BodyPos );
            bw.Write( BodyRot );
            bw.Write( BodyVel );

            fs.Close();
            bw.Close();
        }
        //reads the data from a file to construct a world
        public static World FromFile( string filepath )
        {
            StreamReader sr = new( filepath );
            BinaryReader br = new( sr.BaseStream );

            World w = new( PhysicsEnvironment.Default_Gravity, 0.02f );
            w.player = new Player3D( Matrix.IdentityMatrix(), PhysObj.Default_Coeffs, Player3D.PLAYER_MASS, Player3D.PLAYER_ROTI );

            int EntListLength = br.ReadInt32();
            for ( int i = 0; i < EntListLength; ++i )
            {
                if ( br.ReadBoolean() )
                {
                    int EntByteLength = br.ReadInt32();
                    BaseEntity ent = BaseEntity.FromBytes( br.ReadBytes( EntByteLength ) );
                    w.Add( ent );
                }
            }

            int PhysObjCount = br.ReadInt32();
            for ( int i = 0; i < PhysObjCount; ++i )
            {
                if ( br.ReadBoolean() )
                {
                    int PhysObjByteLength = br.ReadInt32();
                    PhysObj p = PhysObj.FromBytes( br.ReadBytes( PhysObjByteLength ) );
                    w.Add( p );
                }
            }

            Matrix CameraRot = BytesToStruct<Matrix>( br.ReadBytes( Marshal.SizeOf( typeof( Matrix ) ) ) );
            Vector BodyPos = BytesToStruct<Vector>( br.ReadBytes( Marshal.SizeOf( typeof( Vector ) ) ) );
            Matrix BodyRot = BytesToStruct<Matrix>( br.ReadBytes( Marshal.SizeOf( typeof( Matrix ) ) ) );
            Vector BodyVel = BytesToStruct<Vector>( br.ReadBytes( Marshal.SizeOf( typeof( Vector ) ) ) );
            w.player.Body.LinkedEnt.SetAbsOrigin( BodyPos );
            w.player.Body.LinkedEnt.SetAbsRot( BodyRot );
            w.player.camera.SetLocalRot( CameraRot );
            w.player.Body.Velocity = BodyVel;

            sr.Close();
            br.Close();

            return w;
        }

        public override BasePhysics[] GetPhysObjList() => Environment.PObjs.ToArray();

        public override BasePhysics GetEntPhysics( BaseEntity ent )
        {
            for ( int i = 0; i < Environment.PObjs.Count; ++i )
            {
                if ( Environment.PObjs[ i ].LinkedEnt == ent )
                    return Environment.PObjs[ i ];
            }
            return null;
        }
    }
}
