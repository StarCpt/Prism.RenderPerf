using System;

namespace Prism.RenderPerf;

[Flags]
enum BillboardType : int
{
    Quad = 0,
    Tri  = 1,
    COUNT = 2,
}
