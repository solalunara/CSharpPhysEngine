using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    public class PhysicsObject
    {
        const float GroundDragCoeff = 0.6f;
        const float AirDensity = 1.255f; //kg/m^3
        
        public static readonly Vector Default_Gravity = new Vector( 0, -10, 0 );
        public static readonly Vector Default_Coeffs = new Vector( 1, 1, 1 );

        public PhysicsObject( BaseEntity LinkedEnt, Vector Gravity, Vector AirDragCoeffs, float Mass, float RotInertia )
        {
            this.LinkedEnt = LinkedEnt;
            this.Gravity = Gravity;
            this.Mass = Mass;
            this.AirDragCoeffs = AirDragCoeffs;
            this.RotInertia = RotInertia;

            this.BaseVelocity = new Vector();
        }

        public BaseEntity LinkedEnt;
        public Vector Gravity;
        public Vector NetForce;
        public Vector AirDragCoeffs;
        public Vector Velocity;
        public Vector BaseVelocity;
        public Vector AngularMomentum;
        public Vector Torque;
        public float RotInertia;
        public float Mass;

        //collide a physics object with the world
        public void Collide( BaseEntity OtherEnt )
        {
            Plane collisionplane = OtherEnt.GetCollisionPlane( LinkedEnt.GetAbsOrigin() );
            //if velocity is already in the direction of the normal, don't reflect it
            if ( Vector.Dot( collisionplane.Normal, Velocity ) > 0 )
                return;

            //reflect the velocity about the normal of the collision plane
            Velocity -= 2.0f * Vector.Dot( Velocity, collisionplane.Normal ) * collisionplane.Normal;

            /*
            Vector CollisionPoint = collisionplane.ClosestPointOnPlane( LinkedEnt.GetAbsOrigin() );
            //anti-penetration
            if ( LinkedEnt.AABB.TestCollisionPoint( CollisionPoint, LinkedEnt.GetAbsOrigin() ) )
            {
                Vector Mins = LinkedEnt.ent.AABB.mins;
                Vector Maxs = LinkedEnt.ent.AABB.maxs;
                for ( int i = 0; i < 3; ++i )
                {
                    if ( Math.Abs( collisionplane.Normal[ i ] ) > .9f )
                    {
                        if ( collisionplane.Normal[ i ] > .9f )
                            CollisionPoint[ i ] -= Mins[ i ];
                        else //if normal[i] < -.9f
                            CollisionPoint[ i ] -= Maxs[ i ];
                        break;
                    }
                }
                LinkedEnt.Transform.SetAbsPos( CollisionPoint );
            }
            */

            
            for ( int i = 0; i < 3; ++i )
            {
                if ( Math.Abs( collisionplane.Normal[ i ] ) > .9f )
                {
                    Velocity[ i ] = 0.0f;
                    break;
                }
            }
            

            if ( Collision.TestCollision( LinkedEnt, OtherEnt, collisionplane.Normal / 100, new Vector() ) )
                LinkedEnt.SetAbsOrigin( LinkedEnt.GetAbsOrigin() + collisionplane.Normal / 100 );
        }


        public void Simulate( float dt, World world )
        {
            if ( LinkedEnt.Parent != null )
                return;

            for( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                BaseEntity WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions
                if ( world.GetEntPhysics( WorldEnt ) != null )
                    continue; //physics objects have their own collision resolution

                if ( Collision.TestCollision( WorldEnt, LinkedEnt ) ) //if (collision)
                {
                    Collide( WorldEnt );
                }
            }

            TestCollision( world, out bool bCollision, out bool TopCollision );
            //if there is a (top) collision, a normal force acts on the object that cancels the gravitational force
            //this is mathematically equivalent to not doing the gravitational force (for simple newtonian mechanics)
            if ( !TopCollision )
                NetForce += Gravity * Mass;

            DragSimulate( dt, bCollision );

            Velocity += NetForce / Mass * dt;
            LinkedEnt.SetAbsOrigin( LinkedEnt.GetAbsOrigin() + Velocity * dt );

            AngularMomentum += Torque * dt;
            if ( AngularMomentum != new Vector() ) //only do if non-zero
            {
                Matrix.GLMRotMatrix( AngularMomentum.Length() / RotInertia, AngularMomentum.Normalized(), out Matrix rot );
                LinkedEnt.SetAbsRot( rot * LinkedEnt.GetAbsRot() );
            }

            //reset the net force each frame
            for ( int i = 0; i < 3; ++i )
            {
                NetForce[ i ] = 0;
                Torque[ i ] = 0;
            }
        }

        //checks to see if this entity should collide with any entities in the world given it's current position
        public void TestCollision( World world, out bool bCollision, out bool TopCollision )
        {
            bCollision = false;
            TopCollision = false;

            if ( LinkedEnt.Meshes.Length == 0 )
                return;

            for ( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                BaseEntity WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions
                if ( WorldEnt.Meshes.Length == 0 )
                    continue; //nothing to collide with

                if ( Collision.TestCollision( WorldEnt, LinkedEnt ) )
                {
                    bCollision = true;

                    Vector vCollisionNormal = WorldEnt.GetCollisionPlane( LinkedEnt.GetAbsOrigin() ).Normal;
                    if ( vCollisionNormal.y > .9f )
                        TopCollision = true;
                }
            }
        }

        //NOTE: Drag uses LOCAL velocity. If you want an object to not experience drag in a certain situation 
        //      (eg. being on top of an object) then set the BASE velocity.
        public void DragSimulate( float dt, bool GroundFriction )
        {
            Vector Drag = new Vector();


            //if the object is almost stopped, force it to stop.
            //drag in this case commonly overshoots.
            if ( Velocity.Length() < 0.1f )
            {
                Velocity = new Vector();
                return;
            }

            Vector WindDir = -Velocity.Normalized();
            //ground friction, only do on ground contact
            if ( GroundFriction )
                Drag += WindDir * Mass * Gravity.Length() * GroundDragCoeff;

            if ( LinkedEnt.GetType() == typeof( BoxEnt ) )
            {
                BoxEnt box = (BoxEnt)LinkedEnt;
                Vector Areas = new Vector();
                Vector Maxs = box.AABB.maxs;
                Vector Mins = box.AABB.mins;
                Areas.x = (Maxs.y - Mins.y) * (Maxs.z - Mins.z);
                Areas.y = (Maxs.x - Mins.x) * (Maxs.z - Mins.z);
                Areas.z = (Maxs.x - Mins.x) * (Maxs.y - Mins.y);
                for (int i = 0; i < 3; ++i)
                {
                    int iWindSign = WindDir[i] > 0 ? 1 : -1;
                    Drag[i] += .5f * AirDensity * Velocity[i] * Velocity[i] * AirDragCoeffs[i] * Areas[i] * iWindSign;
                }
            }
            
            
            NetForce += Drag;

            AngularMomentum *= (float) Math.Pow( .999f, 1/dt );
        }

        public static List<PhysicsObject[]> GetCollisionPairs( World world )
        {
            List<PhysicsObject[]> Pairs = new List<PhysicsObject[]>();
            for ( int i = 0; i < world.PhysicsObjects.Count; ++i )
            {
                for ( int j = i + 1; j < world.PhysicsObjects.Count; ++j )
                {
                    if ( Collision.TestCollision( world.PhysicsObjects[ i ].LinkedEnt, world.PhysicsObjects[ j ].LinkedEnt ) )
                    {
                        PhysicsObject[] pair = { world.PhysicsObjects[ i ], world.PhysicsObjects[ j ] };
                        Pairs.Add( pair );
                    }
                }
            }
            return Pairs;
        }
        public static void Collide( PhysicsObject Obj1, PhysicsObject Obj2, float dt, World world )
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

            if ( Obj1 != world.player.Body )
                Obj1.Torque += Vector.Cross( Radius1, Obj1Force );
            if ( Obj2 != world.player.Body )
                Obj2.Torque += Vector.Cross( Radius2, Obj2Force );

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
    }
}
