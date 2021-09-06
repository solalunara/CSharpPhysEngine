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
            Collisions = new();
            this.Gravity = Gravity;
            LastSimTime = DateTime.Now;
        }

        public DateTime LastSimTime;

        public List<PhysObj> PObjs;
        public Vector Gravity;

        internal List<Collision> Collisions;
        internal SimulationData Data;

        public void Simulate( object source, ElapsedEventArgs e )
        {
            LastSimTime = DateTime.Now;
            float dt = Data.PhysSimTime;
            IWorldHandle world = Data.world;
            System.Diagnostics.Debug.Assert( !Data.Paused ); //shouldn't be called if we're paused

            List<PhysObj[]> Pairs = PhysObj.GetCollisionPairs( world );
            foreach ( PhysObj[] Pair in Pairs )
            {
                bool NewCollision = true;
                foreach ( Collision c in Collisions )
                {
                    if ( ( Pair[ 0 ] == c.Obj1 && Pair[ 1 ] == c.Obj2 ) || ( Pair[ 0 ] == c.Obj2 && Pair[ 1 ] == c.Obj1 ) )
                        NewCollision = false;
                }
                if ( !NewCollision )
                    continue;

                Collisions.Add( new Collision( 100, Pair[ 0 ], Pair[ 1 ] ) );
            }

            for ( int i = Collisions.Count; --i >= 0; )
            {
                Collisions[ i ].Progress( dt );
                if ( Collisions[ i ].Frame >= Collisions[ i ].Frames )
                {
                    Collisions.Remove( Collisions[ i ] );
                }
            }

            foreach ( PhysObj p in PObjs )
            {
                if ( p.LinkedEnt.Parent != null )
                    continue; //don't update physics objects in hierarchy 

                bool Collide = false;
                foreach ( IEntHandle ent in world.GetEntList() )
                {
                    if ( ent == p.LinkedEnt )
                        continue; //prevent self collisions
                    if ( world.GetEntPhysics( ent ) != null )
                        continue; //physics objects have their own collision detection

                    if ( Collision.TestCollision( ent, p.LinkedEnt ) )
                    {
                        Collide = true;
                        p.Collide( ent, Gravity );
                    }
                }

                p.TestCollision( world, out _, out bool TopCollision );

                p.NetForce += Gravity * p.Mass;

                p.Momentum += p.NetForce * dt;
                p.LinkedEnt.SetAbsOrigin( p.LinkedEnt.GetAbsOrigin() + p.Velocity * dt );

                p.AngularMomentum += p.Torque * dt;
                if ( p.AngularVelocity.Length() > 0.1f )
                {
                    p.LinkedEnt.SetAbsRot( Matrix.RotMatrix( p.AngularVelocity.Length() * 180 / MathF.PI, p.AngularMomentum.Normalized() ) * p.LinkedEnt.GetAbsRot() );
                }


                p.Torque = new();
                p.NetForce = new();
                p.ForceChannels.Clear();

                p.DragSimulate( Collide, Gravity );
            }
        }


    }
}
