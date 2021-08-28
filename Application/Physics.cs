using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    class PhysicsObject
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
        
        public void Collide( EHandle OtherEnt, float ReflectValue = 0.0f )
        {
            Plane collisionplane = OtherEnt.AABB.GetCollisionPlane( LinkedEnt.Transform.Position, OtherEnt.Transform.Position );
            //if velocity is already in the direction of the normal, don't reflect it
            if ( Vector.Dot( collisionplane.Normal, Velocity ) > 0 )
                return;

            //reflect the velocity about the normal of the collision plane
            Velocity -= 2.0f * Vector.Dot( Velocity, collisionplane.Normal ) * collisionplane.Normal;

            Vector NewPos = collisionplane.ClosestPointOnPlane( LinkedEnt.Transform.Position );

            Vector Mins = LinkedEnt.AABB.Mins;
            Vector Maxs = LinkedEnt.AABB.Maxs;

            // technically a lot of the "if"s in the "else if"s aren't neccesary, but they help with readability
            if ( Math.Abs( collisionplane.Normal.x ) > .9f )
            {
                if ( collisionplane.Normal.x > .9f )
                    NewPos.x -= Mins.x;
                else if ( collisionplane.Normal.x < -.9f )
                    NewPos.x -= Maxs.x;

                Velocity.x *= ReflectValue;
            }
            else if ( Math.Abs( collisionplane.Normal.y ) > .9f )
            {
                if ( collisionplane.Normal.y > .9f )
                    NewPos.y -= Mins.y;
                else if ( collisionplane.Normal.y < -.9f )
                    NewPos.y -= Maxs.y;

                Velocity.y *= ReflectValue;
            }
            else if ( Math.Abs( collisionplane.Normal.z ) > .9f )
            {
                if ( collisionplane.Normal.z > .9f )
                    NewPos.z -= Mins.z;
                else if ( collisionplane.Normal.z < -.9f )
                    NewPos.z -= Maxs.z;

                Velocity.z *= ReflectValue;
            }
            LinkedEnt.Transform.Position = NewPos;
        }
        //returns true if there was a collision
        public void Simulate( float dt, World world )
        {
            for( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                EHandle WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions

                if ( WorldEnt.AABB.TestCollision( WorldEnt.Transform.Position, LinkedEnt.AABB,  LinkedEnt.Transform.Position ) ) //if (collision)
                {
                    Collide( WorldEnt );
                }
            }
            Player p = world.player;
            if ( p != LinkedEnt && p.AABB.TestCollision( p.Transform.Position, LinkedEnt.AABB, LinkedEnt.Transform.Position ) )
            {
                Collide( p );
            }

            bool Collision = TestCollision( world, out bool TopCollision );
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
        public static void SimulateWorld( float dt, World world )
        {
            for ( int i = 0; i < world.PhysicsObjects.Count; ++i )
                world.PhysicsObjects[ i ].Simulate( dt, world );
        }

        //checks to see if this entity should collide with any entities in the world given it's current position
        public bool TestCollision( World world, out bool TopCollision )
        {
            bool Collision = false;
            TopCollision = false;
            for ( int i = 0; i < world.WorldEnts.Count; ++i )
            {
                EHandle WorldEnt = world.WorldEnts[ i ];
                if ( WorldEnt == LinkedEnt )
                    continue; //prevent self collisions

                if ( WorldEnt.AABB.TestCollision( WorldEnt.Transform.Position, LinkedEnt.AABB,  LinkedEnt.Transform.Position ) ) //if (collision)
                {
                    Collision = true;
                    Vector vCollisionNormal = WorldEnt.AABB.GetCollisionNormal( LinkedEnt.Transform.Position, WorldEnt.Transform.Position );
                    if ( vCollisionNormal.y > .9f )
                        TopCollision = true;
                }
            }
            Player p = world.player;
            if ( p != LinkedEnt && p.AABB.TestCollision( p.Transform.Position, LinkedEnt.AABB, LinkedEnt.Transform.Position ) )
            {
                Collision = true;
            }
            return Collision;
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
            Vector Maxs = LinkedEnt.AABB.Maxs;
            Vector Mins = LinkedEnt.AABB.Mins;
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


        public EHandle LinkedEnt;
        public Vector Gravity;
        public Vector NetForce;
        public Vector AirDragCoeffs;
        public Vector Velocity;
        public Vector BaseVelocity;
        public float Mass;
    }
}
