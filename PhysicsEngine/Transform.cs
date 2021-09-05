using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderInterface;

namespace PhysEngine
{
    class Transform
    {
        public Transform( Vector Position, Vector Scale, Matrix Rotation )
        {
            this._Position = Position;
            this._Scale = Scale;
            this._Rotation = Rotation;
            Update();
        }
        public Matrix ThisToWorld;
        public Matrix WorldToThis;

        private Vector _Position;
        public Vector Position
        {
            get => _Position;
            set
            {
                _Position = value;
                Update();
            }
        }

        private Vector _Scale;
        public Vector Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Update();
            }
        }

        private Matrix _Rotation;
        public Matrix Rotation
        {
            get => _Rotation;
            set
            {
                _Rotation = value;
                Update();
            }
        }

        public Vector QAngles
        {
            get
            {
                Vector angles = new();
                if ( Rotation.Columns[ 0 ][ 1 ] > 0.998 )
                { // singularity at north pole
                    angles.y = (float) Math.Atan2( Rotation.Columns[ 2 ][ 0 ], Rotation.Columns[ 2 ][ 2 ] );
                    angles.x = (float) Math.PI / 2;
                    angles.z = 0;
                    return angles;
                }
                if ( Rotation.Columns[ 0 ][ 1 ] < -0.998 )
                { // singularity at south pole
                    angles.y = (float) Math.Atan2( Rotation.Columns[ 2 ][ 0 ], Rotation.Columns[ 2 ][ 2 ] );
                    angles.x = (float) -Math.PI / 2;
                    angles.z = 0;
                    return angles;
                }
                angles.x = (float) Math.Asin( Rotation.Columns[ 0 ][ 1 ] );
                angles.y = (float) Math.Atan2( -Rotation.Columns[ 0 ][ 2 ], Rotation.Columns[ 0 ][ 0 ] );
                angles.z = (float) Math.Atan2( -Rotation.Columns[ 2 ][ 1 ], Rotation.Columns[ 1 ][ 1 ] );
                return angles;
            }
            set
            {
                Rotation = Matrix.RotMatrix( value.y, new Vector( 0, 1, 0 ) ) * Matrix.RotMatrix( value.x, new Vector( 1, 0, 0 ) ) * Matrix.RotMatrix( value.z, new Vector( 0, 0, 1 ) );
            }
        }

        public void Update()
        {
            ThisToWorld = Matrix.Translate( Position ) * Rotation * Matrix.Scale( Scale );
            WorldToThis = -ThisToWorld;
        }

        //NOTE: these are only local, absolute versions are implemented in baseentity
        public Vector TransformDirection( Vector dir ) => (Vector) ( ThisToWorld * new Vector4( dir, 0.0f ) );
        public Vector TransformPoint( Vector pt ) => (Vector) ( ThisToWorld * new Vector4( pt, 1.0f ) );
        public Vector InverseTransformDirection( Vector dir ) => (Vector) ( WorldToThis * new Vector4( dir, 0.0f ) );
        public Vector InverseTransformPoint( Vector pt ) => (Vector) ( WorldToThis * new Vector4( pt, 1.0f ) );
    }
}
