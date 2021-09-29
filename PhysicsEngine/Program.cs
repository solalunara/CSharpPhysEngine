#define ALWAYS_USE_EDITOR

global using Physics;
global using RenderInterface;
global using static System.Diagnostics.Debug;
global using static RenderInterface.Renderer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Threading;
using Editor;

namespace PhysEngine
{
    internal static class Program
    {
        public static readonly string DirName = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
        private static Move MoveTracker = Move.MOVE_NONE;
        private static World MainWorld;

        public const float fov = 75.0f;
        public const float nearclip = 0.01f;
        public const float farclip = 1000.0f;
        public const float Movespeed_Air = 5.0f;
        public const float Movespeed_Gnd = 20.0f;
        public const float Max_Player_Speed = 5.0f;
        
        public static void Main( string[] args )
        {
            try
            {
                Thread t = new( () => App.Main() );
                t.SetApartmentState( ApartmentState.STA );

                Init( out IntPtr window );
                string[] TextureNames = Directory.EnumerateFiles( DirName + "\\Textures" ).ToArray();
                for ( int i = 0; i < TextureNames.Length; ++i )
                {
                    AddTexture( TextureNames[ i ] );
                }
                if ( args.Length > 0 )
                {
                    bool EditorStarted = false;
                    for ( int i = 0; i < args.Length; ++i )
                    {
#if !ALWAYS_USE_EDITOR
                        if ( args[ i ].ToLower() == "-editor" )
#endif
                        if ( !EditorStarted )
                        {
                            t.Start();
                            EditorStarted = true;
                        }

                        switch ( args[ i ].ToLower() )
                        {
                            case "-3d":
                            {
                                Run3D( window );
                                break;
                            }
                        }
                        if ( args[ i ].ToLower().Contains( ".worldmap" ) )
                        {
                            MainWorld = World.FromFile( args[ i ] );
                        }

                    }
                }
                else
                {
#if ALWAYS_USE_EDITOR
                    t.Start();
#endif
                    Run3D( window );
                }
            }
            finally
            {
                Terminate();
            }
        }

        public static void Run3D( IntPtr window )
        {
            Shader shader = new( DirName + "\\Shaders\\VertexShader.vert", DirName + "\\Shaders\\FragmentShader.frag" );
            Shader GUI = new( DirName + "\\Shaders\\GUIVert.vert", DirName + "\\Shaders\\GUIFrag.frag" );
            shader.SetAmbientLight( 0.0f );

            Light[] testlights = 
            { 
                new Light( new Vector4( 0, -5,-5, 1 ), new Vector4( 0.7f, 0.7f, 1, 1 ), 20 ),
                new Light( new Vector4( 0, -5, 5, 1 ), new Vector4( 0.7f, 0.7f, 1, 1 ), 20 )
            };
            shader.SetLights( testlights );

            GetWindowSize( window, out int width, out int height );
            Matrix persp = Matrix.Perspective( fov, (float) width / height, nearclip, farclip );

            float[] CrosshairVerts =
            {
                -.05f, -.05f, 0.0f,     0.0f, 0.0f,
                0.05f, -.05f, 0.0f,     1.0f, 0.0f,
                0.05f, 0.05f, 0.0f,     1.0f, 1.0f,
                -.05f, 0.05f, 0.0f,     0.0f, 1.0f
            };
            int[] CrosshairInds = { 0, 1, 3, 1, 2, 3 };
            FaceMesh CrosshairMesh = new( CrosshairVerts, CrosshairInds, FindTexture( DirName + "\\Textures\\Crosshair.png" ), new Vector( 0, 0, 0 ) );


            MainWorld = new( PhysicsEnvironment.Default_Gravity, 0.02f );
            if ( MainWindow.Started ) //editor open
                MainWindow.Instance.World = MainWorld;

            (Texture, string)[] dirt = { (FindTexture( DirName + "\\Textures\\dirt.png" ), DirName + "\\Textures\\dirt.png") };
            (Texture, string)[] grass = { (FindTexture( DirName + "\\Textures\\grass.png" ), DirName + "\\Textures\\grass.png") };
            MainWorld.player = new Player3D( persp, PhysObj.Default_Coeffs, Player3D.PLAYER_MASS, Player3D.PLAYER_ROTI );
            //player = new Player2D();
            MainWorld.Add
            (
                new PhysObj( new BoxEnt( new Vector( -1, -1, -7 ), new Vector( 1, 0, -5 ), grass ), PhysObj.Default_Coeffs, 25, 5, new() )
            );
            MainWorld.Add
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

            SetInputCallback( window, Marshal.GetFunctionPointerForDelegate<InputHandle>( Input ) );
            SetWindowMoveCallback( window, Marshal.GetFunctionPointerForDelegate<WindowHandle>( WindowMove ) );
            SetMouseButtonCallback( window, Marshal.GetFunctionPointerForDelegate<MouseHandle>( MouseClick ) );

            float lasttime = GetTime();

            while ( !ShouldTerminate( window ) )
            {
                float time = GetTime();
                float frametime = time - lasttime;
                lasttime = time;
                if ( frametime > 1.0f )
                    frametime = 0; //most likely debugging

                if ( !MainWorld.Simulator.Paused() )
                {
                    BasePhysics CamPhys = MainWorld.player.Body;

                    Mouse.HideMouse( window );
                    const float LookSpeed = 10.0f;
                    Point2<double> MousePos = Mouse.GetMouseOffset( window );
                    MainWorld.player.Body.LinkedEnt.SetAbsRot( Matrix.RotMatrix( frametime * LookSpeed * -(float)MousePos.x, new Vector( 0, 1, 0 ) ) * MainWorld.player.Body.LinkedEnt.GetAbsRot() );

                    Matrix PrevHead = MainWorld.player.camera.GetLocalRot();
                    MainWorld.player.camera.SetLocalRot( Matrix.RotMatrix( frametime * LookSpeed * -(float)MousePos.y, new Vector( 1, 0, 0 ) ) * MainWorld.player.camera.GetLocalRot() );
                    if ( Vector.Dot( new Vector( 0, 0, -1 ), MainWorld.player.camera.GetLocalRot().GetForward() ) < 0 ) //looking >90 degrees
                    {
                        MainWorld.player.camera.SetLocalRot( PrevHead );
                    }

                    Mouse.MoveMouseToCenter( window );

                    CamPhys.TestCollision( MainWorld, out bool Collision, out bool TopCollision );

                    float Movespeed = Collision ? Movespeed_Gnd : Movespeed_Air;
                    Vector Force = new();
                    if ( ( MoveTracker & Move.MOVE_FORWARD ) != 0 )
                        Force += MainWorld.player.Body.LinkedEnt.TransformDirection( new Vector( 0, 0, -1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & Move.MOVE_BACKWARD ) != 0 )
                        Force += MainWorld.player.Body.LinkedEnt.TransformDirection( new Vector( 0, 0, 1 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & Move.MOVE_LEFT ) != 0 )
                        Force += MainWorld.player.Body.LinkedEnt.TransformDirection( new Vector( -1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    if ( ( MoveTracker & Move.MOVE_RIGHT ) != 0 )
                        Force += MainWorld.player.Body.LinkedEnt.TransformDirection( new Vector( 1, 0, 0 ) ) * Movespeed * CamPhys.Mass;
                    //Force.y = 0;
                    CamPhys.AddForce( Force, 0 );

                    if ( ( MoveTracker & Move.MOVE_JUMP ) != 0 && TopCollision )
                    {
                        Vector vel = CamPhys.Velocity;
                        vel.y = 5.0f;
                        CamPhys.Velocity = vel;
                    }
                    if ( CamPhys.Velocity.Length() > Max_Player_Speed && Collision )
                    {
                        CamPhys.Velocity = CamPhys.Velocity.Normalized() * Max_Player_Speed;
                    }
                }
                else
                    Mouse.ShowMouse( window );


                TimeSpan TimeDiff = DateTime.Now - MainWorld.Environment.LastSimTime;

                StartFrame( window );
                SetCameraValues( shader, MainWorld.player.camera.Perspective, -MainWorld.player.camera.CalcEntMatrix() );
                try
                {
                    foreach ( BaseEntity b in MainWorld.WorldEnts )
                    {
                        SetRenderValues( shader, b.CalcEntMatrix() );
                        foreach ( (FaceMesh, string) m in b.Meshes )
                        {
                            if ( !m.Item1.texture.Initialized )
                                continue; //nothing to render

                            m.Item1.Render( shader );
                        }
                    }
                    CrosshairMesh.Render( GUI );

                    if ( MainWindow.Started )
                    {
                        if ( MainWindow.ShouldCreateEnt )
                            MainWindow.ExternCreateEnt( MainWorld );
                    }
                }
                catch ( InvalidOperationException )
                {
                    Console.WriteLine( "Entity list updated during frame" );
                }
                EndFrame( window );
            }
            //world.ToFile( DirName + "/Worlds/world1.worldmap" );
        }


        public delegate void InputHandle( IntPtr window, Keys key, int scancode, Actions act, int mods );
        public static void Input( IntPtr window, Keys key, int scancode, Actions act, int mods )
        {
            if ( act == Actions.PRESSED && key == Keys.ESCAPE ) //pressed
                MainWorld.Simulator.SetPause( !MainWorld.Simulator.Paused() );

            if ( act == Actions.PRESSED )
            {
                switch ( key )
                {
                    case Keys.F6:
                    {
                        MainWorld.ToFile( DirName + "/Worlds/world1.worldmap" );
                        break;
                    }
                    case Keys.F7:
                    {
                        MainWorld.Close();
                        MainWorld = World.FromFile( DirName + "/Worlds/world1.worldmap" );
                        GetWindowSize( window, out int width, out int height );
                        MainWorld.player.camera.Perspective = Matrix.Perspective( fov, width / height, nearclip, farclip );
                        break;
                    }
                    case Keys.Q:
                    {
                        break;
                    }
                    case Keys.E:
                    {
                        Player3D player3 = (Player3D)MainWorld.player;
                        if ( player3.HeldEnt != null )
                        {
                            player3.HeldEnt.Parent = null;
                            player3.HeldEnt = null;
                        }
                        else
                        {
                            Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                            Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                            RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt, MainWorld.player.camera );
                            if ( hit.bHit )
                            {
                                PhysObj HitPhys = (PhysObj)MainWorld.GetEntPhysics( hit.HitEnt );
                                if ( HitPhys != null )
                                    HitPhys.Velocity = new Vector();
                                hit.HitEnt.Parent = MainWorld.player.camera;
                                player3.HeldEnt = hit.HitEnt;
                            }
                        }
                        break;
                    }
                    case Keys.Z:
                    {
                        break;
                    }
                    case Keys.X:
                    {
                        break;
                    }
                    case Keys.F:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt, MainWorld.player.camera );
                        if ( hit.bHit )
                            MainWorld.WorldEnts.Remove( hit.HitEnt );
                        break;
                    }
                    case Keys.R:
                    {
                        break;
                    }
                    case Keys.C:
                    {
                        break;
                    }
                    case Keys.V:
                    {
                        break;
                    }

                    default:
                        break;
                }
            }

            if ( MainWorld.player.GetType() == typeof( Player3D ) )
            {
                Player3D player3 = (Player3D) MainWorld.player;
                if ( ( act == Actions.PRESSED || act == Actions.HELD ) && key == Keys.LCONTROL ) //holding control
                    player3.Crouch();
                else if ( key == Keys.LCONTROL ) //not holding control
                    player3.UnCrouch();
            }
            
            if ( act == Actions.PRESSED )
            {
                switch ( key )
                {
                    case Keys.W:
                    MoveTracker |= Move.MOVE_FORWARD;
                    break;
                    case Keys.S:
                    MoveTracker |= Move.MOVE_BACKWARD;
                    break;
                    case Keys.A:
                    MoveTracker |= Move.MOVE_LEFT;
                    break;
                    case Keys.D:
                    MoveTracker |= Move.MOVE_RIGHT;
                    break;
                }
            }
            if ( act == Actions.RELEASED )
            {
                switch ( key )
                {
                    case Keys.W:
                    MoveTracker &= ~Move.MOVE_FORWARD;
                    break;
                    case Keys.S:
                    MoveTracker &= ~Move.MOVE_BACKWARD;
                    break;
                    case Keys.A:
                    MoveTracker &= ~Move.MOVE_LEFT;
                    break;
                    case Keys.D:
                    MoveTracker &= ~Move.MOVE_RIGHT;
                    break;
                }
            }
            if ( ( act == Actions.HELD || act == Actions.PRESSED ) && key == Keys.SPACE )
                MoveTracker |= Move.MOVE_JUMP;
            else if ( key == Keys.SPACE )
                MoveTracker &= ~Move.MOVE_JUMP;
        }
        public delegate void WindowHandle( IntPtr window, int width, int height );
        public static void WindowMove( IntPtr window, int width, int height )
        {
            WindowSizeChanged( width, height );
            MainWorld.player.camera.Perspective = Matrix.Perspective( fov, (float) width / height, 0.01f, 1000.0f );
        }
        public delegate void MouseHandle( IntPtr window, MouseButton mouse, Actions act, int mods );
        public static void MouseClick( IntPtr window, MouseButton mouse, Actions act, int mods )
        {
            if ( act == Actions.PRESSED )
            {
                switch ( mouse )
                {
                    case MouseButton.LEFT:
                    {
                        if ( MainWindow.Started ) //editor open
                        {
                            Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                            Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                            RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt, MainWorld.player.camera );
                            if ( hit.bHit )
                            {
                                MainWindow.Instance.SelectedEntity = hit.HitEnt;
                                MainWindow.Instance.SelectedFace = hit.HitFace;
                            }
                        }
                        break;
                    }
                    case MouseButton.RIGHT:
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
}
