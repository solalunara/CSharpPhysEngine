using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using RenderInterface;
using Physics;
using System.Diagnostics;

namespace PhysEngine
{
    static class Program
    {
        static string DirName = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
        static Move MoveTracker = Move.MOVE_NONE;
        static World MainWorld;
        static List<Texture> Textures = new();
        static Texture FindTexture( string Name )
        {
            for ( int i = 0; i < Textures.Count; ++i )
            {
                if ( Textures[ i ].TextureName == Name )
                    return Textures[ i ];
            }
            Debug.Assert( false, "Failed to find texture with given name" );
            return new Texture();
        }

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
                if ( args.Length > 0 )
                {
                    for ( int i = 0; i < args.Length; ++i )
                    {
                        switch ( args[ i ].ToLower() )
                        {
                            case "-2d":
                                Run2D();
                                break;
                            case "-3d":
                                Run3D();
                                break;
                        }
                    }
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
            Renderer.Init( out IntPtr window );

            string[] TextureNames = Directory.EnumerateFiles( DirName + "/Textures" ).ToArray();
            for ( int i = 0; i < TextureNames.Length; ++i )
            {
                Textures.Add( new Texture( TextureNames[ i ] ) );
            }

            Shader shader = new( DirName + "/Shaders/VertexShader.vert", DirName + "/Shaders/FragmentShader.frag" );
            Shader GUI = new( DirName + "/Shaders/GUIVert.vert", DirName + "/Shaders/GUIFrag.frag" );
            shader.SetAmbientLight( 0.0f );

            Light[] testlights = 
            { 
                new Light( new Vector4( 0, -5,-5, 1 ), new Vector4( 0.7f, 0.7f, 1, 1 ), 20 ),
                new Light( new Vector4( 0, -5, 5, 1 ), new Vector4( 0.7f, 0.7f, 1, 1 ), 20 )
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
            FaceMesh CrosshairMesh = new( CrosshairVerts, CrosshairInds, new Texture( DirName + "/Textures\\Crosshair.png" ), new Vector( 0, 0, 0 ) );


            MainWorld = new( PhysicsEnvironment.Default_Gravity, 0.02f );

            Texture[] dirt = { FindTexture( DirName + "/Textures\\dirt.png" ) };
            Texture[] grass = { FindTexture( DirName + "/Textures\\grass.png" ) };
            MainWorld.player = new Player3D( persp, PhysObj.Default_Coeffs, Player3D.PLAYER_MASS, Player3D.PLAYER_ROTI );
            //player = new Player2D();
            MainWorld.Add
            (
                new PhysObj( new BoxEnt( new Vector( -1, -1, -7 ), new Vector( 1, 0, -5 ), grass ), PhysObj.Default_Coeffs, 25, 20, new() )
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

                    /*
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
                    */
                }
                else
                    Mouse.ShowMouse( window );


                TimeSpan TimeDiff = DateTime.Now - MainWorld.Environment.LastSimTime;

                Renderer.StartFrame( window );
                Renderer.SetCameraValues( shader, MainWorld.player.camera.Perspective, -MainWorld.player.camera.CalcEntMatrix() );
                foreach ( BaseEntity b in MainWorld.WorldEnts )
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

            string[] TextureNames = Directory.EnumerateFiles( DirName + "/Textures" ).ToArray();
            for ( int i = 0; i < TextureNames.Length; ++i )
            {
                Textures.Add( new Texture( TextureNames[ i ] ) );
            }

            Shader shader = new( DirName + "/Shaders/VertexShader.vert", DirName + "/Shaders/FragmentShader.frag" );
            MainWorld = new( PhysicsEnvironment.Default_Gravity, 0.02f );
            Texture[] dirt = { FindTexture( DirName + "/Textures/dirt.png" ) };
            Texture button = FindTexture( DirName + "/Textures/button.png" );

            Texture Sun = FindTexture( DirName + "/Textures/Sun.png" );
            Texture Earth = FindTexture( DirName + "/Textures/Earth.png" );
            Texture Jupiter = FindTexture( DirName + "/Textures/Jupiter.png" );

            MainWorld.Add
            (
                new Button( new( -9, -2.5f ), new( -4, 2.5f ), Sun, () => Console.WriteLine( "The sun" ) ),
                new Button( new( -2.5f, -2.5f ), new( 2.5f, 2.5f ), Earth, () => Console.WriteLine( "The earth" ) ),
                new Button( new( 4, -2.5f ), new( 9, 2.5f ), Jupiter, () => Console.WriteLine( "The planet jupiter" ) )
            );
            shader.SetAmbientLight( 1.0f );

            InputHandle inptptr = Input;
            Renderer.SetInputCallback( window, Marshal.GetFunctionPointerForDelegate( inptptr = Input ) );
            WindowHandle wndptr = WindowMove;
            Renderer.SetWindowMoveCallback( window, Marshal.GetFunctionPointerForDelegate( wndptr ) );
            MouseHandle msptr = MouseClick;
            Renderer.SetMouseButtonCallback( window, Marshal.GetFunctionPointerForDelegate( msptr ) );

            MainWorld.player = new Player2D();

            while ( !Renderer.ShouldTerminate( window ) )
            {
                /*
                if ( rtMech.HasFlag( RuntimeMechanics.FIRELEFT ) )
                {
                    rtMech &= ~RuntimeMechanics.FIRELEFT;
                    Point2<double> ms = Mouse.GetMouseNormalizedPos( window );
                    Vector MousePos = MainWorld.player.camera.TransformPoint( new( (float) ms.x * 10, (float) ms.y * 10, 0 ) );
                    MousePos.z = 0;
                    for ( int i = 0; i < MainWorld.WorldEnts.Count; ++i )
                    {
                        if ( MainWorld.WorldEnts[ i ].GetType() == typeof( Button ) )
                        {
                            Button b = (Button)MainWorld.WorldEnts[ i ];
                            if ( b.TestCollision( MousePos ) )
                                b.ClickCallback();

                        }
                    }
                }
                */
                Renderer.StartFrame( window );
                Matrix Cam = -MainWorld.player.camera.CalcEntMatrix();
                Renderer.SetCameraValues( shader, MainWorld.player.camera.Perspective, -MainWorld.player.camera.CalcEntMatrix() );
                foreach ( BaseEntity b in MainWorld.WorldEnts )
                {
                    b.SetAbsRot( Matrix.RotMatrix( 0.5f, new( 0, 1, 1 ) ) * b.GetAbsRot() );
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
            MainWorld.Close();
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
                        break;
                    case Keys.F7:
                        break;
                    case Keys.Q:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        if ( hit.bHit )
                        {
                            PhysObj HitPhys = (PhysObj)MainWorld.GetEntPhysics( hit.HitEnt );
                            if ( HitPhys != null )
                            {
                                if ( HitPhys.AngularMomentum.y > 0 )
                                    HitPhys.Torque -= new Vector( 0, 10, 10 );
                                else
                                    HitPhys.Torque += new Vector( 0, 10, 10 );
                            }
                        }
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
                            RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
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
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        Vector ptCenter = new();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = MainWorld.player.camera.GetAbsOrigin() + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        MainWorld.Add( new BoxEnt( mins, maxs, new[] { FindTexture( DirName + "/Textures/grass.png" ) } ) );
                        break;
                    }
                    case Keys.X:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        Vector ptCenter = new();
                        if ( hit.bHit )
                            ptCenter = hit.ptHit + hit.vNormal * 0.5f;
                        else
                            ptCenter = MainWorld.player.camera.GetAbsOrigin() + TransformedForward / 10;

                        Vector mins = ptCenter + new Vector( -.5f, -.5f, -.5f );
                        Vector maxs = ptCenter + new Vector( 0.5f, 0.5f, 0.5f );
                        MainWorld.Add( new BoxEnt( mins, maxs, new[] { FindTexture( DirName + "/Textures/dirt.png" ) } ) );
                        break;
                    }
                    case Keys.F:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        if ( hit.bHit )
                            MainWorld.WorldEnts.Remove( hit.HitEnt );
                        break;
                    }
                    case Keys.R:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        if ( hit.bHit || MainWorld.GetEntPhysics( hit.HitEnt ) == null )
                            MainWorld.Add( new PhysObj( hit.HitEnt, PhysObj.Default_Coeffs, 25, 1, new() ) );
                        break;
                    }
                    case Keys.C:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        if ( hit.bHit )
                        {
                            BaseEntity HitEnt = hit.HitEnt;
                            HitEnt.LocalTransform.Scale *= new Vector( 1.1f, 1.1f, 1.1f );
                        }
                        break;
                    }
                    case Keys.V:
                    {
                        Vector TransformedForward = (Vector)( MainWorld.player.camera.GetAbsRot() * new Vector4( 0, 0, -30, 1 ) );
                        Vector EntPt = MainWorld.player.camera.GetAbsOrigin() + TransformedForward;
                        RayHitInfo hit = MainWorld.TraceRay( MainWorld.player.camera.GetAbsOrigin(), EntPt, MainWorld.player.Body.LinkedEnt );
                        if ( hit.bHit )
                        {
                            BaseEntity HitEnt = hit.HitEnt;
                            HitEnt.LocalTransform.Scale *= new Vector( 0.9f, 0.9f, 0.9f );
                        }
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
            Renderer.WindowSizeChanged( width, height );
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
                        break;
                    case MouseButton.RIGHT:
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
}
