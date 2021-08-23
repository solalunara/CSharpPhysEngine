using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PhysEngine
{
    class Program
    {
        static bool CursorControl = true;
        static uint MoveTracker = (uint) Move.MOVE_NONE;
        static Player player;

        const float fov = 75.0f;
        const float Movespeed_Air = 5.0f;
        const float Movespeed_Gnd = 10.0f;


        static void Main( string[] args )
        {
            Renderer.Init( out IntPtr window, out Shader shader );
            Texture[] texture = { new Texture( "themasterpiece.png" ) };

            EHandle b1 = new EHandle( new Vector( -10, -10, -11 ), new Vector( 10, 10, -9 ), texture );
            EHandle b2 = new EHandle( new Vector( -10, -11, -10 ), new Vector( 10, -9, 10 ), texture );
            EHandle[] brushes = { b1, b2 };

            Renderer.GetWindowSize( window, out int width, out int height );
            Util.MakePerspective( fov, (float) width / height, 0.01f, 1000.0f, out Matrix persp );
            player = new Player( new THandle( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ), persp );

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( Marshal.GetFunctionPointerForDelegate( wndptr ) );

            PhysicsObject CamPhys = new PhysicsObject( player.LinkedEnt, PhysicsObject.Default_Gravity, PhysicsObject.Default_Coeffs, 50.0f );

            float lasttime = Renderer.GetTime();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                float time = Renderer.GetTime();
                float frametime = time - lasttime;
                lasttime = time;
                if ( frametime > 1.0f )
                    frametime = 0; //most likely debugging

                BaseEntity[] EntList = { b1.ent, b2.ent };
                bool Collision = false;
                if ( CursorControl )
                {
                    Mouse.HideMouse( window );
                    const float LookSpeed = 10.0f;
                    Mouse.GetMouseOffset( window, out double x, out double y );
                    Vector Up = player.LinkedEnt.ent.transform.InverseTransformDirection( new Vector( 0, 1, 0 ) );
                    Vector Right = new Vector( -1, 0, 0 );
                    Util.MakeRotMatrix( (float) ( frametime * LookSpeed * -x ), Up, out Matrix XRot );
                    Util.MakeRotMatrix( (float) ( frametime * LookSpeed * y ), Right, out Matrix YRot );
                    Util.MultiplyMatrix( ref player.LinkedEnt.ent.transform.Rotation, XRot );
                    Util.MultiplyMatrix( ref player.LinkedEnt.ent.transform.Rotation, YRot );
                    Transform.UpdateTransform( ref player.LinkedEnt.ent.transform );
                    Mouse.MoveMouseToCenter( window );

                    Collision = CamPhys.Simulate( frametime, brushes );
                }
                else
                    Mouse.ShowMouse( window );

                if ( CursorControl )
                {
                    float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                    Vector Force = new Vector();
                    if ( ( MoveTracker & (uint) Move.MOVE_FORWARD ) != 0 )
                        Force += player.LinkedEnt.ent.transform.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_BACKWARD ) != 0 )
                        Force += player.LinkedEnt.ent.transform.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_LEFT ) != 0 )
                        Force += player.LinkedEnt.ent.transform.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_RIGHT ) != 0 )
                        Force += player.LinkedEnt.ent.transform.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    Force.y = 0;
                    CamPhys.NetForce += Force;

                    if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 && Collision )
                    {
                        CamPhys.Velocity.y = 5.0f;
                    }
                }
                Renderer.RenderLoop( window, shader, player.LinkedEnt.ent, player.Perspective, EntList, EntList.Length );
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
        public delegate void WindowHandle( IntPtr window, int width, int height );
        public static void WindowMove( IntPtr window, int width, int height )
        {
            Util.MakePerspective( fov, (float) width / height, 0.01f, 1000.0f, out Matrix persp );
            player.Perspective = persp;
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
