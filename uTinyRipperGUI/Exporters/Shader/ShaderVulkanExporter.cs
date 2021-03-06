using Smolv;
using SpirV;
using System;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Converters.Shaders;

namespace uTinyRipperGUI.Exporters
{
	public class ShaderVulkanExporter : ShaderTextExporter
	{
		public override void Export(ShaderWriter writer, ref ShaderSubProgram subProgram)
		{
			using (MemoryStream ms = new MemoryStream(subProgram.ProgramData))
			{
				using (BinaryReader reader = new BinaryReader(ms))
				{
					int unknown = reader.ReadInt32();
					int minOffset = subProgram.ProgramData.Length;
#warning TODO: const snippet count? equal to ShaderType count: 5 snippets for < 2019.3, 6 for >= 2019.3
					while (reader.BaseStream.Position < minOffset)
					{
						int offset = reader.ReadInt32();
						int size = reader.ReadInt32();
						if (size > 0)
						{
							if (offset < minOffset)
							{
								minOffset = offset;
							}
							ExportSnippet(writer, ms, offset, size);
						}
					}
				}
			}
		}

		private void ExportSnippet(TextWriter writer, Stream stream, int offset, int size)
		{
			using (PartialStream snippetStream = new PartialStream(stream, offset, size))
			{
				int decodedSize = SmolvDecoder.GetDecodedBufferSize(snippetStream);
				if (decodedSize == 0)
				{
					throw new Exception("Invalid SMOL-V shader header");
				}
				using (MemoryStream decodedStream = new MemoryStream(new byte[decodedSize]))
				{
					if (SmolvDecoder.Decode(stream, size, decodedStream))
					{
						decodedStream.Position = 0;
						Module module = Module.ReadFrom(decodedStream);
						string listing = m_disassembler.Disassemble(module, DisassemblyOptions.Default);
						ExportListing(writer, listing);
					}
					else
					{
						throw new Exception("Unable to decode SMOL-V shader");
					}
				}
			}
		}

		private readonly Disassembler m_disassembler = new Disassembler();
	}
}
