using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;
using static VRageRender.MyBillboard;

namespace Prism.RenderPerf.Billboards;

class BillboardRenderGroupOrdered : BillboardRenderGroup
{
    private readonly Dictionary<MyStringId, Batch> _batches = new(MyStringId.Comparer);
    private readonly List<MyStringId> _batchOrder = [];
    private bool _batchOrderSorted = false;

    public BillboardRenderGroupOrdered(BlendTypeEnum blendType) : base(blendType)
    {
        if (blendType is BlendTypeEnum.LDR || blendType is BlendTypeEnum.PostPP)
        {
            throw new Exception($"Use {nameof(BillboardRenderGroupUnordered)}");
        }
    }

    protected override Batch GetOrCreateBatch(MyStringId materialId)
    {
        Batch batch = _batches.GetValueOrNew(materialId);
        if (batch.BillboardCount == 0)
        {
            _batchOrder.Add(materialId);
        }
        return batch;
    }

    public override IEnumerable<MyStringId> GetMaterialsInUse()
    {
        foreach (var (key, batch) in _batches)
        {
            if (batch.BillboardCount != 0)
            {
                yield return key;
            }
        }
    }

    public override IEnumerable<(MyStringId, Batch)> GetOrderedBatches()
    {
        if (!_batchOrderSorted)
        {
            _batchOrderSorted = true;
            _batchOrder.Sort(MyStringId.Comparer);
        }

        int batchCount = _batchOrder.Count;
        for (int i = 0; i < batchCount; i++)
        {
            MyStringId id = _batchOrder[i];
            yield return (id, _batches[id]);
        }
    }

    public override void Clear()
    {
        base.Clear();

        foreach (var batch in _batches.Values)
        {
            batch.Clear();
        }
        _batchOrder.ClearFast();
        _batchOrderSorted = false;
    }
}
