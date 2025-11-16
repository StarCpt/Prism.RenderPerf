using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Text;

namespace Prism.Common
{
    public class FileShaderCompiler : IShaderCompiler
    {
        private class FileIncludeHandler : Include
        {
            public IDisposable Shadow { get; set; }

            private readonly string _compileFilePath;

            public FileIncludeHandler(string compileFilePath)
            {
                _compileFilePath = Path.GetDirectoryName(compileFilePath);
            }

            public void Close(Stream stream)
            {
                stream.Dispose();
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                if (type != IncludeType.Local)
                {
                    throw new ArgumentException($"Unsupported {nameof(type)} argument: {type}, must be {IncludeType.Local}.");
                }

                if (fileName == null)
                {
                    throw new ArgumentNullException(nameof(fileName));
                }

                string includeFilePath;
                if (parentStream is FileStream fs)
                {
                    string parentFilePath = Path.GetDirectoryName(fs.Name);
                    includeFilePath = Path.Combine(parentFilePath, fileName);
                }
                else
                {
                    includeFilePath = Path.Combine(_compileFilePath, fileName);
                }

                return File.OpenRead(includeFilePath);
            }

            public void Dispose()
            {
                Shadow?.Dispose();
            }
        }

        private readonly string _baseShaderPath;

        public FileShaderCompiler(string baseShaderPath)
        {
            _baseShaderPath = baseShaderPath;
        }

        public PixelShader CompilePixel(Device device, string id, string entryPoint, params ShaderMacro[] defines)
        {
            string filePath = Path.Combine(_baseShaderPath, id);
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string fileText = sr.ReadToEnd();
                CompilationResult compilation = ShaderBytecode.Compile(
                    fileText,
                    entryPoint,
                    "ps_5_0",
                    ShaderFlags.OptimizationLevel3,
                    EffectFlags.None,
                    defines,
                    new FileIncludeHandler(filePath));
                return new PixelShader(device, compilation);
            }
        }

        public VertexShader CompileVertex(Device device, string id, string entryPoint, params ShaderMacro[] defines)
        {
            string filePath = Path.Combine(_baseShaderPath, id);
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string fileText = sr.ReadToEnd();
                CompilationResult compilation = ShaderBytecode.Compile(
                    fileText,
                    entryPoint,
                    "vs_5_0",
                    ShaderFlags.OptimizationLevel3,
                    EffectFlags.None,
                    defines,
                    new FileIncludeHandler(filePath));
                return new VertexShader(device, compilation);
            }
        }

        public CompilationResult CompileVertexBytecode(Device device, string id, string entryPoint, params ShaderMacro[] defines)
        {
            string filePath = Path.Combine(_baseShaderPath, id);
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string fileText = sr.ReadToEnd();
                return ShaderBytecode.Compile(
                    fileText,
                    entryPoint,
                    "vs_5_0",
                    ShaderFlags.OptimizationLevel3,
                    EffectFlags.None,
                    defines,
                    new FileIncludeHandler(filePath));
            }
        }

        public ComputeShader CompileCompute(Device device, string id, string entryPoint, params ShaderMacro[] defines)
        {
            string filePath = Path.Combine(_baseShaderPath, id);
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string fileText = sr.ReadToEnd();
                CompilationResult compilation = ShaderBytecode.Compile(
                    fileText,
                    entryPoint,
                    "cs_5_0",
                    ShaderFlags.OptimizationLevel3,
                    EffectFlags.None,
                    defines,
                    new FileIncludeHandler(filePath));
                return new ComputeShader(device, compilation);
            }
        }
    }
}
