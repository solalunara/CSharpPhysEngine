using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using Physics;
using RenderInterface;

namespace PhysEngine
{
    class Program
    {
        static RuntimeMechanics rtMech = RuntimeMechanics.NONE;
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
            try
            {
                if ( args.Length == 1 )
                {
                    if ( args[ 0 ] == "-2d" )
                        Run2D();
                    else if ( args[ 0 ] == "-3d" )
                        Run3D();
                    else
                        throw new NotImplementedException( "only 2d and 3d supported" );
                }
                else
                    Run3D();
            }
            finally
            {
                Renderer.Terminate();
            }
        }

        public static void Run3D()
        {
            string DirName = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            Renderer.Init( out IntPtr window );
            Shader shader = new( DirName + "/Shaders/VertexShader.vert", DirName + "/Shaders/FragmentShader.frag" );
            Shader GUI = new( DirName + "/Shaders/GUIVert.vert", DirName + "/Shaders/GUIFrag.frag" );
            shader.SetAmbientLight( 0.0f );

            Light[] testlights = 
            { 
                new Light( new Vector( 0, -5,-5 ), new Vector( 0.7f, 0.7f, 1 ), 20 ),
                new Light( new Vector( 0, -5, 5 ), new Vector( 0.7f, 0.7f, 1 ), 20 )
            };
            shader.SetLights( testlights );

            Renderer.GetWindowSize( window, out int width, out int height );
            Matrix persp = Matrix.Perspective( fov, (float) width / height, nearclip, farclip );

            float[] CrosshairVerts =
            {
                -.05f, -.05f, 0.0f,     0.0f, 0.0f,
                0.05f, -.05f, 0.0f,     1.0f, 0.0f,
                0.05f, 0.05f, 0.0f,     1.0f, 1.0f,
                -.05f, 0.05f, 0.0f,     0.0f, 1.0f
            };
            int[] CrosshairInds = { 0, 1, 3, 1, 2, 3 };
            FaceMesh CrosshairMesh = new( CrosshairVerts, CrosshairInds, new Texture( DirName + "/Textures/Crosshair.png" ), new Vector( 0, 0, 0 ) );


            World world = new( PhysicsEnvironment.Default_Gravity, 0.02f );

            Texture[] dirt = { new Texture( DirName + "/Textures/dirt.png" ) };
            Texture[] grass = { new Texture( DirName + "/Textures/grass.png" ) };
            player = new Player3D( persp, PhysObj.Default_Coeffs, Player3D.PLAYER_MASS, Player3D.PLAYER_ROTI );
            //player = new Player2D();
            world.Add
            (
                new PhysObj( new BoxEnt( new Vector( -1, -1, -7 ), new Vector( 1, 0, -5 ), grass ), PhysObj.Default_Coeffs, 25, 20, new() ),
                player.Body
            );
            world.Add
            (
                //dirt floors
                new BoxEnt( new Vector( -10, -11, -10 ), new Vector( 0, -10, 10 ), dirt ),
                new BoxEnt( new Vector( 0, -12, -10 ), new Vector( 20, -11, 10 ), dirt ),

                //grass walls
                new BoxEnt( new Vector( -10, -10, -12 ), new Vector( 0, 5, -10 ), grass ),
                new BoxEnt( new Vector( -10, -10, 10 ), new Vector( 0, 5, 12 ), grass ),
                new BoxEnt( new Vector( 0, -11, -12 ), new Vector( 20, 5, -10 ), grass ),
                new BoxEnt( new Vector( 0, -11, 10 ), new Vector( 20, 5, 12 ), grass ),
                new BoxEnt( new Vector( -12, -10, -10 ), new Vector( -10, 5, 10 ), grass ),
                new BoxEnt( new Vector( 10, -10, -10 ), new Vector( 12, 5, 10 ), grass ),
                new BoxEnt( new Vector( 20, -11, -10 ), new Vector( 22, 5, 10 ), grass ),

                //dirt roof
                new BoxEnt( new Vector( -10, 5, -10 ), new Vector( 20, 6, 10 ), dirt )
            );

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( window, Marshal.GetFunctionPointerForDelegate( inptptr ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( window, Marshal.GetFunctionPointerForDelegate( wndptr ) );
            MouseHandle msptr = MouseClick;
            Renderer.SetMouseButtonCallback( window, Marshal.GetFunctionPointerForDelegate( msptr ) );

            float lasttime = Renderer.GetTime();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                float time = Renderer.GetTime();
                float frametime = time - lasttime;
                lasttime = time;
                if ( frametime > 1.0f )
                    frametime = 0; //most likely debugging

                world.Simulator.SetPause( rtMech.HasFlag( RuntimeMechanics.PAUSED ) );

                if ( !rtMech.HasFlag( RuntimeMechanics.PAUSED ) )
                {

                    IPhysHandle CamPhys = player.Body;

                    Mouse.HideMouse( window );
                    const float LookSpeed = 10.0f;
                    Point2 MousePos = Mouse.GetMouseOffset( window );
                    player.Body.LinkedEnt.SetAbsRot( Matrix.RotMatrix( frametime * LookSpeed * -MousePos.x, new Vector( 0, 1, 0 ) ) * player.Body.LinkedEnt.GetAbsRot() );

                    Matrix PrevHead = player.camera.GetLocalRot();
                    player.camera.SetLocalRot( Matrix.RotMatrix( frametime * LookSpeed * -MousePos.y, new Vector( 1, 0, 0 ) ) * player.camera.GetLocalRot() );
                    if ( Vector.Dot( new Vector( 0, 0, -1 ), player.camera.GetLocalRot().GetForward() ) < 0 ) //looking >90 degrees
                    {
                        player.camera.SetLocalRot( PrevHead );
                    }

                    Mouse.MoveMouseToCenter( window );

                    CamPhys.TestCollision( world, out bool Collision, out bool TopCollision );

                    float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                    Vector Force = new();
                    if ( ( MoveTracker & (uint) Move.MOVE_FORWARD ) != 0 )
                        Force += player.Body.LinkedEnt.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_BACKWARD ) != 0 )
                        Force += player.Body.LinkedEnt.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_LEFT ) != 0 )
                        Force += player.Body.LinkedEnt.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & (uint) Move.MOVE_RIGHT ) != 0 )
                        Force += player.Body.LinkedEnt.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    //Force.y = 0;
                    CamPhys.AddForce( Force, 0 );

                    if ( ( MoveTracker & (uint) Move.MOVE_JUMP ) != 0 && TopCollision )
                    {
                        Vector vel = CamPhys.Velocity;
                        vel.y = 5.0f;
                        CamPhys.Velocity = vel;
                    }
                    if ( CamPhys.Velocity.Length() > Max_Player_Speed && Collision )
                    {
                        CamPhys.Velocity = CamPhys.Velocity.Normalized() * Max_Player_Speed;
                    }

                    if ( rtMech.HasFlag( RuntimeMechanics.SAVE ) )
                    {
                        rtMech &= ~RuntimeMechanics.SAVE;
                        //world.ToFile( DirName + "/Worlds/world1.worldmap" );
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.LOAD ) )
                    {
                        rtMech &= ~RuntimeMechanics.LOAD;
                        //world.Close();
                        //world = World.FromFile( DirName + "/Worlds/world1.worldmap" );
                        //Renderer.GetWindowSize( window, out width, out height );
                        //Matrix.GLMPerspective( fov, (float) width / height, nearclip, farclip, out persp );
                        //world.player.Perspective = persp;
                    }

                    if ( rtMech.HasFlag( RuntimeMechanics.FIREC ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREC;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt );
                        if ( hit.bHit )
                        {
                            IEntHandle HitEnt = hit.HitEnt;
                            HitEnt.LocalTransform.Scale *= new Vector( 1.1f, 1.1f, 1.1f );
                        }
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREV ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREV;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt );
                        if ( hit.bHit )
                        {
                            IEntHandle HitEnt = hit.HitEnt;
                            HitEnt.LocalTransform.Scale *= new Vector( 0.9f, 0.9f, 0.9f );
                        }
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREX ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREX;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                        Vector ptCenter = new();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = player.camera.GetAbsOrigin() + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        world.Add( new BoxEnt( mins, maxs, dirt ) );
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREZ ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREZ;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                        Vector ptCenter = new();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = player.camera.GetAbsOrigin() + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        world.Add( new BoxEnt( mins, maxs, grass ) );
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREF ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREF;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                        if ( hit.bHit )
                            world.WorldEnts.Remove( hit.HitEnt );
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIRER ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIRER;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                        if ( hit.bHit || world.GetEntPhysics( hit.HitEnt ) == null )
                            world.Add( new PhysObj( hit.HitEnt, PhysObj.Default_Coeffs, 25, 1, new() ) );
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREE ) && player.GetType() == typeof( Player3D ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREE;
                        Player3D player3 = (Player3D) player;
                        if ( player3.HeldEnt != null )
                        {
                            player3.HeldEnt.Parent = null;
                            player3.HeldEnt = null;
                        }
                        else
                        {
                            Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                            Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                            RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                            if ( hit.bHit )
                            {
                                PhysObj HitPhys = (PhysObj) world.GetEntPhysics( hit.HitEnt );
                                if ( HitPhys != null )
                                    HitPhys.Velocity = new Vector();
                                hit.HitEnt.Parent = player.camera;
                                player3.HeldEnt = hit.HitEnt;
                            }
                        }
                    }
                    if ( rtMech.HasFlag( RuntimeMechanics.FIREQ ) )
                    {
                        rtMech &= ~RuntimeMechanics.FIREQ;
                        Vector TransformedForward = (Vector) ( player.camera.GetAbsRot() * new Vector( 0, 0, -30 ) );
                        Vector EntPt = player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = world.TraceRay( player.camera.GetAbsOrigin(), EntPt, player.Body.LinkedEnt);
                        if ( hit.bHit )
                        {
                            PhysObj HitPhys = (PhysObj) world.GetEntPhysics( hit.HitEnt );
                            if ( HitPhys != null )
                            {
                                if ( HitPhys.AngularMomentum.y > 0 )
                                    HitPhys.Torque -= new Vector( 0, 10, 10 );
                                else
                                    HitPhys.Torque += new Vector( 0, 10, 10 );
                            }
                        }
                    }
                }
                else
                    Mouse.ShowMouse( window );


                TimeSpan TimeDiff = DateTime.Now - world.Environment.LastSimTime;

                Renderer.StartFrame( window );
                Renderer.SetCameraValues( shader, player.camera.Perspective, -player.camera.CalcEntMatrix() );
                foreach ( BaseEntity b in world.WorldEnts )
                {
                    Renderer.SetRenderValues( shader, b.CalcEntMatrix() );
                    foreach ( FaceMesh m in b.Meshes )
                    {
                        if ( !m.texture.Initialized )
                            continue; //nothing to render

                        m.Render( shader );
                    }
                }
                CrosshairMesh.Render( GUI );
                Renderer.EndFrame( window );
            }
            //world.ToFile( DirName + "/Worlds/world1.worldmap" );
        }

        public static void Run2D()
        {
            string DirName = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            Renderer.Init( out IntPtr window );
            Shader shader = new( DirName + "/Shaders/VertexShader.vert", DirName + "/Shaders/FragmentShader.frag" );
            World world = new( PhysicsEnvironment.Default_Gravity, 0.02f );
            Texture[] dirt = { new( DirName + "/Textures/dirt.png" ) };
            Texture button = new( DirName + "/Textures/button.png" );
            world.Add
            (
                new Button( new( -1, -1 ), new( 1, 1 ), button, () => Console.WriteLine( "clicked" ) )
            );
            world.Add
            (
                new PhysObj( new Dim2Box( new( -1, 3 ), new( 1, 5 ), dirt[ 0 ] ), PhysObj.Default_Coeffs, 5, 5, new() )
            );
            shader.SetAmbientLight( 1.0f );

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( window, Marshal.GetFunctionPointerForDelegate( inptptr = Input ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( window, Marshal.GetFunctionPointerForDelegate( wndptr ) );
            MouseHandle msptr = MouseClick;
            Renderer.SetMouseButtonCallback( window, Marshal.GetFunctionPointerForDelegate( msptr ) );

            player = new Player2D();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                if ( rtMech.HasFlag( RuntimeMechanics.FIRELEFT ) )
                {
                    rtMech &= ~RuntimeMechanics.FIRELEFT;
                    Point2 ms = Mouse.GetMouseNormalizedPos( window );
                    Vector MousePos = player.camera.TransformPoint( new( ms.x * 10, ms.y * 10, 0 ) );
                    MousePos.z = 0;
                    for ( int i = 0; i < world.WorldEnts.Count; ++i )
                    {
                        if ( world.WorldEnts[ i ].GetType() == typeof( Button ) )
                        {
                            Button b = (Button) world.WorldEnts[ i ];
                            if ( b.TestCollision( MousePos ) )
                                b.ClickCallback();
                        }
                    }
                }

                Renderer.StartFrame( window );
                Matrix Cam = -player.camera.CalcEntMatrix();
                Renderer.SetCameraValues( shader, player.camera.Perspective, -player.camera.CalcEntMatrix() );
                foreach ( BaseEntity b in world.WorldEnts )
                {
                    Renderer.SetRenderValues( shader, b.CalcEntMatrix() );
                    foreach ( FaceMesh m in b.Meshes )
                    {
                        if ( !m.texture.Initialized )
                            continue; //nothing to render

                        m.Render( shader );
                    }
                };
                Renderer.EndFrame( window );
            }

            dirt[ 0 ].Close();
            button.Close();
            world.Close();
        }

        public delegate void InputHandle( IntPtr window, Keys key, int scancode, Actions act, int mods );
        public static void Input( IntPtr window, Keys key, int scancode, Actions act, int mods )
        {
            if ( act == Actions.PRESSED && key == Keys.ESCAPE ) //pressed
                rtMech ^= RuntimeMechanics.PAUSED;

            if ( act == Actions.PRESSED )
            {
                switch ( key )
                {
                    case Keys.F6:
                        rtMech |= RuntimeMechanics.SAVE;
                        break;
                    case Keys.F7:
                        rtMech |= RuntimeMechanics.LOAD;
                        break;
                    case Keys.Q:
                        rtMech |= RuntimeMechanics.FIREQ;
                        break;
                    case Keys.E:
                        rtMech |= RuntimeMechanics.FIREE;
                        break;
                    case Keys.Z:
                        rtMech |= RuntimeMechanics.FIREZ;
                        break;
                    case Keys.X:
                        rtMech |= RuntimeMechanics.FIREX;
                        break;
                    case Keys.F:
                        rtMech |= RuntimeMechanics.FIREF;
                        break;
                    case Keys.R:
                        rtMech |= RuntimeMechanics.FIRER;
                        break;
                    case Keys.C:
                        rtMech |= RuntimeMechanics.FIREC;
                        break;
                    case Keys.V:
                        rtMech |= RuntimeMechanics.FIREV;
                        break;

                    default:
                        break;
                }
            }

            if ( player.GetType() == typeof( Player3D ) )
            {
                Player3D player3 = (Player3D) player;
                if ( ( act == Actions.PRESSED || act == Actions.HELD ) && key == Keys.LCONTROL ) //holding control
                    player3.Crouch();
                else if ( key == Keys.LCONTROL ) //not holding control
                    player3.UnCrouch();
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
            Renderer.WindowSizeChanged( width, height );
            player.camera.Perspective = Matrix.Perspective( fov, (float) width / height, 0.01f, 1000.0f );
        }
        public delegate void MouseHandle( IntPtr window, MouseButton mouse, Actions act, int mods );
        public static void MouseClick( IntPtr window, MouseButton mouse, Actions act, int mods )
        {
            if ( act == Actions.PRESSED )
            {
                switch ( mouse )
                {
                    case MouseButton.LEFT:
                        rtMech |= RuntimeMechanics.FIRELEFT;
                        break;
                    case MouseButton.RIGHT:
                        rtMech |= RuntimeMechanics.FIRERIGHT;
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
}
