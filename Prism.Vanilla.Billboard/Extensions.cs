using Prism.Common;
using SharpDX.Direct3D11;
using System;
using VRage.Render11.RenderContext;
using VRage.Render11.Resources;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Prism.Vanilla.Billboard;

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

    public static void SetConstantBuffer(this MyAllShaderStages stage, int slot, IConstantBuffer constantBuffer, int offsetInBytes, int num16ByteConstants)
    {
        if (offsetInBytes % 256 != 0)
        {
            throw new Exception("Constant buffer offset must be a multiple of 256 bytes.");
        }

        Buffer[] cbvs = [constantBuffer.Buffer];
        int[] offsets = [offsetInBytes / 16];
        int[] nums = [num16ByteConstants];

        stage.m_vertexStage.m_constantBuffers[slot] = null;
        stage.m_pixelStage.m_constantBuffers[slot] = null;
        stage.m_computeStage.m_constantBuffers[slot] = null;

        ((DeviceContext1)stage.m_vertexStage.m_deviceContext).VSSetConstantBuffers1(slot, 1, cbvs, offsets, nums);
        ((DeviceContext1)stage.m_pixelStage.m_deviceContext).PSSetConstantBuffers1(slot, 1, cbvs, offsets, nums);
        ((DeviceContext1)stage.m_computeStage.m_deviceContext).CSSetConstantBuffers1(slot, 1, cbvs, offsets, nums);

        stage.m_vertexStage.m_statistics.SetConstantBuffers++;
        stage.m_pixelStage.m_statistics.SetConstantBuffers++;
        stage.m_computeStage.m_statistics.SetConstantBuffers++;
    }
}
