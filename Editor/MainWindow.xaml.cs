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
using static System.Diagnostics.Debug;
using static RenderInterface.Renderer;
using Physics;
using Vector = RenderInterface.Vector;

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

                if ( SelectedEntity is BoxEnt Box )
                {
                    Vector vMins = Box.Mins;
                    Mins.Dispatcher.Invoke( new Action( () => Mins.Text = $"{vMins.x}, {vMins.y}, {vMins.z}" ) );
                    Vector vMaxs = Box.Maxs;
                    Maxs.Dispatcher.Invoke( new Action( () => Maxs.Text = $"{vMaxs.x}, {vMaxs.y}, {vMaxs.z}" ) );
                }

                BasePhysics? Physics = World.GetEntPhysics( SelectedEntity );
                if ( Physics is null )
                    return;
                Mass.Dispatcher.Invoke( new Action( () => Mass.Text = Physics.Mass.ToString( CultureInfo.CurrentCulture ) ) );
                RotInertia.Dispatcher.Invoke( new Action( () => RotInertia.Text = Physics.RotInertia.ToString( CultureInfo.CurrentCulture ) ) );
            }
        }
        public int SelectedFace;

        public MainWindow()
        {
            Started = true;
            Instance = this;
            InitializeComponent();
        }

        private void CloseWindow( object sender, EventArgs e )
        {
            Started = false;
            Instance = null;
        }

        private void CreateNewEnt( object sender, RoutedEventArgs e )
        {
            ShouldCreateEnt = true;
            MinsElems = Mins.Text.Split( ',' );
            MaxsElems = Maxs.Text.Split( ',' );
            TexturePath = Texture.Text;
        }

        public static bool ShouldCreateEnt;
        private static string[] MinsElems = Array.Empty<string>();
        private static string[] MaxsElems = Array.Empty<string>();
        private static string TexturePath = "";
        public static void ExternCreateEnt( BaseWorld World )
        {
            ShouldCreateEnt = false;

            if ( World is null )
                return;

            try
            {
                Vector vMins = new( float.Parse( MinsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MinsElems[ 2 ], CultureInfo.CurrentCulture ) );
                Vector vMaxs = new( float.Parse( MaxsElems[ 0 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 1 ], CultureInfo.CurrentCulture ), float.Parse( MaxsElems[ 2 ], CultureInfo.CurrentCulture ) );
                World.WorldEnts.Add( new BoxEnt( vMins, vMaxs, new[] { (FindTexture( TexturePath ), TexturePath) } ) );
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
                ShouldCreateEnt = true;
                MinsElems = Mins.Text.Split( ',' );
                MaxsElems = Maxs.Text.Split( ',' );
                TexturePath = Texture.Text;

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
                PhysObj p = new( SelectedEntity, Vector.One, fMass, fRotInertia, new(), PhysicsEnvironment.Default_Gravity );
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
        }

        private void SetFaceTexture( object sender, RoutedEventArgs e )
        {
            if ( SelectedEntity is null || SelectedFace >= SelectedEntity.Meshes.Length || SelectedFace < 0 )
            {
                Assert( false );
                return;
            }
            TexturePath = Texture.Text;
            SelectedEntity.Meshes[ SelectedFace ].Item2 = TexturePath;
            SelectedEntity.Meshes[ SelectedFace ].Item1.texture = FindTexture( TexturePath );
        }
    }
}
