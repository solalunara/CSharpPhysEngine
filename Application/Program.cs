using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace PhysEngine
{
    class Program
    {
        static bool Paused = false;
        static bool Save = false;
        static bool Load = false;
        static bool FireQ = false;
        static bool FireE = false;
        static bool FireZ = false;
        static bool FireX = false;
        static bool FireF = false;
        static bool FireR = false;
        static uint MoveTracker = (uint) Move.MOVE_NONE;
        static Player player;


        const float fov = 75.0f;
        const float nearclip = 0.01f;
        const float farclip = 1000.0f;
        const float Movespeed_Air = 5.0f;
        const float Movespeed_Gnd = 20.0f;

        const float Max_Player_Speed = 5.0f;


        static void Main( string[] args )
        {
            string DirName = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            bool bMakeNewMap = args.Length != 1;

            Renderer.Init( out IntPtr window );
            Shader shader = new Shader( DirName + "/Shaders/VertexShader.vert", DirName + "/Shaders/FragmentShader.frag" );
            Shader GUI = new Shader( DirName + "/Shaders/GUIVert.vert", DirName + "/Shaders/GUIFrag.frag" );
            shader.SetAmbientLight( 0.0f );

            Light[] testlights = 
            { 
                new Light( new Vector( 0, -5,-5 ), new Vector( 0.7f, 0.7f, 1 ), 20 ),
                new Light( new Vector( 0, -5, 5 ), new Vector( 0.7f, 0.7f, 1 ), 20 )
            };
            shader.SetLights( testlights );

            Renderer.GetWindowSize( window, out int width, out int height );
            Util.MakePerspective( fov, (float) width / height, nearclip, farclip, out Matrix persp );

            
            World world = new World();
            if ( bMakeNewMap )
            {
                //world needs a list of textures in it for saverestore
                world.Add
                (
                    new TextureHandle( DirName + "/Textures/dirt.png" ),
                    new TextureHandle( DirName + "/Textures/grass.png" )
                );
                Texture[] dirt = { world.Textures[ 0 ].texture };
                Texture[] grass = { world.Textures[ 1 ].texture };
                world.Add
                (
                    //player
                    player = new Player( new THandle( new Vector(), new Vector( 1, 1, 1 ), Matrix.IdentityMatrix() ), persp )
                );
                world.Add
                (
                    true, new PhysicsObject( new EHandle( new Vector( -1, -5, -1 ), new Vector( 1, -4, 1 ), grass ), PhysicsObject.Default_Gravity, PhysicsObject.Default_Coeffs, 5.0f )
                );
                world.Add
                (
                    false, new PhysicsObject( player, PhysicsObject.Default_Gravity, PhysicsObject.Default_Coeffs, 50.0f )
                );
                world.Add
                (
                    //dirt floors
                    new EHandle( new Vector( -10, -11, -10 ), new Vector( 0, -10, 10 ), dirt ),
                    new EHandle( new Vector( 0, -12, -10 ), new Vector( 20, -11, 10 ), dirt ),

                    //grass walls
                    new EHandle( new Vector( -10, -10, -12 ), new Vector( 0, 0, -10 ), grass ),
                    new EHandle( new Vector( -10, -10, 10 ), new Vector( 0, 0, 12 ), grass ),
                    new EHandle( new Vector( 0, -11, -12 ), new Vector( 20, 0, -10 ), grass ),
                    new EHandle( new Vector( 0, -11, 10 ), new Vector( 20, 0, 12 ), grass ),
                    new EHandle( new Vector( -12, -10, -10 ), new Vector( -10, 0, 10 ), grass ),
                    new EHandle( new Vector( 10, -10, -10 ), new Vector( 12, 0, 10 ), grass ),
                    new EHandle( new Vector( 20, -11, -10 ), new Vector( 22, 0, 10 ), grass )
                );
            }
            else //load map from arg
            {
                Console.WriteLine( "attempting to create from file..." );
                world = World.FromFile( args[ 0 ] );
                world.player.Perspective = persp;
            }


            player = world.player;

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( Marshal.GetFunctionPointerForDelegate( wndptr ) );

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
                    PhysicsObject CamPhys = world.GetPlayerPhysics();

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
                    //Force.y = 0;
                    CamPhys.NetForce += Force;

                    if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 && TopCollision )
                    {
                        CamPhys.Velocity.y = 5.0f;
                    }
                    if ( CamPhys.Velocity.Length() > Max_Player_Speed && Collision )
                    {
                        CamPhys.Velocity = CamPhys.Velocity.Normalized() * Max_Player_Speed;
                    }

                    PhysicsObject.SimulateWorld( frametime, world );

                    if ( Save )
                    {
                        Save = false;
                        world.ToFile( DirName + "/Worlds/world1.worldmap" );
                    }
                    if ( Load )
                    {
                        Load = false;
                        world.Close();
                        world = World.FromFile( DirName + "/Worlds/world1.worldmap" );
                        Renderer.GetWindowSize( window, out width, out height );
                        Util.MakePerspective( fov, (float) width / height, nearclip, farclip, out persp );
                        world.player.Perspective = persp;
                        player = world.player;
                    }

                    if ( FireQ )
                    {
                        FireQ = false;
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        if ( hit.bHit )
                        {
                            EHandle HitEnt = hit.HitEnt;
                            HitEnt.Transform.Scale *= new Vector( 1.1f, 1.1f, 1.1f );
                            HitEnt.AABB.Mins *= 1.1f;
                            HitEnt.AABB.Maxs *= 1.1f;
                        }
                    }
                    if ( FireE )
                    {
                        FireE = false;
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        if ( hit.bHit )
                        {
                            EHandle HitEnt = hit.HitEnt;
                            HitEnt.Transform.Scale *= new Vector( 0.9f, 0.9f, 0.9f );
                            HitEnt.AABB.Mins *= 0.9f;
                            HitEnt.AABB.Maxs *= 0.9f;
                        }
                    }
                    if ( FireX )
                    {
                        FireX = false;
                        Texture[] dirt = { world.Textures[ 0 ].texture };
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        Vector ptCenter = new Vector();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = player.Transform.Position + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        world.Add( new EHandle( mins, maxs, dirt ) );
                    }
                    if ( FireZ )
                    {
                        FireZ = false;
                        Texture[] grass = { world.Textures[ 1 ].texture };
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        Vector ptCenter = new Vector();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = player.Transform.Position + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        world.Add( new EHandle( mins, maxs, grass ) );
                    }
                    if ( FireF )
                    {
                        FireF = false;
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        if ( hit.bHit )
                            world.WorldEnts.Remove( hit.HitEnt );
                    }
                    if ( FireR )
                    {
                        FireR = false;
                        Vector TransformedForward = player.Transform.TransformDirection( new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.Transform.Position + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.Transform.Position, EntPt );
                        if ( hit.bHit )
                            world.Add( false, new PhysicsObject( hit.HitEnt, PhysicsObject.Default_Gravity, PhysicsObject.Default_Coeffs, 5 ) );
                    }
                }
                else
                    Mouse.ShowMouse( window );

                Renderer.RenderLoop( window, shader, player.ent, player.Perspective, world.GetEntList(), world.WorldEnts.Count );
            }
            world.ToFile( DirName + "/Worlds/world1.worldmap" );
            Renderer.Terminate();
        }

        public delegate void InputHandle( IntPtr window, Keys key, int scancode, Actions act, int mods );
        public static void Input( IntPtr window, Keys key, int scancode, Actions act, int mods )
        {
            if ( act == Actions.PRESSED && key == Keys.ESCAPE ) //pressed
                Paused = !Paused;

            if ( act == Actions.PRESSED )
            {
                if ( key == Keys.F6 )
                    Save = true;
                if ( key == Keys.F7 )
                    Load = true;
                if ( key == Keys.Q )
                    FireQ = true;
                if ( key == Keys.E )
                    FireE = true;
                if ( key == Keys.Z )
                    FireZ = true;
                if ( key == Keys.X )
                    FireX = true;
                if ( key == Keys.F )
                    FireF = true;
                if ( key == Keys.R )
                    FireR = true;
            }


            if ( ( act == Actions.PRESSED || act == Actions.HELD ) && key == Keys.LCONTROL ) //holding control
                player.Crouch();
            else if ( key == Keys.LCONTROL ) //not holding control
                player.UnCrouch();



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
        Q = 81,
        E = 69,
        Z = 90,
        X = 88,
        C = 67,
        V = 86,
        F = 70,
        R = 82,
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
