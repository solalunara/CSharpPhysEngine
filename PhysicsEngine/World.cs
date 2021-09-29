using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
                if ( !PhysicsObjects.Contains( _Player.Body ) )
                    PhysicsObjects.Add( _Player.Body );
                if ( !WorldEnts.Contains( _Player.camera ) )
                    WorldEnts.Add( _Player.camera );
                if ( !Environment.PObjs.Contains( _Player.Body ) )
                    Environment.PObjs.Add( _Player.Body );
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
            PhysicsObjects.AddRange( pobjs );
            Environment.PObjs.AddRange( pobjs );
        }
        public override void AddPhysicsObject( BasePhysics p )
        {
            PhysicsObjects.Add( p );
            Environment.PObjs.Add( p );
        }
        public override void RemovePhysicsObject( BasePhysics p )
        {
            PhysicsObjects.Remove( p );
            Environment.PObjs.Remove( p );
        }
        public void Close()
        {
            for ( int i = 0; i < WorldEnts.Count; ++i )
                WorldEnts[ i ].Close();
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
                byte[] EntBytes = entlist[ i ].ToBytes();
                bw.Write( EntBytes.Length );
                bw.Write( EntBytes );

                bool IsPlayerCamera = entlist[ i ] == player.camera;
                bool IsPlayerBody = entlist[ i ] == player.Body.LinkedEnt;
                bw.Write( IsPlayerCamera );
                bw.Write( IsPlayerBody );
            }

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
                int EntByteLength = br.ReadInt32();
                BaseEntity ent = BaseEntity.FromBytes( br.ReadBytes( EntByteLength ) );
                w.Add( ent );

                bool IsPlayerCamera = br.ReadBoolean();
                bool IsPlayerBody = br.ReadBoolean();
                if ( IsPlayerCamera )
                {
                    w.WorldEnts.Remove( w.player.camera );
                    w.player.camera = new Camera( ent );
                }
                if ( IsPlayerBody )
                {
                    w.WorldEnts.Remove( w.player.Body.LinkedEnt );
                    w.player.Body.LinkedEnt = ent;
                }
            }

            w.player.camera.Parent = w.player.Body.LinkedEnt;
            w.player.camera.SetLocalOrigin( Player3D.EYE_CENTER_OFFSET );
            w.player.camera.SetLocalRot( Matrix.IdentityMatrix() );

            sr.Close();
            br.Close();

            return w;
        }
    }
}
