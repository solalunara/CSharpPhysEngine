using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    class Dim2Box : BoxEnt
    {
        public Dim2Box( Point2<float> Mins, Point2<float> Maxs, Texture tex ) : base( new( Mins.x, Mins.y, -1 ), new( Maxs.x, Maxs.y, 1 ), Array.Empty<Texture>() )
        {
            for ( int i = 0; i < Meshes.Length; ++i )
                Meshes[ i ].texture = tex;
        }
        public virtual bool IsButton() => false;
    }
    class Button : Dim2Box
    {
        public Button( Point2<float> Mins, Point2<float> Maxs, Texture tex, Click ClickCallback ) : base( new( Mins.x, Mins.y ), new( Maxs.x, Maxs.y ), tex )
        {
            this.ClickCallback = ClickCallback;
        }
        public Click ClickCallback;

        public override bool IsButton() => true;
    }
    public delegate void Click();
}
