using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysEngine
{
    enum Keys
    {
        W = 87,
        S = 83,
        A = 65,
        D = 68,
        SPACE = 32,
        ESCAPE = 256,
        LCONTROL = 341,
        F6 = 295,
        F7 = 296,
        Q = 81,
        E = 69,
        Z = 90,
        X = 88,
        C = 67,
        V = 86,
        F = 70,
        R = 82,
    }
    enum MouseButton
    {
        LEFT =  0,
        RIGHT = 1
    }
    enum Actions
    {
        RELEASED = 0,
        PRESSED = 1,
        HELD = 2,
    }
    enum Move
    {
        MOVE_NONE       = 0,
        MOVE_FORWARD    = 1 << 0,
        MOVE_BACKWARD   = 1 << 1,
        MOVE_LEFT       = 1 << 2,
        MOVE_RIGHT      = 1 << 3,
        MOVE_JUMP       = 1 << 4
    }
    enum Space
    {
        NONE      = 0,
        WORLD     = 1 << 0,
        SELF      = 1 << 1,
    }
}
