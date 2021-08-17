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
        static bool CursorControl = true;
        static void Main( string[] args )
        {
            Render_Interface.Init( out IntPtr window, out IntPtr shader, out IntPtr camera, out IntPtr world, out IntPtr inputdata );
            IntPtr[] texture = { Texture_Interface.InitTexture( Texture_Interface.ToCString( "themasterpiece.png" ) ) };
            Brush_Interface.InitBrush( new Vector( -10, -10, -11 ), new Vector( 10, 10, -9 ), texture, 1, world );
            Brush_Interface.InitBrush( new Vector( -10, -11, -10 ), new Vector( 10, -9, 10 ), texture, 1, world );

            InputHandle inptptr = Input;
            Render_Interface.SetInputCallback( Marshal.GetFunctionPointerForDelegate( inptptr ) );

            long starttime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            float lasttime = 0;

            while ( !Render_Interface.ShouldTerminate( window ) )
            {
                float time = ( DateTimeOffset.Now.ToUnixTimeMilliseconds() - starttime ) / 1000.0f;
                float frametime = time - lasttime;
                float fps = 1 / frametime;
                lasttime = time;
                Console.WriteLine( fps );

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

}
