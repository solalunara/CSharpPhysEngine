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
        internal Vector LastAngVelocity;

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
        public void AddForceAtPoint( Vector Force, Vector WorldPt )
        {
            NetForce += Force;
            Vector Radius = WorldPt - LinkedEnt.GetAbsOrigin();
            Vector tr = Vector.Cross( Radius, Force );
            Torque += Vector.Cross( Radius, Force );
        }

        public void DragSimulate( bool GroundFriction, Vector Gravity )
        {
            //Objects sometimes have a hard time coming to rest on a surface, because they'll keep alternating direction of rotation.
            //Here we try to help those objects out a little bit by detecting their condition then rounding their angles to rest nicely on the surface.
            if ( Vector.Dot( LastAngVelocity.Normalized(), AngularVelocity.Normalized() ) < -.75f )
            {
                AngularMomentum = new();
                Vector QAngles = LinkedEnt.LocalTransform.QAngles;
                for ( int i = 0; i < 3; ++i )
                    QAngles[ i ] = MathF.Round( QAngles[ i ] );
                LinkedEnt.LocalTransform.QAngles = QAngles;
            }

            //if the object is almost stopped, force it to stop.
            //drag in this case commonly overshoots.
            if ( AngularMomentum.Length() < 0.5f )
            {
                AngularMomentum = new();
            }
            if ( Velocity.Length() < 0.1f )
            {
                Velocity = new();
                return;
            }

            Vector Drag;

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

            //const float AirTorqueMultiplier = 0;
            //if ( AngularVelocity.Length() > 0.01f )
            //    Torque -= MathF.Pow( Area, (float) 5 / 2 ) * AngularVelocity * AngularVelocity.Length() * AirDensity * AirTorqueMultiplier;

            

            //ground friction, only do on ground contact
            if ( GroundFriction )
                Drag += WindDir * Mass * Gravity.Length() * GroundDragCoeff;

            NetForce += Drag;
        }

        //collide a physics object with the world
        public void Collide( IEntHandle OtherEnt, Vector Gravity )
        {
            Plane CollisionPlane = OtherEnt.GetCollisionPlane( LinkedEnt.GetAbsOrigin() );

            //If the velocity is going into the plane, zero the component of the plane
            if ( Vector.Dot( CollisionPlane.Normal, Velocity ) <= 0 )
            {
                for ( int i = 0; i < 3; ++i )
                {
                    if ( Math.Abs( CollisionPlane.Normal[ i ] ) > .75f )
                    {
                        Momentum[ i ] = 0.0f;
                        break;
                    }
                }
            }

            Vector[] WorldPts = LinkedEnt.GetWorldVerts();
            float[] Dists = new float[ WorldPts.Length ];
            List<int> PenetrationIndexes = new();
            for ( int i = 0; i < WorldPts.Length; ++i )
            {
                Dists[ i ] = CollisionPlane.DistanceFromPointToPlane( WorldPts[ i ] );

                if ( OtherEnt.TestCollision( WorldPts[ i ] ) )
                {
                    PenetrationIndexes.Add( i );
                }
            }

            Vector CollisionPoint = new();

            //technically the result would be the same, but this is just an optimization
            //since the case of 1 point is a lot simpler and doesn't require all the list allocations
            if ( PenetrationIndexes.Count == 1 ) //single point of contact
            {
                int PenetrationIndex = PenetrationIndexes[ 0 ];
                CollisionPoint = WorldPts[ PenetrationIndex ] + CollisionPlane.Normal * -Dists[ PenetrationIndex ];
            }
            else if ( PenetrationIndexes.Count > 0 ) //multiple points of contact
            {
                Vector[] CollisionPoints = new Vector[ PenetrationIndexes.Count ];
                for ( int i = 0; i < CollisionPoints.Length; ++i )
                    CollisionPoints[ i ] = WorldPts[ PenetrationIndexes[ i ] ];

                for ( int i = 0; i < CollisionPoints.Length; ++i )
                    CollisionPoint += CollisionPoints[ i ];
                CollisionPoint /= CollisionPoints.Length;

                CollisionPoint = CollisionPlane.ClosestPointOnPlane( CollisionPoint );
            }
            else //no obvious point of contact
            {
                CollisionPoint = CollisionPlane.ClosestPointOnPlane( LinkedEnt.GetAbsOrigin() );
            }

            //solve penetration
            LinkedEnt.SetAbsOrigin( LinkedEnt.GetAbsOrigin() + CollisionPlane.Normal * -Dists.Min() );

            Vector Force = Vector.Dot( -Gravity * Mass, CollisionPlane.Normal ) * CollisionPlane.Normal; 
            AddForceAtPoint( Force, CollisionPoint );

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
                    if ( vCollisionNormal.y > .7f )
                        TopCollision = true;
                }
            }
        }

        public static List<PhysObj[]> GetCollisionPairs( IWorldHandle world )
        {
            List<PhysObj[]> Pairs = new();
            IPhysHandle[] PhysList = world.GetPhysObjList();
            for ( int i = 0; i < PhysList.Length; ++i )
            {
                for ( int j = i + 1; j < PhysList.Length; ++j )
                {
                    if ( Collision.TestCollision( PhysList[ i ].LinkedEnt, PhysList[ j ].LinkedEnt ) )
                    {
                        PhysObj[] pair = { (PhysObj) PhysList[ i ], (PhysObj) PhysList[ j ] };
                        Pairs.Add( pair );
                    }
                }
            }
            return Pairs;
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