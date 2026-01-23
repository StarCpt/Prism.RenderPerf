using Prism.Common;
using SharpDX.Direct3D11;
using System;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Prism.RenderPerf;

public static class Extensions
{
    public static BufferMapping MapWriteDiscard(this IBuffer buffer)
    {
        return new BufferMapping(buffer.Buffer, MapMode.WriteDiscard);
    }

    public static BufferMapping MapWriteDiscard(this IBuffer buffer, MyRenderContext rc)
    {
        return new BufferMapping(rc.DeviceContext, buffer.Buffer, MapMode.WriteDiscard);
    }

    [ThreadStatic]
    private static (Buffer[], int[], int[])? _temp = null;

    public static unsafe void SetConstantBuffer(this MyVertexStage stage, int slot, IConstantBuffer constantBuffer, int offsetInBytes, int num16ByteConstants)
    {
        if (offsetInBytes % 256 != 0)
            throw new Exception("Constant buffer offset must be a multiple of 256 bytes.");

        var (cbvs, offsets, nums) = _temp ??= (new Buffer[1], new int[1], new int[1]);
        cbvs[0] = constantBuffer.Buffer;
        offsets[0] = offsetInBytes / 16;
        nums[0] = num16ByteConstants;

        stage.m_constantBuffers[slot] = null;
        ((DeviceContext1)stage.m_deviceContext).VSSetConstantBuffers1(slot, 1, cbvs, offsets, nums);
        stage.m_statistics.SetConstantBuffers++;
    }

    public static unsafe void SetConstantBuffer(this MyPixelStage stage, int slot, IConstantBuffer constantBuffer, int offsetInBytes, int num16ByteConstants)
    {
        if (offsetInBytes % 256 != 0)
            throw new Exception("Constant buffer offset must be a multiple of 256 bytes.");

        var (cbvs, offsets, nums) = _temp ??= (new Buffer[1], new int[1], new int[1]);
        cbvs[0] = constantBuffer.Buffer;
        offsets[0] = offsetInBytes / 16;
        nums[0] = num16ByteConstants;

        stage.m_constantBuffers[slot] = null;
        ((DeviceContext1)stage.m_deviceContext).PSSetConstantBuffers1(slot, 1, cbvs, offsets, nums);
        stage.m_statistics.SetConstantBuffers++;
    }
}
