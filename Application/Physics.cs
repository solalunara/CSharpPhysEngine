using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    class PhysicsObject
    {
        public static readonly Vector Default_Gravity = new Vector( 0, -10, 0 );
        public PhysicsObject( IntPtr pLinkedEnt, Vector vGravity, float Mass = 1.0f )
        {
            this.pLinkedEnt = pLinkedEnt;
            this.vGravity = vGravity;
            this.Mass = Mass;
        }
        public IntPtr pLinkedEnt;
        public Vector vGravity;
        public float Mass
        { get; set; }
    }
}
