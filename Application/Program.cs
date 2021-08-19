using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Application
{
    class Program
    {
        static readonly IntPtr NULL = IntPtr.Zero;
#pragma warning disable 0414 
        static readonly float Movespeed_Air = 5.0f;
        static readonly float Movespeed_Gnd = 20.0f;

        static bool CursorControl = true;
        static uint MoveTracker = (uint) Move.MOVE_NONE;
        static void Main( string[] args )
        {
            Render_Interface.Init( out IntPtr window, out IntPtr shader, out IntPtr camera, out IntPtr world, out IntPtr inputdata );
            Texture[] texture = { new Texture( "themasterpiece.png" ) };

            World w = new World( world );

            Brush b1 = new Brush( new Vector( -10, -10, -11 ), new Vector( 10, 10, -9 ), texture, w );
            Brush b2 = new Brush( new Vector( -10, -11, -10 ), new Vector( 10, -9, 10 ), texture, w );
            Player player = new Player( camera );

            InputHandle inptptr = Input;
            Render_Interface.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );

            PhysicsObject CamPhys = new PhysicsObject( player, PhysicsObject.Default_Gravity );
            // Pointer, so doesn't need to be updated each frame, the underlying object is changed
            Transform CamTransform = player.Transform;

            float lasttime = Render_Interface.GetTime();

            while ( !Render_Interface.ShouldTerminate( window ) )
            {
                float time = Render_Interface.GetTime();
                float frametime = time - lasttime;
                lasttime = time;

                
                bool Collision = CamPhys.Simulate( frametime );

                float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                Vector Force = new Vector();
                if ( ( MoveTracker & (uint) Move.MOVE_FORWARD ) != 0 )
                    Force += CamTransform.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_BACKWARD ) != 0 )
                    Force += CamTransform.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_LEFT ) != 0 )
                    Force += CamTransform.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_RIGHT ) != 0 )
                    Force += CamTransform.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                Force.y = 0;
                CamPhys.AddForce( Force );

                if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 )
                {
                    CamPhys.Velocity.y = 5.0f;
                }

                Render_Interface.RenderLoop( window, shader, camera, world, CursorControl );
            }
            Render_Interface.Terminate( window, shader, camera, world );
        }

        public delegate void InputHandle( IntPtr window, Keys key, int scancode, Actions act, int mods );
        public static void Input( IntPtr window, Keys key, int scancode, Actions act, int mods )
        {
            if ( act == Actions.PRESSED && key == Keys.ESCAPE ) //pressed
            {
                CursorControl = !CursorControl;
            }

            bool bSetToTrue = act == Actions.PRESSED || act == Actions.HELD;

            switch( key )
            {
                case Keys.W:
                    Render_Interface.SetFlag( ref MoveTracker, (uint) Move.MOVE_FORWARD, bSetToTrue );
                    break;
                case Keys.S:
                    Render_Interface.SetFlag( ref MoveTracker, (uint) Move.MOVE_BACKWARD, bSetToTrue );
                    break;
                case Keys.A:
                    Render_Interface.SetFlag( ref MoveTracker, (uint) Move.MOVE_LEFT, bSetToTrue );
                    break;
                case Keys.D:
                    Render_Interface.SetFlag( ref MoveTracker, (uint) Move.MOVE_RIGHT, bSetToTrue );
                    break;
                case Keys.SPACE:
                    Render_Interface.SetFlag( ref MoveTracker, (uint) Move.MOVE_JUMP, bSetToTrue );
                    break;
                default:
                    break;
            }
        }
    }
    public enum Keys
    {
        W = 87,
        S = 83,
        A = 65,
        D = 68,
        SPACE = 32,
        ESCAPE = 256,
    }
    public enum Actions
    {
        RELEASED = 0,
        PRESSED = 1,
        HELD = 2,
    }
    public enum Move
    {
        MOVE_NONE       = 0,
        MOVE_FORWARD    = 1 << 0,
        MOVE_BACKWARD   = 1 << 1,
        MOVE_LEFT       = 1 << 2,
        MOVE_RIGHT      = 1 << 3,
        MOVE_JUMP       = 1 << 4,
    }
}
