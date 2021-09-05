using System;
using System.Collections.Generic;
using System.Timers;
using RenderInterface;

namespace Physics
{
    class SimulationData
    {
        public SimulationData( IWorldHandle world, float PhysSimTime )
        {
            this.world = world;
            this.PhysSimTime = PhysSimTime;
        }
        public IWorldHandle world;
        public float PhysSimTime;
        public bool Paused = false;
    }
    public class PhysicsSimulator
    {
        public PhysicsSimulator( float PhysFrameTime, PhysicsEnvironment SimEnvironment, IWorldHandle world )
        {
            int iPhysFrameTime = (int) ( PhysFrameTime * 1000 );
            //timer = new( SimEnvironment.Simulate, Data, 0, iPhysFrameTime );
            SimEnvironment.Data = new( world, PhysFrameTime );
            timer = new( PhysFrameTime );
            timer.Elapsed += SimEnvironment.Simulate;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        ~PhysicsSimulator()
        {
            timer.Dispose();
        }

        private readonly Timer timer;

        public void SetPause( bool Paused )
        {
            timer.AutoReset = !Paused;
        }
        public void SetFrameTime( float ft )
        {
            timer.Interval = ft;
        }
    }

    public class PhysicsEnvironment
    {
        public static readonly Vector Default_Gravity = new( 0, -10, 0 );

        public PhysicsEnvironment( Vector Gravity )
        {
            PObjs = new();
            this.Gravity = Gravity;
        }
        public PhysicsEnvironment( List<PhysObj> list, Vector Gravity )
        {
            PObjs = list;
            this.Gravity = Gravity;
        }

        public List<PhysObj> PObjs;
        public Vector Gravity;
        internal SimulationData Data;

        public void Simulate( object source, ElapsedEventArgs e )
        {
            float dt = Data.PhysSimTime;
            IWorldHandle world = Data.world;
            System.Diagnostics.Debug.Assert( !Data.Paused ); //shouldn't be called if we're paused

            foreach( PhysObj p in PObjs )
            {
                bool Collide = false;
                foreach ( IEntHandle ent in world.GetEntList() )
                {
                    if ( Collision.TestCollision( ent, p.LinkedEnt ) )
                    {
                        Collide = true;
                        p.Collide( ent );
                    }
                }


                p.TestCollision( world, out _, out bool TopCollision );
                if ( !TopCollision )
                    p.NetForce += Gravity * p.Mass;

                p.Momentum += p.NetForce * dt;
                p.LinkedEnt.SetAbsOrigin( p.LinkedEnt.GetAbsOrigin() + p.Velocity * dt );

                p.AngularMomentum += p.Torque * dt;
                if ( p.AngularVelocity != new Vector() )
                {
                    p.LinkedEnt.SetAbsRot( Matrix.RotMatrix( p.AngularVelocity.Length(), p.AngularVelocity.Normalized() ) * p.LinkedEnt.GetAbsRot() );
                }


                p.Torque = new();
                p.NetForce = new();
                p.ForceChannels.Clear();

                p.DragSimulate( Collide, Gravity );
            }
        }


    }
}
