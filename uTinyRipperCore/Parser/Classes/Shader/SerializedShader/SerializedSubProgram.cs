namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedSubProgram : IAssetReadable
	{
		public static int ToSerializedVersion(Version version)
		{
			// KeywordIndices has been renamed to GlobalKeywordIndices
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}

			// TODO:
			return 2;
			// return 1;
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasLocalKeywordIndices(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasSamplers(Version version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasShaderRequirements(Version version) => version.IsGreaterEqual(2017, 2);

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlignKeywordIndices(Version version) => version.IsGreaterEqual(2017, 1);

		public void Read(AssetReader reader)
		{
			BlobIndex = reader.ReadUInt32();
			Channels.Read(reader);
			GlobalKeywordIndices = reader.ReadUInt16Array();
			if (IsAlignKeywordIndices(reader.Version))
			{
				reader.AlignStream();
			}
			if (HasLocalKeywordIndices(reader.Version))
			{
				LocalKeywordIndices = reader.ReadUInt16Array();
				reader.AlignStream();
			}

			ShaderHardwareTier = reader.ReadByte();
			GpuProgramType = (ShaderGpuProgramType)reader.ReadByte();
			reader.AlignStream();

			VectorParams = reader.ReadAssetArray<VectorParameter>();
			MatrixParams = reader.ReadAssetArray<MatrixParameter>();
			TextureParams = reader.ReadAssetArray<TextureParameter>();
			BufferParams = reader.ReadAssetArray<BufferBinding>();
			ConstantBuffers = reader.ReadAssetArray<ConstantBuffer>();
			ConstantBufferBindings = reader.ReadAssetArray<BufferBinding>();
			UAVParams = reader.ReadAssetArray<UAVParameter>();

			if (HasSamplers(reader.Version))
			{
				Samplers = reader.ReadAssetArray<SamplerParameter>();
			}
			if (HasShaderRequirements(reader.Version))
			{
				ShaderRequirements = reader.ReadInt32();
			}
		}

		public void Export(ShaderWriter writer, ShaderType type, bool isTier)
		{
			writer.WriteIndent(4);
#warning TODO: convertion (DX to HLSL)
			writer.Write("SubProgram \"{0} ", GpuProgramType.ToGPUPlatform(writer.Platform));
			if (isTier)
			{
				writer.Write("hw_tier{0} ", ShaderHardwareTier.ToString("00"));
			}
			writer.Write("\" {\n");
			writer.WriteIndent(5);

			GPUPlatform gpuPlatform = GpuProgramType.ToGPUPlatform(writer.Platform);
			int platformIndex = writer.Shader.Platforms.IndexOf(gpuPlatform);
			writer.Shader.Blobs[platformIndex].SubPrograms[BlobIndex].Export(writer, type);

			writer.Write('\n');
			writer.WriteIndent(4);
			writer.Write("}\n");
		}

		public uint BlobIndex { get; set; }
		/// <summary>
		/// KeywordIndices previously
		/// </summary>
		public ushort[] GlobalKeywordIndices { get; set; }
		public ushort[] LocalKeywordIndices { get; set; }
		public byte ShaderHardwareTier { get; set; }
		public ShaderGpuProgramType GpuProgramType { get; set; }
		public VectorParameter[] VectorParams { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public TextureParameter[] TextureParams { get; set; }
		public BufferBinding[] BufferParams { get; set; }
		public ConstantBuffer[] ConstantBuffers { get; set; }
		public BufferBinding[] ConstantBufferBindings { get; set; }
		public UAVParameter[] UAVParams { get; set; }
		public SamplerParameter[] Samplers { get; set; }
		public int ShaderRequirements { get; set; }

		public ParserBindChannels Channels;
	}
}
