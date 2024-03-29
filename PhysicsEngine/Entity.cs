﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    class Camera : BaseEntity
    {
        public Camera( Matrix Perspective ) : base( Array.Empty<(FaceMesh, string)>(), new( new(), new( 1, 1, 1 ), Matrix.IdentityMatrix() ) )
        {
            this.Perspective = Perspective;
        }
        public Camera( BaseEntity ent ) : base( ent ) { this.Perspective = Matrix.IdentityMatrix(); }
        public Matrix Perspective;
    }

    abstract class Player
    {
        public Camera camera;
        public PhysObj Body;
    }
    class Player3D : Player
    {
        public static readonly Vector EYE_CENTER_OFFSET = new( 0, 0.5f, 0 );
        public static readonly BBox PLAYER_NORMAL_BBOX = new( new Vector( -.5f, -1.0f, -.5f ), new Vector( .5f, 1.0f, .5f ) );
        public static readonly BBox PLAYER_CROUCH_BBOX = new( new Vector( -.5f, -0.5f, -.5f ), new Vector( .5f, 0.5f, .5f ) );
        public static readonly (FaceMesh, string)[] PLAYER_NORMAL_FACES = new BaseEntity( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, Array.Empty<(Texture, string)>() ).Meshes;
        public static readonly (FaceMesh, string)[] PLAYER_CROUCH_FACES = new BaseEntity( PLAYER_CROUCH_BBOX.mins, PLAYER_CROUCH_BBOX.maxs, Array.Empty<(Texture, string)>() ).Meshes;
        public const float PLAYER_MASS = 50.0f;
        public const float PLAYER_ROTI = float.PositiveInfinity;

        public Player3D( Matrix Perspective, Vector Coeffs, float Mass, float RotI )
        {
            _crouched = false;
            Body = new PhysObj( new BaseEntity( PLAYER_NORMAL_BBOX.mins, PLAYER_NORMAL_BBOX.maxs, Array.Empty<(Texture, string)>() ), Coeffs, Mass, RotI, new(), PhysicsEnvironment.Default_Gravity );
            camera = new Camera( Perspective )
            {
                Parent = Body.LinkedEnt
            };
            camera.SetLocalOrigin( EYE_CENTER_OFFSET );
        }

        private bool _crouched;
        public BaseEntity HeldEnt;
        public void Crouch()
        {
            if ( !_crouched )
            {
                _crouched = true;
                Body.LinkedEnt.Meshes = PLAYER_CROUCH_FACES;
                camera.SetLocalOrigin( new Vector() );
            }
        }
        public void UnCrouch()
        {
            if ( _crouched )
            {
                _crouched = false;
                Body.LinkedEnt.Meshes = PLAYER_NORMAL_FACES;
                camera.SetLocalOrigin( EYE_CENTER_OFFSET );
            }
        }
    }

    class BBox
    {
        public BBox( Vector mins, Vector maxs )
        {
            this.mins = mins;
            this.maxs = maxs;
        }
        public Vector mins;
        public Vector maxs;

        //member methods
        public bool TestCollisionAABB( BBox bbox, Vector ptThis, Vector ptB2 )
        {
            Vector ptWorldMins1 = this.mins + ptThis;
            Vector ptWorldMaxs1 = this.maxs + ptThis;
            Vector ptWorldMins2 = bbox.mins + ptB2;
            Vector ptWorldMaxs2 = bbox.maxs + ptB2;
            bool bCollisionX = ptWorldMins1.x <= ptWorldMaxs2.x && ptWorldMaxs1.x >= ptWorldMins2.x;
            bool bCollisionY = ptWorldMins1.y <= ptWorldMaxs2.y && ptWorldMaxs1.y >= ptWorldMins2.y;
            bool bCollisionZ = ptWorldMins1.z <= ptWorldMaxs2.z && ptWorldMaxs1.z >= ptWorldMins2.z;
            return bCollisionX && bCollisionY && bCollisionZ;
        }
        public bool TestCollisionPoint( Vector pt, Vector ptThis )
        {
            bool bShouldCollide = true;
            for ( int i = 0; i < 3; ++i )
                if ( !( pt[ i ] > mins[ i ] + ptThis[ i ] && pt[ i ] < maxs[ i ] + ptThis[ i ] ) )
                    bShouldCollide = false;
            return bShouldCollide;
        }
        public Plane GetCollisionPlane( Vector pt, Vector ptB )
        {
            Vector ptWorldMins = mins + ptB;
            Vector ptWorldMaxs = maxs + ptB;

            Plane[] planes =
            {
                new Plane( new Vector( 0, 0, 1 ), Vector.Dot( new Vector( 0, 0, 1 ), ptWorldMaxs ) ),
                new(new Vector(0, 0, -1), Vector.Dot(new Vector(0, 0, -1), ptWorldMins)),
                new Plane( new Vector( 0, 1, 0 ), Vector.Dot( new Vector( 0, 1, 0 ), ptWorldMaxs ) ),
                new Plane( new Vector( 0,-1, 0 ), Vector.Dot( new Vector( 0,-1, 0 ), ptWorldMins ) ),
                new Plane( new Vector( 1, 0, 0 ), Vector.Dot( new Vector( 1, 0, 0 ), ptWorldMaxs ) ),
                new Plane( new Vector( -1, 0, 0 ), Vector.Dot( new Vector( -1, 0, 0 ), ptWorldMins ) ),
            };

            float[] fPlaneDists =
            {
                Vector.Dot( planes[ 0 ].Normal, pt ) - planes[ 0 ].Dist,
                Vector.Dot( planes[ 1 ].Normal, pt ) - planes[ 1 ].Dist,
                Vector.Dot( planes[ 2 ].Normal, pt ) - planes[ 2 ].Dist,
                Vector.Dot( planes[ 3 ].Normal, pt ) - planes[ 3 ].Dist,
                Vector.Dot( planes[ 4 ].Normal, pt ) - planes[ 4 ].Dist,
                Vector.Dot( planes[ 5 ].Normal, pt ) - planes[ 5 ].Dist,
            };

            float fMaxDist = fPlaneDists[ 0 ];
            int iMaxIndex = 0;
            for ( int i = 0; i < 6; ++i )
            {
                if ( fPlaneDists[ i ] > fMaxDist )
                {
                    iMaxIndex = i;
                    fMaxDist = fPlaneDists[ i ];
                }
            }

            return planes[ iMaxIndex ];
        }
        public Vector GetCollisionNormal( Vector pt, Vector ptB )
        {
            Plane p = GetCollisionPlane( pt, ptB );
            return p.Normal;
        }
    }
}
