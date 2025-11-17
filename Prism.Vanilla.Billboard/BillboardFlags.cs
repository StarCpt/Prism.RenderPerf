using System;

namespace Prism.Vanilla.Billboard;

[Flags]
enum BillboardFlags : int
{
    None = 0,
    SingleChannel = 1,
    AlphaCutout   = 1 << 1,
    OIT           = 1 << 2,
    SoftParticle  = 1 << 3,
    MAX_NUM       = (1 << 4) - 1,
}
