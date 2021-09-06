using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderInterface;

namespace Physics
{
    public class Collision
    {
        public Collision( int Frames, PhysObj Obj1, PhysObj Obj2 )
        {
            this.Frames = Frames;
            this.Obj1 = Obj1;
            this.Obj2 = Obj2;

            StartVel1 = Obj1.Velocity;
            StartVel2 = Obj2.Velocity;

            float m1 = Obj1.Mass;
            float m2 = Obj2.Mass;
            EndVel1 = ( Obj1.Velocity * ( m1 - m2 ) / ( m1 + m2 ) ) + ( Obj2.Velocity * 2 * m2 / ( m1 + m2 ) );
            EndVel2 = ( Obj1.Velocity * 2 * m1 / ( m1 + m2 ) ) + ( Obj2.Velocity * ( m2 - m1 ) / ( m1 + m2 ) );
        }

        public int Frame;
        public int Frames;
        private float _Progress;

        public PhysObj Obj1;
        public PhysObj Obj2;

        public Vector EndVel1;
        public Vector EndVel2;
        public Vector StartVel1;
        public Vector StartVel2;

        public void Progress( float FrameTime )
        {
            ++Frame;
            _Progress = (float) Frame / Frames;
            System.Diagnostics.Debug.Assert( _Progress <= 1.0f && _Progress > 0.0f );

            Obj1.Velocity = ( EndVel1 - StartVel1 ) / _Progress;
            Obj2.Velocity = ( EndVel2 - StartVel2 ) / _Progress;

            //Vector DeltaVel1 = NewVel1 - Obj1.Velocity;
            //Vector DeltaVel2 = NewVel2 - Obj2.Velocity;

            /*
            //newton's second law
            Vector Obj1Force = Obj1.Mass * DeltaVel1 / FrameTime;
            Vector Obj2Force = Obj2.Mass * DeltaVel2 / FrameTime;
            //newton's third law
            Obj1.NetForce -= Obj1Force;
            Obj2.NetForce += Obj1Force;
            Obj1.NetForce += Obj2Force;
            Obj2.NetForce -= Obj2Force;
            */
        }

        public static bool TestCollision( IEntHandle ent1, IEntHandle ent2, Vector offset1, Vector offset2 )
        {
            Vector[] Points1 = ent1.GetWorldVerts();
            Vector[] Points2 = ent2.GetWorldVerts();
            for ( int i = 0; i < Points1.Length; ++i )
                Points1[i] += offset1;
            for ( int i = 0; i < Points2.Length; ++i )
                Points2[i] += offset2;

            for ( int i = 0; i < ent1.Meshes.Length; ++i )
            {
                if ( !TestCollision( ent1.TransformDirection( ent1.Meshes[ i ].Normal ), Points1, Points2 ) )
                    return false;
            }
            for ( int i = 0; i < ent2.Meshes.Length; ++i )
            {
                if ( !TestCollision( ent2.TransformDirection( ent2.Meshes[ i ].Normal ), Points1, Points2 ) )
                    return false;
            }
            return true;
        }
        public static bool TestCollision( IEntHandle ent1, IEntHandle ent2 )
        {
            IEntHandle[] ents = { ent1, ent2 };

            Vector[] Points1 = ents[ 0 ].GetWorldVerts();
            Vector[] Points2 = ents[ 1 ].GetWorldVerts();

            for ( int EntIndex = 0; EntIndex < 2; ++EntIndex )
            {
                IEntHandle Ent = ents[ EntIndex ];
                for ( int i = 0; i < Ent.Meshes.Length; ++i )
                {
                    if ( !TestCollision( Ent.TransformDirection( Ent.Meshes[ i ].Normal ), Points1, Points2 ) )
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

/*
        public static void Collide( PhysObj Obj1, PhysObj Obj2, float dt )
        {
            float m1 = Obj1.Mass;
            float m2 = Obj2.Mass;

            //a normal vector of collision pointing out of obj1
            Plane CollisionPlane = Obj1.LinkedEnt.GetCollisionPlane( Obj2.LinkedEnt.GetAbsOrigin() );
            Vector Normal = CollisionPlane.Normal;

            Vector Vel1 = ( Obj1.Velocity * ( m1 - m2 ) / ( m1 + m2 ) ) + ( Obj2.Velocity * 2 * m2 / ( m1 + m2 ) );
            Vector Vel2 = ( Obj1.Velocity * 2 * m2 / ( m1 + m2 ) ) + ( Obj2.Velocity * ( m2 - m1 ) / ( m1 + m2 ) );

            //newton's second law
            Vector Obj1Force = Obj1.Mass * ( Obj1.Velocity - Vel1 ) / dt;
            Vector Obj2Force = Obj2.Mass * ( Obj2.Velocity - Vel2 ) / dt;
            //newton's third law
            Obj1.NetForce -= Obj1Force;
            Obj2.NetForce += Obj1Force;
            Obj1.NetForce += Obj2Force;
            Obj2.NetForce -= Obj2Force;

            Vector vLine = Obj1.LinkedEnt.GetAbsOrigin() - Obj2.LinkedEnt.GetAbsOrigin();
            Vector ptStart = Obj2.LinkedEnt.GetAbsOrigin();
            Vector ptOnPlane = CollisionPlane.ClosestPointOnPlane( Obj2.LinkedEnt.GetAbsOrigin() );
            Vector CollisionPoint = Vector.Dot( ( ptOnPlane - ptStart ), Normal ) / Vector.Dot( vLine, Normal ) * vLine + ptStart;

            Vector Radius1 = CollisionPoint - Obj1.LinkedEnt.GetAbsOrigin();
            Vector Radius2 = CollisionPoint - Obj2.LinkedEnt.GetAbsOrigin();

            Obj1.Torque += Vector.Cross( Obj1Force, Radius1 ) / 100;
            Obj2.Torque += Vector.Cross( Obj2Force, Radius2 ) / 100;

            //greenberg's first law (shove the objects away from each other)
            Obj1.Velocity -= Normal;
            Obj2.Velocity += Normal;

            //greenberg's second law (if two objects are penetrating, make it stop)
            if ( Collision.TestCollision( Obj1.LinkedEnt, Obj2.LinkedEnt ) )
            {
                Obj1.LinkedEnt.SetAbsOrigin( Obj1.LinkedEnt.GetAbsOrigin() - Normal / 100 );
                Obj2.LinkedEnt.SetAbsOrigin( Obj2.LinkedEnt.GetAbsOrigin() + Normal / 100 );
            }
        }
*/
