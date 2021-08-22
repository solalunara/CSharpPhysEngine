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
            Renderer.Init( out IntPtr window, out Shader shader, out Player player );
            Texture[] texture = { new Texture( "themasterpiece.png" ) };

            EHandle b1 = new EHandle( new Vector( -10, -10, -11 ), new Vector( 10, 10, -9 ), texture );
            EHandle b2 = new EHandle( new Vector( -10, -11, -10 ), new Vector( 10, -9, 10 ), texture );
            EHandle[] brushes = { b1, b2 };

            EHandle PlayerHandle = new EHandle( player.LinkedEnt );

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );

            PhysicsObject CamPhys = new PhysicsObject( PlayerHandle, PhysicsObject.Default_Gravity );

            float lasttime = Renderer.GetTime();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                float time = Renderer.GetTime();
                float frametime = time - lasttime;
                lasttime = time;

                PlayerHandle.ent.transform.Rotation = player.LinkedEnt.transform.Rotation; //c++ code does rotation with mouse, so copy change over here
                BaseEntity[] EntList = { b1.ent, b2.ent };


                bool Collision = CamPhys.Simulate( frametime, brushes );

                float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                Vector Force = new Vector();
                if ( ( MoveTracker & (uint) Move.MOVE_FORWARD ) != 0 )
                    Force += player.LinkedEnt.transform.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_BACKWARD ) != 0 )
                    Force += player.LinkedEnt.transform.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_LEFT ) != 0 )
                    Force += player.LinkedEnt.transform.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                if ( ( MoveTracker & (uint) Move.MOVE_RIGHT ) != 0 )
                    Force += player.LinkedEnt.transform.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                Force.y = 0;
                CamPhys.NetForce += Force;

                if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 )
                {
                    CamPhys.Velocity.y = 5.0f;
                }
                player.LinkedEnt.transform.Position = PlayerHandle.ent.transform.Position;
                Renderer.RenderLoop( window, shader, ref player, EntList, EntList.Length, CursorControl );
            }
            Renderer.Terminate();
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
                    Renderer.SetFlag( ref MoveTracker, (uint) Move.MOVE_FORWARD, bSetToTrue );
                    break;
                case Keys.S:
                    Renderer.SetFlag( ref MoveTracker, (uint) Move.MOVE_BACKWARD, bSetToTrue );
                    break;
                case Keys.A:
                    Renderer.SetFlag( ref MoveTracker, (uint) Move.MOVE_LEFT, bSetToTrue );
                    break;
                case Keys.D:
                    Renderer.SetFlag( ref MoveTracker, (uint) Move.MOVE_RIGHT, bSetToTrue );
                    break;
                case Keys.SPACE:
                    Renderer.SetFlag( ref MoveTracker, (uint) Move.MOVE_JUMP, bSetToTrue );
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
