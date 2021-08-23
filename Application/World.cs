using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    class World
    {
        public World()
        {
            WorldEnts = new List<EHandle>();
        }
        public World( params EHandle[] Ents )
        {
            WorldEnts = new List<EHandle>( Ents );
        }
        public List<EHandle> WorldEnts;

        public BaseEntity[] GetEntList()
        {
            BaseEntity[] ret = new BaseEntity[ WorldEnts.Count ];
            for ( int i = 0; i < ret.Length; ++i )
                ret[ i ] = WorldEnts[ i ].ent;
            return ret;
        }
    }
}
