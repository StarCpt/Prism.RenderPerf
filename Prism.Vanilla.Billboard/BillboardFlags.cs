using System;

namespace Prism.Vanilla.Billboard;

[Flags]
enum BillboardFlags : int
{
    None = 0,
    SingleChannel = 1,
    AlphaCutout = 1 << 1,
    OIT = 1 << 2,
    MAX_NUM = (1 << 3) - 1,
}
