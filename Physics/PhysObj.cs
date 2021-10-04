using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderInterface;
using static RenderInterface.SaveRestore;

namespace Physics
{
    public class PhysObj : BasePhysics
    {
        public static readonly Vector Default_Coeffs = Vector.One;

        const float GroundDragCoeff = 0.6f;
        const float AtmoDensity = 1.255f; //kg/m^3

        public PhysObj( BaseEntity LinkedEnt, Vector AirDragCoeffs, float Mass, float RotInertia, Vector Velocity, Vector Gravity ) : base( LinkedEnt )
        {
            this.AirDragCoeffs = AirDragCoeffs;
            this.Mass = Mass;
            this.RotInertia = RotInertia;
            this.Velocity = Velocity;
            this.Gravity = Gravity;
        }

        internal Vector LastAngVelocity;

        public void AddForceAtPoint( Vector Force, Vector WorldPt )
        {
            NetForce += Force;
            Vector Radius = WorldPt - LinkedEnt.GetAbsOrigin();
            Torque += Vector.Cross( Radius, Force );
        }
        public void AddImpulse( Vector Force, float ds )
        {
            // dv = Fdt / m
            Velocity += ( Force * ds ) / Mass;
        }

        public void DragSimulate( bool GroundFriction, Vector Gravity, float AirDensity = AtmoDensity )
        {
            //Objects sometimes have a hard time coming to rest on a surface, because they'll keep alternating direction of rotation.
            //Here we try to help those objects out a little bit by detecting their condition then rounding their angles to rest nicely on the surface.
            /*
            if ( Vector.Dot( LastAngVelocity.Normalized(), AngularVelocity.Normalized() ) < -.5f && GroundFriction )
            {
                AngularMomentum = new();
                Matrix Rot = LinkedEnt.GetAbsRot();
                for ( int i = 0; i < 2; ++i )
                {
                    for ( int j = 0; j < 2; ++j )
                    {
                        Rot.Columns[ i ][ j ] = MathF.Round( Rot.Columns[ i ][ j ] * 180 / MathF.PI ) * MathF.PI / 180;
                    }
                }
                LinkedEnt.SetAbsRot( Rot );
            }
            */

            //we can do this to simulate rotational drag because physframes are always the same time
            AngularMomentum *= .99f;

            //if the object is almost stopped, force it to stop.
            //drag in this case commonly overshoots.
            if ( AngularMomentum.Length() < 0.1f )
                AngularMomentum = new();
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
            Vector Direction = OtherEnt.GetAbsOrigin() - LinkedEnt.GetAbsOrigin();
            Vector CollisionNormal = BaseEntity.TestCollision( LinkedEnt, OtherEnt );
            if ( Vector.Dot( Direction.Normalized(), CollisionNormal ) > 0 )
                CollisionNormal = -CollisionNormal;
            float CollisionDepth = BaseEntity.TestCollisionDepth( LinkedEnt, OtherEnt );

            //If the velocity is going into the plane, zero the component of the plane
            if ( Vector.Dot( CollisionNormal, Velocity ) <= 0 )
            {
                for ( int i = 0; i < 3; ++i )
                {
                    if ( Math.Abs( CollisionNormal[ i ] ) > .75f )
                    {
                        Momentum[ i ] = 0.0f;
                        break;
                    }
                }
            }

            Vector[] WorldPts = LinkedEnt.GetWorldVerts();
            List<Vector> ContactPoints = new();
            for ( int i = 0; i < WorldPts.Length; ++i )
            {
                if ( OtherEnt.TestCollision( WorldPts[ i ] ) )
                    ContactPoints.Add( WorldPts[ i ] );
            }

            Vector CollisionPoint = new();
            if ( ContactPoints.Count == 0 ) //no points of contact
            {
                CollisionPoint = LinkedEnt.GetAbsOrigin();
            }
            else //points of contact
            {
                for ( int i = 0; i < ContactPoints.Count; ++i )
                    CollisionPoint += ContactPoints[ i ];
                CollisionPoint /= ContactPoints.Count;
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

        public static PhysObj FromBytes( byte[] Bytes, int ByteOffset = 0 )
        {
            int Index = ByteOffset;

            int EntSize = BytesToStruct<int>( Bytes, Index );
            Index += sizeof( int );

            BaseEntity LinkedEnt = BaseEntity.FromBytes( Bytes, Index );
            Index += EntSize;

            Vector Momentum = BytesToStruct<Vector>( Bytes, Index );
            Index += Marshal.SizeOf( Momentum );

            Vector AngularMomentum = BytesToStruct<Vector>( Bytes, Index );
            Index += Marshal.SizeOf( AngularMomentum );

            Vector AirDragCoeffs = BytesToStruct<Vector>( Bytes, Index );
            Index += Marshal.SizeOf( AirDragCoeffs );

            Vector AirVelocity = BytesToStruct<Vector>( Bytes, Index );
            Index += Marshal.SizeOf( AirVelocity );

            float Mass = BytesToStruct<float>( Bytes, Index );
            Index += sizeof( float );

            float RotInertia = BytesToStruct<float>( Bytes, Index );
            Index += sizeof( float );

            Vector Gravity = BytesToStruct<Vector>( Bytes, Index );

            PhysObj ret = new( LinkedEnt, AirDragCoeffs, Mass, RotInertia, Momentum / Mass, Gravity )
            {
                AngularMomentum = AngularMomentum,
                AirVelocity = AirVelocity
            };
            return ret;
        }
    }
}