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
        static bool Paused = false;
        static bool Save = false;
        static bool Load = false;
        static uint MoveTracker = (uint) Move.MOVE_NONE;
        static Player player;


        const float fov = 75.0f;
        const float Movespeed_Air = 5.0f;
        const float Movespeed_Gnd = 20.0f;

        const float Max_Player_Speed = 5.0f;

        public static readonly BBox PLAYER_NORMAL_BBOX = new BBox( new Vector( -.5f, -1.5f, -.5f ), new Vector( .5f, .5f, .5f ) );
        public static readonly BBox PLAYER_CROUCH_BBOX = new BBox( new Vector( -.5f, -0.5f, -.5f ), new Vector( .5f, .5f, .5f ) );


        static void Main( string[] args )
        {
            Renderer.Init( out IntPtr window );
            Shader shader = new Shader( "Shaders/VertexShader.vert", "Shaders/FragmentShader.frag" );

            Renderer.GetWindowSize( window, out int width, out int height );
            Util.MakePerspective( fov, (float) width / height, 0.01f, 1000.0f, out Matrix persp );

            /*
            World world = new World();
            //world needs a list of textures in it for saverestore
            world.Add
            (
                new TextureHandle( "Textures/dirt.png" ),
                new TextureHandle( "Textures/grass.png" )
            );
            Texture[] dirt = { world.Textures[ 0 ].texture };
            Texture[] grass = { world.Textures[ 1 ].texture };
            world.Add
            (
                //player
                new Player( new THandle( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ), persp )
            );
            world.Add
            (
                //dirt floors
                new EHandle(    new Vector( -10, -11, -10 ),   new Vector( 0, -10, 10 ),    dirt ),
                new EHandle(    new Vector( 0, -12, -10 ),     new Vector( 20, -11, 10 ),   dirt ),

                //grass walls
                new EHandle(    new Vector( -10, -10, -12 ),    new Vector( 0, 0, -10 ),    grass ),
                new EHandle(    new Vector( -10, -10, 10 ),     new Vector( 0, 0, 12 ),     grass ),
                new EHandle(    new Vector( 0, -11, -12 ),      new Vector( 20, 0, -10 ),   grass ),
                new EHandle(    new Vector( 0, -11, 10 ),       new Vector( 20, 0, 12 ),    grass ),
                new EHandle(    new Vector( -12, -10, -10 ),    new Vector( -10, 0, 10 ),   grass ),
                new EHandle(    new Vector( 10, -10, -10 ),     new Vector( 12, 0, 10 ),    grass ),
                new EHandle(    new Vector( 20, -11, -10 ),     new Vector( 22, 0, 10 ),    grass )
            );
            */
            
            //World world = World.FromFile( "Worlds/world1.map" );



            //map is correctly loading for c# but not passing through to c++
            //most likely an issue with needing to reinitialize gpu memory pointers
            World world = World.FromFile( "Worlds/world1.map" );

            player = world.Players[ 0 ];

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( Marshal.GetFunctionPointerForDelegate( wndptr ) );

            PhysicsObject CamPhys = new PhysicsObject( player, PhysicsObject.Default_Gravity, PhysicsObject.Default_Coeffs, 50.0f );

            float lasttime = Renderer.GetTime();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                float time = Renderer.GetTime();
                float frametime = time - lasttime;
                lasttime = time;
                if ( frametime > 1.0f )
                    frametime = 0; //most likely debugging

                bool Collision = false;
                if ( !Paused )
                {
                    Mouse.HideMouse( window );
                    const float LookSpeed = 10.0f;
                    Mouse.GetMouseOffset( window, out double x, out double y );
                    Vector Up = player.Transform.InverseTransformDirection( new Vector( 0, 1, 0 ) );
                    Vector Rt = new Vector( 1, 0, 0 );
                    Util.MakeRotMatrix( (float) ( frametime * LookSpeed * -x ), new Vector( 0, -1, 0 ), out Matrix XRotGlobal );
                    Util.MakeRotMatrix( (float) ( frametime * LookSpeed * -x ), Up, out Matrix XRot );
                    Util.MakeRotMatrix( (float) ( frametime * LookSpeed * -y ), Rt, out Matrix YRot );
                    player.Transform.Rotation = Util.MultiplyMatrix( player.Transform.Rotation, XRot );
                    player.Transform.Rotation = Util.MultiplyMatrix( player.Transform.Rotation, YRot );

                    Mouse.MoveMouseToCenter( window );

                    Collision = CamPhys.TestCollision( world, out bool TopCollision );

                    float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                    Vector Force = new Vector();
                    if ( ( MoveTracker & (uint) Move.MOVE_FORWARD ) != 0 )
                        Force += player.Transform.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_BACKWARD ) != 0 )
                        Force += player.Transform.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_LEFT ) != 0 )
                        Force += player.Transform.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_RIGHT ) != 0 )
                        Force += player.Transform.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    Force.y = 0;
                    CamPhys.NetForce += Force;

                    if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 && TopCollision )
                    {
                        CamPhys.Velocity.y = 5.0f;
                    }

                    if ( CamPhys.Velocity.Length() > Max_Player_Speed && Collision )
                    {
                        CamPhys.Velocity = CamPhys.Velocity.Normalized() * Max_Player_Speed;
                    }

                    CamPhys.Simulate( frametime, world );

                    if ( Save )
                    {
                        Save = false;
                        world.ToFile( "Worlds/world1.map" );
                    }
                    if ( Load )
                    {
                        Load = false;
                        world.Close();
                        world = World.FromFile( "Worlds/world1.map" );
                        player = world.Players[ 0 ];
                        CamPhys.LinkedEnt = player;
                    }
                }
                else
                    Mouse.ShowMouse( window );

                Renderer.RenderLoop( window, shader, player.ent, player.Perspective, world.GetEntList(), world.WorldEnts.Count );
            }
            world.ToFile( "Worlds/world1.map" );
            Renderer.Terminate();
        }

        public delegate void InputHandle( IntPtr window, Keys key, int scancode, Actions act, int mods );
        public static void Input( IntPtr window, Keys key, int scancode, Actions act, int mods )
        {
            if ( act == Actions.PRESSED && key == Keys.ESCAPE ) //pressed
            {
                Paused = !Paused;
            }

            if ( act == Actions.PRESSED && key == Keys.F6 )
            {
                Save = true;
            }
            if ( act == Actions.PRESSED && key == Keys.F7 )
            {
                Load = true;
            }

            if ( ( act == Actions.PRESSED || act == Actions.HELD ) && key == Keys.LCONTROL ) //holding control
            {
                player.AABB = new BHandle( PLAYER_CROUCH_BBOX );
            }
            else if ( key == Keys.LCONTROL ) //not holding control
            {
                player.AABB = new BHandle( PLAYER_NORMAL_BBOX );
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
        LCONTROL = 341,
        F6 = 295,
        F7 = 296,
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
