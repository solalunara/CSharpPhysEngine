using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    public struct CollisionInfo
    {

        public Vector CollisionNormal; //
    }
    public class Collision
    {


        public static bool TestCollision( BaseEntity ent1, BaseEntity ent2, Vector offset1, Vector offset2 )
        {
            Vector[] Points1 = ent1.GetWorldVerts();
            Vector[] Points2 = ent2.GetWorldVerts();
            for ( int i = 0; i < Points1.Length; ++i )
                Points1[i] += offset1;
            for ( int i = 0; i < Points2.Length; ++i )
                Points2[i] += offset2;

            for ( int i = 0; i < ent1.Meshes.Length; ++i )
            {
                if ( !TestCollision( ent1.TransformDirection( ent1.Meshes[i].Normal ), Points1, Points2 ) )
                    return false;
            }
            for ( int i = 0; i < ent2.Meshes.Length; ++i )
            {
                if ( !TestCollision( ent2.TransformDirection( ent2.Meshes[i].Normal ), Points1, Points2 ) )
                    return false;
            }
            return true;
        }
        public static bool TestCollision( BaseEntity ent1, BaseEntity ent2 )
        {
            BaseEntity[] ents = { ent1, ent2 };

            Vector[] Points1 = ents[0].GetWorldVerts();
            Vector[] Points2 = ents[1].GetWorldVerts();

            for ( int EntIndex = 0; EntIndex < 2; ++EntIndex )
            {
                BaseEntity Ent = ents[EntIndex];
                for ( int i = 0; i < Ent.Meshes.Length; ++i )
                {
                    if ( !TestCollision( Ent.TransformDirection( Ent.Meshes[i].Normal ), Points1, Points2 ) )
                        return false;
                }
            }
            return true;
        }
        public static bool TestCollision( Vector Normal, Vector[] Points1, Vector[] Points2 )
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
}
