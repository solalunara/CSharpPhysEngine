using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderInterface
{
    // Interfaces for projects that don't include PhysicsEngine
    public interface IEntHandle
    {
        void Close();

        void SetLocalOrigin( Vector pt );
        void SetLocalRot( Matrix r );
        void SetLocalScale( Vector s );
        Vector GetLocalOrigin();
        Matrix GetLocalRot();
        Vector GetLocalScale();

        void SetAbsOrigin( Vector pt );
        void SetAbsRot( Matrix r );
        Vector GetAbsOrigin();
        Matrix GetAbsRot();

        Matrix CalcEntMatrix();

        Plane GetCollisionPlane( Vector pt );

        Vector[] GetVerts();
        Vector[] GetWorldVerts();


        Vector TransformDirection( Vector dir );
        Vector TransformPoint( Vector pt );
        Vector InverseTransformDirection( Vector dir );
        Vector InverseTransformPoint( Vector pt );

        FaceMesh[] Meshes
        {
            get;
            set;
        }

        IEntHandle Parent
        {
            get;
            set;
        }

        ITransformHandle LocalTransform
        {
            get;
            set;
        }

        bool TestCollision( Vector pt );
    }
    public interface ITransformHandle
    {
        Matrix ThisToWorld
        {
            get;
            set;
        }
        Matrix WorldToThis
        {
            get;
            set;
        }

        Vector Position
        {
            get;
            set;
        }
        Vector Scale
        {
            get;
            set;
        }
        Matrix Rotation
        {
            get;
            set;
        }

        Vector QAngles
        {
            get;
            set;
        }
    }

    public interface IWorldHandle
    {
        IEntHandle[] GetEntList();
        IPhysHandle[] GetPhysObjList();
        IPhysHandle GetEntPhysics( IEntHandle ent );
    }
    public interface IPhysHandle
    {
        IEntHandle LinkedEnt
        {
            get;
            set;
        }
    }
}
