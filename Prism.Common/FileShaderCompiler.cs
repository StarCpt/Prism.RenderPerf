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
            private readonly string _baseSystemIncludePath;

            public FileIncludeHandler(string compileFilePath, string baseSystemIncludePath)
            {
                _compileFilePath = Path.GetDirectoryName(compileFilePath);
                _baseSystemIncludePath = baseSystemIncludePath;
            }

            public void Close(Stream stream)
            {
                stream.Dispose();
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                if (fileName == null)
                {
                    throw new ArgumentNullException(nameof(fileName));
                }

                string includeFilePath;
                if (type is IncludeType.Local)
                {
                    if (parentStream is FileStream fs)
                    {
                        string parentFilePath = Path.GetDirectoryName(fs.Name);
                        includeFilePath = Path.Combine(parentFilePath, fileName);
                    }
                    else
                    {
                        includeFilePath = Path.Combine(_compileFilePath, fileName);
                    }
                }
                else if (type is IncludeType.System)
                {
                    includeFilePath = Path.Combine(_baseSystemIncludePath, fileName);
                }
                else
                {
                    throw new ArgumentException($"Invalid {nameof(IncludeType)}.");
                }

                return File.OpenRead(includeFilePath);
            }

            public void Dispose()
            {
                Shadow?.Dispose();
            }
        }

        private readonly string _baseShaderPath;
        private readonly string _gameShaderBasePath;

        public FileShaderCompiler(string baseShaderPath, string gameShaderBasePath)
        {
            _baseShaderPath = baseShaderPath;
            _gameShaderBasePath = gameShaderBasePath;
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
                    new FileIncludeHandler(filePath, _gameShaderBasePath));
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
                    new FileIncludeHandler(filePath, _gameShaderBasePath));
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
                    new FileIncludeHandler(filePath, _gameShaderBasePath));
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
                    new FileIncludeHandler(filePath, _gameShaderBasePath));
                return new ComputeShader(device, compilation);
            }
        }
    }
}
