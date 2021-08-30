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

        public PhysicsObject( EHandle LinkedEnt, Vector Gravity, Vector AirDragCoeffs, float Mass = 1.0f )
        {
            this.LinkedEnt = LinkedEnt;
            this.Gravity = Gravity;
            this.Mass = Mass;
            this.AirDragCoeffs = AirDragCoeffs;

            this.BaseVelocity = new Vector();
        }
        
        //collide a physics object with the world
        public void Collide( EHandle OtherEnt )
        {
            Plane collisionplane = OtherEnt.GetCollisionPlane( LinkedEnt.Transform.Position );
            //if velocity is already in the direction of the normal, don't reflect it
            if ( Vector.Dot( collisionplane.Normal, Velocity ) > 0 )
                return;

            //reflect the velocity about the normal of the collision plane
            Velocity -= 2.0f * Vector.Dot( Velocity, collisionplane.Normal ) * collisionplane.Normal;

            Vector CollisionPoint = collisionplane.ClosestPointOnPlane( LinkedEnt.Transform.Position );
            //anti-penetration
            if ( LinkedEnt.ent.AABB.TestCollisionPoint( CollisionPoint, LinkedEnt.Transform.Position ) )
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
                LinkedEnt.Transform.Position = CollisionPoint;
            }

            for ( int i = 0; i < 3; ++i )
            {
                if ( Math.Abs( collisionplane.Normal[ i ] ) > .9f )
                {
                    Velocity[ i ] = 0;
                    break;
                }
            }
        }


        public void Simulate( float dt, World world )
        {
            for( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                EHandle WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions
                if ( world.GetEntPhysics( WorldEnt ) != null )
                    continue; //physics objects have their own collision resolution

                if ( EHandle.TestCollision( WorldEnt, LinkedEnt ) ) //if (collision)
                {
                    Collide( WorldEnt );
                }
            }

            TestCollision( world, out bool Collision, out bool TopCollision );
            //if there is a (top) collision, a normal force acts on the object that cancels the gravitational force
            //this is mathematically equivalent to not doing the gravitational force (for simple newtonian mechanics)
            if ( !TopCollision )
                NetForce += Gravity * Mass;

            DragSimulate( dt, Collision );

            Velocity += NetForce / Mass * dt;
            LinkedEnt.Transform.Position += Velocity * dt;

            //reset the net force each frame
            for ( int i = 0; i < 3; ++i )
                NetForce[ i ] = 0;
        }

        //checks to see if this entity should collide with any entities in the world given it's current position
        public void TestCollision( World world, out bool Collision, out bool TopCollision )
        {
            Collision = false;
            TopCollision = false;

            if ( LinkedEnt.ent.FaceLength == 0 )
                return;

            for ( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                EHandle WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions

                if ( EHandle.TestCollision( WorldEnt, LinkedEnt ) )
                {
                    Collision = true;
                    if ( WorldEnt == world.player.Head || WorldEnt == world.player.Body.LinkedEnt )
                        continue; //player doesn't have faces, so don't check for collision normal
                    Vector vCollisionNormal = WorldEnt.GetCollisionNormal( LinkedEnt.Transform.Position );
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

            Vector Areas = new Vector();
            Vector Maxs = LinkedEnt.ent.AABB.maxs;
            Vector Mins = LinkedEnt.ent.AABB.mins;
            Areas.x = ( Maxs.y - Mins.y ) * ( Maxs.z - Mins.z );
            Areas.y = ( Maxs.x - Mins.x ) * ( Maxs.z - Mins.z );
            Areas.z = ( Maxs.x - Mins.x ) * ( Maxs.y - Mins.y );
            for ( int i = 0; i < 3; ++i )
            {
                int iWindSign = WindDir[ i ] > 0 ? 1 : -1;
                Drag[ i ] += .5f * AirDensity * Velocity[ i ] * Velocity[ i ] * AirDragCoeffs[ i ] * Areas[ i ] * iWindSign;
            }
            NetForce += Drag;
        }

        public static List<PhysicsObject[]> GetCollisionPairs( World world )
        {
            List<PhysicsObject[]> Pairs = new List<PhysicsObject[]>();
            for ( int i = 0; i < world.PhysicsObjects.Count; ++i )
            {
                for ( int j = i + 1; j < world.PhysicsObjects.Count; ++j )
                {
                    if ( EHandle.TestCollision( world.PhysicsObjects[ i ].LinkedEnt, world.PhysicsObjects[ j ].LinkedEnt ) )
                    {
                        PhysicsObject[] pair = { world.PhysicsObjects[ i ], world.PhysicsObjects[ j ] };
                        Pairs.Add( pair );
                    }
                }
            }
            return Pairs;
        }
        public static void Collide( PhysicsObject Obj1, PhysicsObject Obj2, float dt )
        {
            float m1 = Obj1.Mass;
            float m2 = Obj2.Mass;

            //a normal vector of collision pointing out of obj1
            Vector Normal = Obj1.LinkedEnt.GetCollisionNormal( Obj2.LinkedEnt.Transform.Position );

            Vector Vel1 = ( Obj1.Velocity * ( m1 - m2 ) / ( m1 + m2 ) ) + ( Obj2.Velocity * 2 * m2 / ( m1 + m2 ) );
            Vector Vel2 = ( Obj1.Velocity * 2 * m2 / ( m1 + m2 ) ) + ( Obj2.Velocity * ( m2 - m1 ) / ( m1 + m2 ) );

            Obj1.Velocity = Vel1;
            Obj2.Velocity = Vel2;

            //anti-penetration
            if ( EHandle.TestCollision( Obj1.LinkedEnt, Obj2.LinkedEnt ) )
            {
                Obj1.LinkedEnt.Transform.Position -= Normal / 100;
                Obj2.LinkedEnt.Transform.Position += Normal / 100;
            }
        }


        public EHandle LinkedEnt;
        public Vector Gravity;
        public Vector NetForce;
        public Vector AirDragCoeffs;
        public Vector Velocity;
        public Vector BaseVelocity;
        public float Mass;
    }
}
