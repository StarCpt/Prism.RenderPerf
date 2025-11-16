using System;

namespace Prism.Vanilla.Billboard;

[Flags]
enum BillboardType : int
{
    Quad = 0,
    Tri  = 1,
    COUNT = 2,
}
