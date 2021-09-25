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
        public RayHitInfo( Vector ptHit, Vector vNormal, BaseEntity HitEnt )
        {
            this.bHit = true;
            this.ptHit = ptHit;
            this.vNormal = vNormal;
            this.HitEnt = HitEnt;
        }
        public bool bHit;
        public Vector ptHit;
        public Vector vNormal;
        public BaseEntity HitEnt;
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
                        return new RayHitInfo( ptStart, plane.Normal, WorldEnts[ i ] );
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

            int EntListLength = br.ReadInt32();
            for ( int i = 0; i < EntListLength; ++i )
            {
                int EntByteLength = br.ReadInt32();
                w.Add( BaseEntity.FromBytes( br.ReadBytes( EntByteLength ) ) );
            }

            sr.Close();
            br.Close();

            return w;
        }
    }
}
