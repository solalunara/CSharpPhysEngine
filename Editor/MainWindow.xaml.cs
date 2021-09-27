using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using RenderInterface;
using static RenderInterface.Renderer;
using Physics;
using Vector = RenderInterface.Vector;
using Matrix = RenderInterface.Matrix;
using Transform = RenderInterface.Transform;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool Started;
        public static MainWindow? Instance;
        public BaseWorld? World;
        public void SetShader( string ApplicationDirectory )
        { 
            if ( !ShaderSet )
                shader = new( ApplicationDirectory + "\\Shaders\\VertexShader.vert", ApplicationDirectory + "\\Shaders\\FragmentShader.frag" );
            ShaderSet = true;
        }

        private IntPtr Window;
        private Matrix Perspective;
        private Transform Camera;
        private Shader shader;
        private bool ShaderSet;

        private BaseEntity? _SelectedEntity;
        public BaseEntity? SelectedEntity
        {
            get => _SelectedEntity;
            set
            {
                _SelectedEntity = value;
                if ( SelectedEntity is null || World is null )
                    return;

                Vector Pos = SelectedEntity.GetAbsOrigin();
                Pos0.Dispatcher.Invoke( new Action( () => Pos0.Text = Pos.x.ToString( CultureInfo.CurrentCulture ) ) );
                Pos1.Dispatcher.Invoke( new Action( () => Pos1.Text = Pos.y.ToString( CultureInfo.CurrentCulture ) ) );
                Pos2.Dispatcher.Invoke( new Action( () => Pos2.Text = Pos.z.ToString( CultureInfo.CurrentCulture ) ) );
                Vector Scl = SelectedEntity.LocalTransform.Scale;
                Scl0.Dispatcher.Invoke( new Action( () => Scl0.Text = Scl.x.ToString( CultureInfo.CurrentCulture ) ) );
                Scl1.Dispatcher.Invoke( new Action( () => Scl1.Text = Scl.y.ToString( CultureInfo.CurrentCulture ) ) );
                Scl2.Dispatcher.Invoke( new Action( () => Scl2.Text = Scl.z.ToString( CultureInfo.CurrentCulture ) ) );
                Vector Rot = SelectedEntity.LocalTransform.QAngles;
                Rot0.Dispatcher.Invoke( new Action( () => Rot0.Text = Rot.x.ToString( CultureInfo.CurrentCulture ) ) );
                Rot1.Dispatcher.Invoke( new Action( () => Rot1.Text = Rot.y.ToString( CultureInfo.CurrentCulture ) ) );
                Rot2.Dispatcher.Invoke( new Action( () => Rot2.Text = Rot.z.ToString( CultureInfo.CurrentCulture ) ) );

                BasePhysics? Physics = World.GetEntPhysics( SelectedEntity );
                if ( Physics is null )
                    return;
                Mass.Dispatcher.Invoke( new Action( () => Mass.Text = Physics.Mass.ToString( CultureInfo.CurrentCulture ) ) );
                RotInertia.Dispatcher.Invoke( new Action( () => RotInertia.Text = Physics.RotInertia.ToString( CultureInfo.CurrentCulture ) ) );
            }
        }

        public MainWindow()
        {
            Started = true;
            Instance = this;
            InitializeComponent();
            Init( out Window );
            Perspective = Matrix.Ortho( 10, 10, 10, 10, 0.01f, 1000 );
            Camera = new( new(), new( 1, 1, 1 ), Matrix.IdentityMatrix() );
        }

        private void CloseWindow( object sender, EventArgs e )
        {
            Started = false;
            Instance = null;
            Terminate();
        }

        private void CreateNewEnt( object sender, RoutedEventArgs e )
        {
            if ( World is null )
                return;

            try
            {
                string[] MinsElems = Mins.Text.Split( ',' );
                string[] MaxsElems = Maxs.Text.Split( ',' );
                //remove parenthases
                //MinsElems[ 0 ] = MinsElems[ 0 ][ 1.. ];
                //MinsElems[ ^1 ] = MinsElems[ ^1 ][ ..MinsElems.Length ];
                //MaxsElems[ 0 ] = MaxsElems[ 0 ][ 1.. ];
                //MaxsElems[ ^1 ] = MaxsElems[ ^1 ][ ..MaxsElems.Length ];

                Vector vMins = new( float.Parse( MinsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 2 ], CultureInfo.CurrentCulture ) );
                Vector vMaxs = new( float.Parse( MaxsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 2 ], CultureInfo.CurrentCulture ) );
                World.WorldEnts.Add( new BoxEnt( vMins, vMaxs, new[] { (Renderer.FindTexture( Texture.Text ), Texture.Text) } ) );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message );
            }
        }

        private void ReplaceEnt( object sender, RoutedEventArgs e )
        {
            if ( SelectedEntity is null || World is null )
                return;
            try
            {
                string[] MinsElems = Mins.Text.Split( ',' );
                string[] MaxsElems = Maxs.Text.Split( ',' );
                //remove parenthases
                MinsElems[ 0 ] = MinsElems[ 0 ][ 1.. ];
                MinsElems[ ^1 ] = MinsElems[ ^1 ][ ..( MinsElems.Length - 1 ) ];
                MaxsElems[ 0 ] = MaxsElems[ 0 ][ 1.. ];
                MaxsElems[ ^1 ] = MaxsElems[ ^1 ][ ..( MinsElems.Length - 1 ) ];

                Vector vMins = new( float.Parse( MinsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 2 ], CultureInfo.CurrentCulture ) );
                Vector vMaxs = new( float.Parse( MaxsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 2 ], CultureInfo.CurrentCulture ) );
                World.WorldEnts.Add( new BoxEnt( vMins, vMaxs, new[] { (Renderer.FindTexture( Texture.Text ), Texture.Text) } ) );

                if ( World.GetEntPhysics( SelectedEntity ) is BasePhysics Phys )
                    World.RemovePhysicsObject( Phys );
                World.WorldEnts.Remove( SelectedEntity );
                SelectedEntity = null;
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message );
            }
        }

        private void DestroyEnt( object sender, RoutedEventArgs e )
        {
            if ( SelectedEntity is null || World is null )
                return;

            if ( World.GetEntPhysics( SelectedEntity ) is BasePhysics Phys )
                World.RemovePhysicsObject( Phys );
            World.WorldEnts.Remove( SelectedEntity );
            SelectedEntity = null;
        }

        private void AddPhys( object sender, RoutedEventArgs e )
        {
            if ( SelectedEntity is null || World is null )
                return;

            if ( World.GetEntPhysics( SelectedEntity ) is BasePhysics Phys )
                World.RemovePhysicsObject( Phys );

            try
            {
                float fMass = float.Parse( Mass.Text, CultureInfo.CurrentCulture );
                float fRotInertia = float.Parse( RotInertia.Text, CultureInfo.CurrentCulture );
                PhysObj p = new( SelectedEntity, Vector.One, fMass, fRotInertia, new() );
                World.AddPhysicsObject( p );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message );
            }
        }

        private void UpdateEnt( object sender, RoutedEventArgs e )
        {
            if ( SelectedEntity is null || World is null )
                return;

            try
            {
                Vector Pos = new( float.Parse( Pos0.Text, CultureInfo.CurrentCulture ), float.Parse( Pos1.Text, CultureInfo.CurrentCulture ), float.Parse( Pos2.Text, CultureInfo.CurrentCulture ) );
                Vector Rot = new( float.Parse( Rot0.Text, CultureInfo.CurrentCulture ), float.Parse( Rot1.Text, CultureInfo.CurrentCulture ), float.Parse( Rot2.Text, CultureInfo.CurrentCulture ) );
                Vector Scl = new( float.Parse( Scl0.Text, CultureInfo.CurrentCulture ), float.Parse( Scl1.Text, CultureInfo.CurrentCulture ), float.Parse( Scl2.Text, CultureInfo.CurrentCulture ) );
                SelectedEntity.LocalTransform.Scale = Scl;
                SelectedEntity.LocalTransform.QAngles = Rot;
                SelectedEntity.SetAbsOrigin( Pos );
            }
            catch ( Exception ex )
            {
                MessageBox.Show( ex.StackTrace, ex.Message );
            }

            StartFrame( Window );
            
            SetCameraValues( shader, Perspective, Camera.WorldToThis );
            foreach ( BaseEntity b in World.WorldEnts )
            {
                SetRenderValues( shader, b.CalcEntMatrix() );
                foreach ( (FaceMesh, string) m in b.Meshes )
                {
                    if ( !m.Item1.texture.Initialized )
                        continue; //nothing to render

                    m.Item1.Render( shader );
                }
            }
            EndFrame( Window );
        }
    }
}
