using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Generator.Enums;

namespace Generator {
	static class CPlusPlusConstants {
		public const string IcedNamespace = "Iced::Intel";
		public const string BlockEncoderNamespace = "Iced::Intel::BlockEncoderInternal";
		public const string DecoderNamespace = "Iced::Intel::DecoderInternal";
		public const string EncoderNamespace = "Iced::Intel::EncoderInternal";
		public const string InstructionInfoNamespace = "Iced::Intel::InstructionInfoInternal";
		public const string FormatterNamespace = "Iced::Intel::FormatterInternal";
		public const string GasFormatterNamespace = "Iced::Intel::GasFormatterInternal";
		public const string IntelFormatterNamespace = "Iced::Intel::IntelFormatterInternal";
		public const string MasmFormatterNamespace = "Iced::Intel::MasmFormatterInternal";
		public const string NasmFormatterNamespace = "Iced::Intel::NasmFormatterInternal";
		public const string FastFormatterNamespace = "Iced::Intel::FastFormatterInternal";

		public const string HasSpanDefine = "defined(HAS_SPAN)";
		public const string DecoderDefine = "defined(DECODER)";
		public const string VexDefine = "!defined(NO_VEX)";
		public const string XopDefine = "!defined(NO_XOP)";
		public const string EvexDefine = "!defined(NO_EVEX)";
		public const string MvexDefine = "defined(MVEX)";
		public const string D3nowDefine = "!defined(NO_D3NOW)";
		public const string DecoderVexDefine = "defined(DECODER) && !defined(NO_VEX)";
		public const string DecoderXopDefine = "defined(DECODER) && !defined(NO_XOP)";
		public const string DecoderVexOrXopDefine = "defined(DECODER) && (!defined(NO_VEX) || !defined(NO_XOP))";
		public const string DecoderEvexDefine = "defined(DECODER) && !defined(NO_EVEX)";
		public const string DecoderMvexDefine = "defined(DECODER) && defined(MVEX)";
		public const string EncoderDefine = "defined(ENCODER)";
		public const string CodeAssemblerDefine = "defined(ENCODER) && defined(BLOCK_ENCODER) && defined(CODE_ASSEMBLER)";
		public const string OpCodeInfoDefine = "defined(ENCODER) && defined(OPCODE_INFO)";
		public const string InstructionInfoDefine = "defined(INSTR_INFO)";
		public const string DecoderOrEncoderDefine = "defined(DECODER) || defined(ENCODER)";
		public const string DecoderOrEncoderOrOpCodeInfoDefine = "defined(DECODER) || defined(ENCODER) || (defined(ENCODER) && defined(OPCODE_INFO))";
		public const string DecoderOrEncoderOrInstrInfoOrOpCodeInfoDefine = "defined(DECODER) || defined(ENCODER) || defined(INSTR_INFO) || (defined(ENCODER) && defined(OPCODE_INFO))";
		public const string AnyFormatterDefine = "defined(GAS) || defined(INTEL) || defined(MASM) || defined(NASM) || defined(FAST_FMT)";
		public const string GasIntelNasmFormatterDefine = "defined(GAS) || defined(INTEL) || defined(NASM)";
		public const string GasFormatterDefine = "defined(GAS)";
		public const string IntelFormatterDefine = "defined(INTEL)";
		public const string MasmFormatterDefine = "defined(MASM)";
		public const string NasmFormatterDefine = "defined(NASM)";
		public const string FastFormatterDefine = "defined(FAST_FMT)";

		public static string GetFilename(GenTypes genTypes, string @namespace, params string[] names) =>
			Path.Combine(new[] { genTypes.Dirs.CPlusPlusDir }.Concat(@namespace.Split("::").Skip(1)).Concat(names).ToArray());

		public static string GetRelativeInclude(GenTypes genTypes, string sourceDir, string headerNamespace, string headerName) {
			return Path.GetRelativePath(sourceDir, GetFilename(genTypes, headerNamespace, headerName)).Replace("\\", "/");
		}

		public static string? GetDefine(EncodingKind encoding) =>
			encoding switch {
				EncodingKind.Legacy => null,
				EncodingKind.VEX => VexDefine,
				EncodingKind.EVEX => EvexDefine,
				EncodingKind.XOP => XopDefine,
				EncodingKind.D3NOW => D3nowDefine,
				EncodingKind.MVEX => MvexDefine,
				_ => throw new InvalidOperationException(),
			};

	}
}
