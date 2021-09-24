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
        internal static void Collide( PhysObj Obj1, PhysObj Obj2, float FrameTime )
        {
            Vector DeltaVel1 = -Obj1.Velocity;
            Vector DeltaVel2 = -Obj2.Velocity;

            //newton's second law
            Vector Obj1Force = Obj1.Mass * DeltaVel1 / FrameTime;
            Vector Obj2Force = Obj2.Mass * DeltaVel2 / FrameTime;
            //newton's third law
            Obj1.NetForce -= Obj1Force;
            Obj2.NetForce += Obj1Force;
            Obj1.NetForce += Obj2Force;
            Obj2.NetForce -= Obj2Force;
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
