using System;
using System.Collections.Generic;
using VRage.Utils;
using static VRageRender.MyBillboard;

namespace Prism.RenderPerf.Billboards;

class BillboardRenderGroupUnordered : BillboardRenderGroup
{
    private class BatchPool
    {
        private readonly List<Batch> _items = [];
        private int _allocIndex = 0;

        public Batch Alloc()
        {
            if (_items.Count == _allocIndex)
            {
                Batch newItem = new();
                _items.Add(newItem);
                _allocIndex++;
                return newItem;
            }
            else
            {
                return _items[_allocIndex++];
            }
        }

        public void DeallocAll()
        {
            for (int i = _allocIndex - 1; i >= 0; i--)
            {
                _items[i].Clear();
            }
            _allocIndex = 0;
        }

        public void Clear()
        {
            _items.Clear();
            _allocIndex = 0;
        }
    }

    private readonly List<(MyStringId MaterialId, Batch Batch)> _batches = [];
    private readonly BatchPool _pool = new();

    public BillboardRenderGroupUnordered(BlendTypeEnum blendType) : base(blendType)
    {
        if (blendType is not BlendTypeEnum.LDR && blendType is not BlendTypeEnum.PostPP)
        {
            throw new Exception($"Use {nameof(BillboardRenderGroupOrdered)}");
        }
    }

    protected override Batch GetOrCreateBatch(MyStringId materialId)
    {
        if (_batches.Count > 0 && _batches[_batches.Count - 1].MaterialId == materialId)
        {
            return _batches[_batches.Count - 1].Batch;
        }
        else
        {
            Batch newBatch = _pool.Alloc();
            _batches.Add((materialId, newBatch));
            return newBatch;
        }
    }

    public override IEnumerable<MyStringId> GetMaterialsInUse()
    {
        int count = _batches.Count;
        for (int i = 0; i < count; i++)
        {
            yield return _batches[i].MaterialId;
        }
    }

    public override IEnumerable<(MyStringId, Batch)> GetOrderedBatches()
    {
        return _batches;
    }

    public override void Clear()
    {
        base.Clear();

        _pool.DeallocAll();
        _batches.ClearFast();
    }
}
