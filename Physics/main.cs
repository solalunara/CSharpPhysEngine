using System;
using System.Collections.Generic;
using System.Threading;

namespace Physics
{
    public class PhysicsSimulator
    {
        public PhysicsSimulator( float PhysFrameTime, PhysicsEnvironment SimEnvironment )
        {
            _frametime = PhysFrameTime;
            int iPhysFrameTime = (int) ( PhysFrameTime * 1000 );
            TimerCallback Runnable = SimEnvironment.Simulate;
            timer = new( Runnable, PhysFrameTime, 0, iPhysFrameTime );
        }

        private float _frametime;
        private bool _Paused;
        private readonly Timer timer;

        public void SetPause( bool Paused )
        {
            if ( Paused == _Paused )
                return;

            _Paused = Paused;
            int iPhysFrameTime = (int) ( _frametime * 1000 );
            timer.Change( 0, Paused ? Timeout.Infinite : iPhysFrameTime );
        }
        public void SetFrameTime( float ft )
        {
            if ( ft == _frametime )
                return;

            _frametime = ft;

            if ( _Paused )
                return;

            int iPhysFrameTime = (int) ( _frametime * 1000 );
            timer.Change( 0, iPhysFrameTime );
        }
    }

    public class PhysicsEnvironment
    {
        public PhysicsEnvironment()
        {

        }


        public List<PhysObj> PObjs = new();

        public void Simulate( object StateInfo )
        {
            float dt = (float) StateInfo;
        }
    }
}
