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
        public RayHitInfo( Vector ptHit, Vector vNormal, EHandle HitEnt )
        {
            this.bHit = true;
            this.ptHit = ptHit;
            this.vNormal = vNormal;
            this.HitEnt = HitEnt;
        }
        public bool bHit;
        public Vector ptHit;
        public Vector vNormal;
        public EHandle HitEnt;
    }
    class World
    {
        public World()
        {
            WorldEnts = new List<EHandle>();
            PhysicsObjects = new List<PhysicsObject>();
            Textures = new List<TextureHandle>();
        }
        public World( params EHandle[] Ents )
        {
            WorldEnts = new List<EHandle>( Ents );
        }
        public List<EHandle> WorldEnts;
        public List<PhysicsObject> PhysicsObjects;
        public Player player;
        public List<TextureHandle> Textures;

        public PhysicsObject GetPlayerPhysics()
        {
            return GetEntPhysics( player );
        }
        public PhysicsObject GetEntPhysics( EHandle ent )
        {
            for ( int i = 0; i < PhysicsObjects.Count; ++i )
            {
                if ( PhysicsObjects[ i ].LinkedEnt == ent )
                    return PhysicsObjects[ i ];
            }
            return null;
        }


        public BaseEntity[] GetEntList()
        {
            BaseEntity[] ret = new BaseEntity[ WorldEnts.Count ];
            for ( int i = 0; i < ret.Length; ++i )
                ret[ i ] = WorldEnts[ i ].ent;
            return ret;
        }
        public int TextureIndex( string tex )
        {
            int index = -1;
            for ( int i = 0; i < Textures.Count; ++i )
            {
                if ( Textures[i].TextureName == tex )
                    index = i;
            }
            return index;
        }
        public int TextureIndex( int ID )
        {
            int index = -1;
            for ( int i = 0; i < Textures.Count; ++i )
            {
                if ( Textures[ i ].texture.ID == ID )
                    index = i;
            }
            return index;
        }

        public void Add( params EHandle[] ent )
        {
            WorldEnts.AddRange( ent );
        }
        public void Add( bool AddToWorld, params PhysicsObject[] pobjs )
        {
            if ( AddToWorld )
            {
                for ( int i = 0; i < pobjs.Length; ++i )
                {
                    WorldEnts.Add( pobjs[ i ].LinkedEnt );
                }
            }
            PhysicsObjects.AddRange( pobjs );
        }
        public void Add( params TextureHandle[] tex )
        {
            Textures.AddRange( tex );
        }
        public void Add( Player p )
        {
            WorldEnts.Add( p );
            player = p;
        }
        public void Close()
        {
            for ( int i = 0; i < WorldEnts.Count; ++i )
                WorldEnts[ i ].ent.Close();
            for ( int i = 0; i < Textures.Count; ++i )
                Textures[ i ].texture.Close();
            player.ent.Close();
        }

        public RayHitInfo TraceRay( Vector ptStart, Vector ptEnd, int TraceFidelity = 300 )
        {
            Vector vDirection = ( ptEnd - ptStart ) / TraceFidelity;
            while ( TraceFidelity > 0 )
            {
                for ( int i = 0; i < WorldEnts.Count; ++i )
                {
                    if ( WorldEnts[ i ] == player )
                        continue;

                    if ( WorldEnts[ i ].AABB.TestCollision( ptStart, WorldEnts[ i ].Transform.Position ) )
                    {
                        Plane plane = WorldEnts[ i ].AABB.GetCollisionPlane( ptStart, WorldEnts[ i ].Transform.Position );
                        return new RayHitInfo( ptStart, plane.Normal, WorldEnts[ i ] );
                    }
                }
                ptStart += vDirection;
                --TraceFidelity;
            }
            return new RayHitInfo();
        }
        public void Simulate( float dt )
        {
            List<PhysicsObject[]> PhysCollisionPairs = PhysicsObject.GetCollisionPairs( this );
            for ( int i = 0; i < PhysCollisionPairs.Count; ++i )
            {
                PhysicsObject.Collide( PhysCollisionPairs[ i ][ 0 ], PhysCollisionPairs[ i ][ 1 ], dt );
            }
            foreach ( PhysicsObject p in PhysicsObjects )
                p.Simulate( dt, this );
        }


        public void ToFile( string filepath )
        {
            FileStream fs = new FileStream( filepath, FileMode.Create );
            BinaryWriter bw = new BinaryWriter( fs );
            BaseEntity[] entlist = GetEntList();

            //player index (to skip in reconstruction)
            int PlayerIndex = WorldEnts.IndexOf( player );
            bw.Write( PlayerIndex );

            //save the player's ent to the file
            int bsize = Marshal.SizeOf( typeof( BaseEntity ) );
            byte[] bbytes = new byte[ bsize ];
            IntPtr bptr = Marshal.AllocHGlobal( bsize );
            Marshal.StructureToPtr( player.ent, bptr, false );
            Marshal.Copy( bptr, bbytes, 0, bsize );
            Marshal.FreeHGlobal( bptr );
            bw.Write( bbytes );

            //save the names of the textures in this world
            bw.Write( Textures.Count );
            for ( int i = 0; i < Textures.Count; ++i )
                bw.Write( Textures[ i ].TextureName );

            //tell the file how long the entlist is
            bw.Write( entlist.Length );
            for ( int i = 0; i < entlist.Length; ++i )
            {
                if ( i == PlayerIndex )
                    continue;
                //baseent stores a ptr to an array of basefaces
                //we need to reconstruct that array and save it into the file
                BaseFace[] faces = new BaseFace[ entlist[i].FaceLength ];
                for ( int z = 0; z < faces.Length; ++z )
                    faces[ z ] = entlist[ i ].GetFaceAtIndex( z );
                
                //tell the file how long the facelist is
                bw.Write( faces.Length );
                for ( int j = 0; j < faces.Length; ++j )
                {
                    //we want to write the string that represents the texture of this face to the file
                    TextureHandle FaceTexture = Textures[ TextureIndex( (int) faces[ j ].texture.ID ) ];
                    bw.Write( FaceTexture.TextureName );

                    //baseface stores a ptr to verts and inds, so we need to store those both in the file
                    //luckily, since floats and inds are primitive types, this bit's pretty simple (ba dum tssss)
                    bw.Write( faces[ j ].VertLength );
                    for ( int z = 0; z < faces[ j ].VertLength; ++z )
                        bw.Write( faces[ j ].GetVertAtIndex( z ) );

                    bw.Write( faces[j].IndLength );
                    for ( int z = 0; z < faces[ j ].IndLength; ++z )
                        bw.Write( faces[ j ].GetIndAtIndex( z ) );

                    for ( int z = 0; z < 3; ++z )
                        bw.Write( faces[ j ].Normal[ z ] );
                }
                int entsize = Marshal.SizeOf( typeof( BaseEntity ) );
                byte[] bytes = new byte[ entsize ];
                IntPtr ptr = Marshal.AllocHGlobal( entsize );
                Marshal.StructureToPtr( entlist[ i ], ptr, true );
                Marshal.Copy( ptr, bytes, 0, entsize );
                Marshal.FreeHGlobal( ptr );
                bw.Write( bytes );
            }
            //write physics objects to file
            bw.Write( PhysicsObjects.Count );
            for ( int i = 0; i < PhysicsObjects.Count; ++i )
            {
                bw.Write( WorldEnts.IndexOf( PhysicsObjects[ i ].LinkedEnt ) );
                for ( int j = 0; j < 3; ++j )
                {
                    bw.Write( PhysicsObjects[ i ].Gravity[ j ] );
                    bw.Write( PhysicsObjects[ i ].AirDragCoeffs[ j ] );
                    bw.Write( PhysicsObjects[ i ].Velocity[ j ] );
                    bw.Write( PhysicsObjects[ i ].BaseVelocity[ j ] );
                }
                bw.Write( PhysicsObjects[ i ].Mass );
            }
            fs.Close();
            bw.Close();
        }
        //reads the data from a file to construct a world
        public static World FromFile( string filepath )
        {
            StreamReader sr = new StreamReader( filepath );
            BinaryReader br = new BinaryReader( sr.BaseStream );

            World w = new World();

            int PlayerIndex = br.ReadInt32();

            //old ent
            byte[] bbytes = br.ReadBytes( Marshal.SizeOf( typeof( BaseEntity ) ) );
            GCHandle phandle = GCHandle.Alloc( bbytes, GCHandleType.Pinned );
            BaseEntity bOld = (BaseEntity) Marshal.PtrToStructure( phandle.AddrOfPinnedObject(), typeof( BaseEntity ) );
            phandle.Free();

            Player p = new Player( Matrix.IdentityMatrix() )
            {
                Transform = new THandle( bOld.transform )
            };
            w.Add( p );

            //get all the textures used in the world for reconstruction
            int TexLength = br.ReadInt32();
            for ( int i = 0; i < TexLength; ++i )
                w.Textures.Add( new TextureHandle( br.ReadString() ) );

            //get how many entities are in this world
            int size = br.ReadInt32();
            BaseEntity[] entlist = new BaseEntity[ size ];

            for ( int i = 0; i < size; ++i )
            {
                if ( i == PlayerIndex )
                    continue;

                //get how many faces are in this ent
                int facesize = br.ReadInt32();
                BaseFace[] faces = new BaseFace[ facesize ];
                for ( int j = 0; j < facesize; ++j )
                {
                    //point the texture of the face to the texture it should point to out of the world textures
                    TextureHandle FaceTex = w.Textures[ w.TextureIndex( br.ReadString() ) ];
                    faces[ j ].texture = FaceTex.texture;

                    int vertsize = br.ReadInt32();
                    float[] vertices = new float[ vertsize ];
                    for ( int z = 0; z < vertsize; ++z )
                         vertices[ z ] = br.ReadSingle();

                    int indsize = br.ReadInt32();
                    int[] indices = new int[ indsize ];
                    for ( int z = 0; z < indsize; ++z )
                        indices[ z ] = br.ReadInt32();

                    Vector Normal = new Vector();
                    for ( int z = 0; z < 3; ++z )
                        Normal[ z ] = br.ReadSingle();

                    faces[ j ] = new BaseFace( vertices, indices, FaceTex.texture, Normal );
                }
                byte[] bytes = br.ReadBytes( Marshal.SizeOf( typeof( BaseEntity ) ) );
                GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
                entlist[ i ] = (BaseEntity) Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( BaseEntity ) );
                handle.Free();
                entlist[ i ] = new BaseEntity( faces, entlist[ i ].transform, entlist[ i ].AABB.mins, entlist[ i ].AABB.maxs );
            }

            EHandle[] handles = new EHandle[ entlist.Length ];
            for ( int i = 0; i < entlist.Length; ++i )
            {
                handles[ i ] = new EHandle( entlist[ i ] )
                {
                    AABB = new BHandle( entlist[ i ].AABB ),
                    Transform = new THandle( entlist[ i ].transform )
                };
            }

            //reconstruct physics objects
            int PhysObjCount = br.ReadInt32();
            PhysicsObject[] pObjs = new PhysicsObject[ PhysObjCount ];
            for ( int i = 0; i < PhysObjCount; ++i )
            {
                int EntIndex = br.ReadInt32();
                Vector Gravity, AirDragCoeffs, Velocity, BaseVelocity;
                Gravity = AirDragCoeffs = Velocity = BaseVelocity = new Vector();
                for ( int j = 0; j < 3; ++j )
                {
                    Gravity[ j ] = br.ReadSingle();
                    AirDragCoeffs[ j ] = br.ReadSingle();
                    Velocity[ j ] = br.ReadSingle();
                    BaseVelocity[ j ] = br.ReadSingle();
                }
                float Mass = br.ReadSingle();
                if ( EntIndex != PlayerIndex )
                    pObjs[ i ] = new PhysicsObject( handles[ EntIndex ], Gravity, AirDragCoeffs, Mass );
                else
                    pObjs[ i ] = new PhysicsObject( w.player, Gravity, AirDragCoeffs, Mass );
                pObjs[ i ].Velocity = Velocity;
                pObjs[ i ].BaseVelocity = BaseVelocity;
            }

            w.Add( handles );
            w.Add( false, pObjs );

            sr.Close();
            br.Close();

            for ( int i = 0; i < w.WorldEnts.Count; ++i )
            {
                if ( w.WorldEnts[ i ].ent.FaceLength == 0 )
                    w.WorldEnts.RemoveAt( i );
            }

            return w;
        }
    }
}
