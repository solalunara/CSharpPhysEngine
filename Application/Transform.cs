using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    public class Transform
    {
        public Transform( Vector Position, Vector Scale, Matrix Rotation )
        {
            this.Position = Position;
            this.Scale = Scale;
            this.Rotation = Rotation;
            Update();
        }
        public Matrix ThisToWorld;
        public Matrix WorldToThis;

        public Vector Position;
        public Vector Scale;
        public Matrix Rotation;

        public void Update()
        {
            Matrix.GLMTranslate( Position, out Matrix PosMatrix );
            Matrix.GLMScale( Scale, out Matrix SclMatrix );
            ThisToWorld = PosMatrix * Rotation * SclMatrix;
            WorldToThis = -ThisToWorld;
        }

        //NOTE: these are only local, absolute versions are implemented in baseentity
        public Vector TransformDirection( Vector dir ) => (Vector) ( ThisToWorld * new Vector4( dir, 0.0f ) );
        public Vector TransformPoint( Vector pt ) => (Vector) ( ThisToWorld * new Vector4( pt, 1.0f ) );
        public Vector InverseTransformDirection( Vector dir ) => (Vector) ( WorldToThis * new Vector4( dir, 0.0f ) );
        public Vector InverseTransformPoint( Vector pt ) => (Vector) ( WorldToThis * new Vector4( pt, 1.0f ) );
    }
}
