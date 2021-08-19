using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    class PhysicsObject
    {
        public static readonly Vector Default_Gravity = new Vector( 0, -10, 0 );
        public PhysicsObject( BaseEntity LinkedEnt, Vector Gravity, float Mass = 1.0f )
        {
            this.Gravity = Gravity;
            this.NetForce = new Vector();
            this.Mass = Mass;
            this.LinkedEnt = LinkedEnt;
            this.Velocity = new Vector();
            this.BaseVelocity = new Vector();
        }
        public void Collide( BaseEntity OtherEnt, float ReflectValue = 0.5f )
        {
            Plane collisionplane = OtherEnt.BBox.GetCollisionPlane( LinkedEnt.Transform.Position, OtherEnt.IsBrush() ? new Vector() : OtherEnt.Transform.Position );
            //if velocity is already in the direction of the normal, don't reflect it
            if ( Vector.Dot( collisionplane.Normal, Velocity ) > 0 )
                return;

            //reflect the velocity about the normal of the collision plane
            Velocity -= 2.0f * Vector.Dot( Velocity, collisionplane.Normal ) * collisionplane.Normal;

            Vector NewPos = collisionplane.ClosestPointOnPlane( LinkedEnt.Transform.Position );

            Vector Mins = LinkedEnt.BBox.mins;
            Vector Maxs = LinkedEnt.BBox.maxs;

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
            LinkedEnt.Transform.SetPosition( NewPos );
        }
        public void AddForce( Vector Force )
        {
            NetForce += Force;
        }
        //returns true if there was a collision
        public bool Simulate( float dt )
        {
            bool Collision = false;

            World world = LinkedEnt.World;
            uint worldsize = world.Size;
            for( int i = 0; i < worldsize; ++i )
            {
                IntPtr WorldEnt = world.GetEntAtIndex( i );
                BaseEntity worldent = new BaseEntity( false, false, WorldEnt );
                if ( WorldEnt == LinkedEnt.LinkedEnt )
                    continue; //prevent self collisions

                if ( LinkedEnt.BBox.TestCollisionAABB( worldent.BBox, LinkedEnt.Transform.Position, new Vector() /*Brush bboxes are defined in world space*/ ) ) //if (collision)
                {
                    Collision = true;
                    Collide( worldent );
                }
            }

            //if there is a collision, a normal force acts on the object that cancels the gravitational force
            //this is mathematically equivalent to not doing the gravitational force (for simple newtonian mechanics)
            if ( !Collision )
                NetForce += Gravity * Mass;

            Velocity += NetForce / Mass * dt;
            LinkedEnt.Transform.AddPosition( Velocity * dt );

            //reset the net force each frame
            for ( int i = 0; i < 3; ++i )
                NetForce[ i ] = 0;

            return Collision;
        }

        //NOTE: Drag uses LOCAL velocity. If you want an object to not experience drag in a certain situation 
        //      (eg. being on top of an object) then set the BASE velocity.
        public void DragSimulate( float dt )
        {

        }

        public Vector Gravity;
        private Vector NetForce;
        public float Mass;

        private readonly BaseEntity LinkedEnt;
        public Vector Velocity;
        public Vector BaseVelocity;
    }
}
