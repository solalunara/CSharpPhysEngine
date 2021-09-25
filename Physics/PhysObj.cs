using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderInterface;

namespace Physics
{
    public class PhysObj : BasePhysics
    {
        public static readonly Vector Default_Coeffs = new( 1, 1, 1 );

        const float GroundDragCoeff = 0.6f;
        const float AirDensity = 1.255f; //kg/m^3


        public PhysObj( BaseEntity LinkedEnt, Vector AirDragCoeffs, float Mass, float RotInertia, Vector Velocity ) : base( LinkedEnt )
        {
            this.AirDragCoeffs = AirDragCoeffs;
            this.Mass = Mass;
            this.RotInertia = RotInertia;
            this.Velocity = Velocity;
        }

        internal Vector LastAngVelocity;

        public void AddForceAtPoint( Vector Force, Vector WorldPt )
        {
            NetForce += Force;
            Vector Radius = WorldPt - LinkedEnt.GetAbsOrigin();
            Torque += Vector.Cross( Radius, Force );
        }

        public void DragSimulate( bool GroundFriction, Vector Gravity )
        {
            //Objects sometimes have a hard time coming to rest on a surface, because they'll keep alternating direction of rotation.
            //Here we try to help those objects out a little bit by detecting their condition then rounding their angles to rest nicely on the surface.
            if ( Vector.Dot( LastAngVelocity.Normalized(), AngularVelocity.Normalized() ) < -.75f )
            {
                AngularMomentum = new();
                Matrix Rot = LinkedEnt.GetAbsRot();
                for ( int i = 0; i < 3; ++i )
                {
                    for ( int j = 0; j < 3; ++j )
                    {
                        Rot.Columns[ i ][ j ] = MathF.Round( Rot.Columns[ i ][ j ] * 180 / MathF.PI ) * MathF.PI / 180;
                    }
                }
                LinkedEnt.SetAbsRot( Rot );
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
        public void Collide( BaseEntity OtherEnt, Vector Gravity )
        {
            Vector CollisionNormal = -BaseEntity.TestCollision( LinkedEnt, OtherEnt ).Item1;
            float CollisionDepth = BaseEntity.TestCollisionDepth( LinkedEnt, OtherEnt ).Item1;

            Plane CollisionPlane = OtherEnt.GetCollisionPlane( LinkedEnt.GetAbsOrigin() );
            //Is the center of mass above the object?
            bool COMAbove = OtherEnt.TestCollision( CollisionPlane.ClosestPointOnPlane( LinkedEnt.GetAbsOrigin() ) );

            //If the velocity is going into the plane, zero the component of the plane
            if ( Vector.Dot( CollisionPlane.Normal, Velocity ) <= 0 && COMAbove )
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

                if ( OtherEnt.TestCollision( WorldPts[ i ] ) )
                {
                    Dists[ i ] = CollisionPlane.DistanceFromPointToPlane( WorldPts[ i ] );
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
            LinkedEnt.SetAbsOrigin( LinkedEnt.GetAbsOrigin() + CollisionNormal * CollisionDepth );

            Vector Force = Vector.Dot( -Gravity * Mass, CollisionNormal ) * CollisionNormal; 
            AddForceAtPoint( Force, CollisionPoint );

        }

        

        public static List<(PhysObj, PhysObj)> GetCollisionPairs( BaseWorld world )
        {
            List<(PhysObj, PhysObj)> Pairs = new();
            BasePhysics[] PhysList = world.GetPhysObjList();
            for ( int i = 0; i < PhysList.Length; ++i )
            {
                for ( int j = i + 1; j < PhysList.Length; ++j )
                {
                    if ( BaseEntity.BinaryTestCollision( PhysList[ i ].LinkedEnt, PhysList[ j ].LinkedEnt ) )
                    {
                        Pairs.Add( ( (PhysObj)PhysList[ i ], (PhysObj)PhysList[ j ] ) );
                    }
                }
            }
            return Pairs;
        }
        
    }
}