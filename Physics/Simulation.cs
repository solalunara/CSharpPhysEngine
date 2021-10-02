global using static System.Diagnostics.Debug;
using System;
using System.Collections.Generic;
using System.Timers;
using RenderInterface;

namespace Physics
{
    class SimulationData
    {
        public SimulationData( BaseWorld world, float PhysSimTime )
        {
            this.world = world;
            this.PhysSimTime = PhysSimTime;
        }
        public BaseWorld world;
        public float PhysSimTime;
        public bool Paused = false;
    }
    public class PhysicsSimulator
    {
        public PhysicsSimulator( float PhysFrameTime, BaseWorld world, PhysicsEnvironment Environment  )
        {
            this.Environment = Environment;
            Environment.Data = new( world, PhysFrameTime );
            timer = new( PhysFrameTime );
            timer.Elapsed += Environment.Simulate;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        ~PhysicsSimulator()
        {
            timer.Dispose();
        }

        public PhysicsEnvironment Environment;

        private readonly Timer timer;

        public void SetPause( bool Paused )
        {
            timer.AutoReset = !Paused;
            Environment.Data.Paused = !timer.AutoReset;
        }
        public bool Paused() => !timer.AutoReset;
        public void SetFrameTime( float ft )
        {
            timer.Interval = ft;
            Environment.Data.PhysSimTime = ft;
        }
        public void Close() => timer.Dispose();
    }

    public class PhysicsEnvironment
    {
        public static readonly Vector Default_Gravity = new( 0, -10, 0 );

        public PhysicsEnvironment()
        {
            PObjs = new();
            LastSimTime = DateTime.Now;
        }

        public DateTime LastSimTime;

        public List<BasePhysics> PObjs;

        internal SimulationData Data;

        public void Simulate( object source, ElapsedEventArgs e )
        {
            try
            {
                LastSimTime = DateTime.Now;
                float dt = Data.PhysSimTime;
                BaseWorld world = Data.world;
                //Assert( !Data.Paused ); //shouldn't be called if we're paused

                List<(PhysObj, PhysObj)> Pairs = PhysObj.GetCollisionPairs( world );
                foreach ( (PhysObj, PhysObj) Pair in Pairs )
                {
                    Collision.Collide( Pair.Item1, Pair.Item2, dt );
                }

                foreach ( PhysObj p in PObjs )
                {
                    if ( p.LinkedEnt.Parent != null )
                        continue; //don't update physics objects in hierarchy 

                    bool Collide = false;
                    foreach ( BaseEntity ent in world.GetEntList() )
                    {
                        if ( ent == p.LinkedEnt )
                            continue; //prevent self collisions
                        if ( world.GetEntPhysics( ent ) != null )
                            continue; //physics objects have their own collision detection

                        if ( BaseEntity.BinaryTestCollision( ent, p.LinkedEnt ) )
                        {
                            Collide = true;
                            p.Collide( ent, p.Gravity );
                        }
                    }

                    //p.TestCollision( world, out _, out bool TopCollision );

                    p.NetForce += p.Gravity * p.Mass;

                    p.Momentum += p.NetForce * dt;
                    p.LinkedEnt.SetAbsOrigin( p.LinkedEnt.GetAbsOrigin() + p.Velocity * dt );

                    p.AngularMomentum += p.Torque * dt;
                    if ( p.AngularVelocity.Length() > 0.1f )
                    {
                        p.LinkedEnt.SetAbsRot( Matrix.RotMatrix( p.AngularVelocity.Length() * dt * 180 / MathF.PI, p.AngularMomentum.Normalized() ) * p.LinkedEnt.GetAbsRot() );
                    }


                    p.Torque = new();
                    p.NetForce = new();
                    p.ClearChannels();

                    p.DragSimulate( Collide, p.Gravity );
                    p.LastAngVelocity = p.AngularVelocity;
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex );
            }
        }


    }
}
