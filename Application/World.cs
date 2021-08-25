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
    class World
    {
        public World()
        {
            WorldEnts = new List<EHandle>();
            Textures = new List<TextureHandle>();
            Players = new List<Player>();

            PinnedHandles = new List<GCHandle>();
        }
        public World( params EHandle[] Ents )
        {
            WorldEnts = new List<EHandle>( Ents );
        }
        public List<EHandle> WorldEnts;
        public List<Player> Players;
        public List<TextureHandle> Textures;

        private List<GCHandle> PinnedHandles;

        public BaseEntity[] GetEntList()
        {
            BaseEntity[] ret = new BaseEntity[ WorldEnts.Count ];
            for ( int i = 0; i < ret.Length; ++i )
            {
                ret[ i ] = WorldEnts[ i ].ent;
                ret[ i ].AABB = WorldEnts[ i ].AABB.Data;
                ret[ i ].transform = WorldEnts[ i ].Transform.Data;
            }
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
        public void Add( params TextureHandle[] tex )
        {
            Textures.AddRange( tex );
        }
        public void Add( params Player[] p )
        {
            Players.AddRange( p );
        }
        public void Close()
        {
            for ( int i = 0; i < WorldEnts.Count; ++i )
                WorldEnts[ i ].ent.Close();
            for ( int i = 0; i < Players.Count; ++i )
                Players[ i ].ent.Close();
            for ( int i = 0; i < PinnedHandles.Count; ++i )
                PinnedHandles[ i ].Free();
        }

        public void ToFile( string filepath )
        {
            FileStream fs = new FileStream( filepath, FileMode.Create );
            BinaryWriter bw = new BinaryWriter( fs );
            BaseEntity[] entlist = GetEntList();

            //tell the file how many players are in the world
            bw.Write( Players.Count );
            for ( int i = 0; i < Players.Count; ++i )
            {
                //save the player's ent to the file
                int bsize = Marshal.SizeOf( typeof( BaseEntity ) );
                byte[] bbytes = new byte[ bsize ];
                IntPtr bptr = Marshal.AllocHGlobal( bsize );
                Marshal.StructureToPtr( Players[ i ].ent, bptr, false );
                Marshal.Copy( bptr, bbytes, 0, bsize );
                Marshal.FreeHGlobal( bptr );
                bw.Write( bbytes );

                //save the player's perspective matrix
                int msize = Marshal.SizeOf( typeof( Matrix ) );
                byte[] mbytes = new byte[ msize ];
                IntPtr mptr = Marshal.AllocHGlobal( msize );
                Marshal.StructureToPtr( Players[ i ].Perspective, mptr, false );
                Marshal.Copy( mptr, mbytes, 0, msize );
                Marshal.FreeHGlobal( mptr );
                bw.Write( mbytes );
            }

            //save the names of the textures in this world
            bw.Write( Textures.Count );
            for ( int i = 0; i < Textures.Count; ++i )
                bw.Write( Textures[ i ].TextureName );

            //tell the file how long the entlist is
            bw.Write( entlist.Length );
            for ( int i = 0; i < entlist.Length; ++i )
            {
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

                    int facesize = Marshal.SizeOf( typeof( BaseFace ) );
                    byte[] facebytes = new byte[ facesize ];
                    IntPtr faceptr = Marshal.AllocHGlobal( facesize );
                    Marshal.StructureToPtr( faces[ j ], faceptr, true );
                    Marshal.Copy( faceptr, facebytes, 0, facesize );
                    Marshal.FreeHGlobal( faceptr );
                    bw.Write( facebytes );
                }
                int entsize = Marshal.SizeOf( typeof( BaseEntity ) );
                byte[] bytes = new byte[ entsize ];
                IntPtr ptr = Marshal.AllocHGlobal( entsize );
                Marshal.StructureToPtr( entlist[ i ], ptr, true );
                Marshal.Copy( ptr, bytes, 0, entsize );
                Marshal.FreeHGlobal( ptr );
                bw.Write( bytes );
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

            //read the players in the world from the file
            int PlayerCount = br.ReadInt32();
            for ( int i = 0; i < PlayerCount; ++i )
            {
                //old ent
                byte[] bbytes = br.ReadBytes( Marshal.SizeOf( typeof( BaseEntity ) ) );
                GCHandle phandle = GCHandle.Alloc( bbytes, GCHandleType.Pinned );
                BaseEntity bOld = (BaseEntity) Marshal.PtrToStructure( phandle.AddrOfPinnedObject(), typeof( BaseEntity ) );
                phandle.Free();

                //old perspective
                byte[] mbytes = br.ReadBytes( Marshal.SizeOf( typeof( Matrix ) ) );
                GCHandle mhandle = GCHandle.Alloc( mbytes, GCHandleType.Pinned );
                Matrix mOld = (Matrix) Marshal.PtrToStructure( mhandle.AddrOfPinnedObject(), typeof( Matrix ) );
                mhandle.Free();

                w.Add( new Player( new THandle( bOld.transform ), mOld ) );
            }

            //get all the textures used in the world for reconstruction
            int TexLength = br.ReadInt32();
            for ( int i = 0; i < TexLength; ++i )
                w.Textures.Add( new TextureHandle( br.ReadString() ) );

            //get how many entities are in this world
            int size = br.ReadInt32();
            BaseEntity[] entlist = new BaseEntity[ size ];
            try
            {
                for ( int i = 0; i < size; ++i )
                {
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


                        byte[] facebytes = br.ReadBytes( Marshal.SizeOf( typeof( BaseFace ) ) );
                        faces[ j ] = new BaseFace( vertices, indices, FaceTex.texture );

                        //pin the arrays so they don't dissapear
                        GCHandle verthandle = GCHandle.Alloc( vertices, GCHandleType.Pinned );
                        GCHandle indhandle = GCHandle.Alloc( indices, GCHandleType.Pinned );
                        w.PinnedHandles.Add( verthandle );
                        w.PinnedHandles.Add( indhandle );
                        faces[ j ].Verts = verthandle.AddrOfPinnedObject();
                        faces[ j ].Inds = indhandle.AddrOfPinnedObject();
                    }
                    byte[] bytes = br.ReadBytes( Marshal.SizeOf( typeof( BaseEntity ) ) );
                    GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
                    entlist[ i ] = (BaseEntity) Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( BaseEntity ) );
                    handle.Free();
                    entlist[ i ] = new BaseEntity( faces, entlist[ i ].transform, entlist[ i ].AABB.mins, entlist[ i ].AABB.maxs );
                    //again pin the array so it doesn't dissappear
                    GCHandle facehandle = GCHandle.Alloc( faces, GCHandleType.Pinned );
                    entlist[ i ].EntFaces = facehandle.AddrOfPinnedObject();
                    w.PinnedHandles.Add( facehandle );
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( e );
            }
            finally
            {
                sr.Close();
                br.Close();
            }
            EHandle[] handles = new EHandle[ entlist.Length ];
            for ( int i = 0; i < entlist.Length; ++i )
            {
                handles[ i ] = new EHandle( entlist[ i ] );
                handles[ i ].AABB = new BHandle( entlist[ i ].AABB );
                handles[ i ].Transform = new THandle( entlist[ i ].transform );
            }
            w.Add( handles );
            return w;
        }
    }
}
