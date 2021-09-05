using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderInterface;

namespace Physics
{
    public class PhysObj : IPhysHandle
    {
        public static readonly Vector Default_Coeffs = new( 1, 1, 1 );

        const float GroundDragCoeff = 0.6f;
        const float AirDensity = 1.255f; //kg/m^3


        public PhysObj( IEntHandle LinkedEnt, Vector AirDragCoeffs, float Mass, float RotInertia, Vector Velocity )
        {
            this.LinkedEnt = LinkedEnt;
            this.AirDragCoeffs = AirDragCoeffs;
            this.Mass = Mass;
            this.RotInertia = RotInertia;
            this.Velocity = Velocity;
            ForceChannels = new();
        }

        private IEntHandle _LinkedEnt;
        public IEntHandle LinkedEnt
        {
            get => _LinkedEnt;
            set => _LinkedEnt = value;
        }

        public Vector AirDragCoeffs;

        public float Mass;
        public float RotInertia;

        public Vector Momentum;
        public Vector Velocity
        {
            get => Momentum / Mass;
            set => Momentum = value * Mass;
        }

        public Vector AirVelocity;

        public Vector AngularMomentum;
        public Vector AngularVelocity
        {
            get => AngularMomentum / RotInertia;
            set => AngularMomentum = value * RotInertia;
        }

        internal Vector NetForce;
        public Vector Torque;

        internal List<int> ForceChannels;
        public void AddForce( Vector force, int Channel )
        {
            if ( !ForceChannels.Contains( Channel ) )
            {
                NetForce += force;
                ForceChannels.Add( Channel );
            }
        }

        public void DragSimulate( bool GroundFriction, Vector Gravity )
        {
            Vector Drag;
            //if the object is almost stopped, force it to stop.
            //drag in this case commonly overshoots.
            if ( Velocity.Length() < 0.1f )
            {
                Velocity = new();
                return;
            }

            //find the relative velocity through the air
            Vector ModVel = Velocity - AirVelocity;
            Vector WindDir = -ModVel.Normalized();
            Plane WindPlane = new( WindDir, 0 );

            //get the area along the wind direction
            Vector[] WorldPts = LinkedEnt.GetVerts();
            Vector[] ProjectedPts = new Vector[ WorldPts.Length ];
            for ( int j = 0; j < ProjectedPts.Length; ++j )
            {
                ProjectedPts[ j ] = WindPlane.ClosestPointOnPlane( WorldPts[ j ] );
            }
            Vector EntNormal = Vector.Cross( ProjectedPts[ ^1 ], ProjectedPts[ 0 ] );
            for ( int j = 1; j < ProjectedPts.Length; ++j )
            {
                EntNormal += Vector.Cross( ProjectedPts[ j - 1 ], ProjectedPts[ j ] );
            }
            float Area = EntNormal.Length() * 0.5f;

            //apply air drag
            Drag = .5f * AirDensity * -ModVel * ModVel.Length() * Area;
            for ( int i = 0; i < 3; ++i )
                Drag[ i ] *= AirDragCoeffs[ i ];

            //ground friction, only do on ground contact
            if ( GroundFriction )
                Drag += WindDir * Mass * Gravity.Length() * GroundDragCoeff;

            NetForce += Drag;
        }

        //collide a physics object with the world
        public void Collide( IEntHandle OtherEnt )
        {
            Plane collisionplane = OtherEnt.GetCollisionPlane( LinkedEnt.GetAbsOrigin() );
            //if velocity is already in the direction of the normal, don't reflect it
            if ( Vector.Dot( collisionplane.Normal, Velocity ) > 0 )
                return;

            //reflect the velocity about the normal of the collision plane
            Velocity -= 2.0f * Vector.Dot( Velocity, collisionplane.Normal ) * collisionplane.Normal;

            for ( int i = 0; i < 3; ++i )
            {
                if ( Math.Abs( collisionplane.Normal[ i ] ) > .9f )
                {
                    //Momentum[ i ] = 0.0f;
                    break;
                }
            }

            //if ( Collision.TestCollision( LinkedEnt, OtherEnt, collisionplane.Normal / 100, new Vector() ) )
            //    LinkedEnt.SetAbsOrigin( LinkedEnt.GetAbsOrigin() + collisionplane.Normal / 100 );
        }

        public void TestCollision( IWorldHandle world, out bool bCollision, out bool TopCollision )
        {
            bCollision = false;
            TopCollision = false;

            if ( LinkedEnt.Meshes.Length == 0 )
                return;

            for ( int i = 0; i < world.GetEntList().Length; ++i )
            {
                IEntHandle WorldEnt = world.GetEntList()[ i ];
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

        public static List<PhysObj[]> GetCollisionPairs( IWorldHandle world )
        {
            List<PhysObj[]> Pairs = new();
            for ( int i = 0; i < world.GetEntList().Length; ++i )
            {
                for ( int j = i + 1; j < world.GetEntList().Length; ++j )
                {
                    if ( Collision.TestCollision( world.GetPhysObjList()[ i ].LinkedEnt, world.GetPhysObjList()[ j ].LinkedEnt ) )
                    {
                        PhysObj[] pair = { (PhysObj) world.GetPhysObjList()[ i ], (PhysObj) world.GetPhysObjList()[ j ] };
                        Pairs.Add( pair );
                    }
                }
            }
            return Pairs;
        }
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