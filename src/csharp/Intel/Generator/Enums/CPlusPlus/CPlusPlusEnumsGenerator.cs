// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Generator.Documentation;
using Generator.Documentation.CPlusPlus;
using Generator.IO;

namespace Generator.Enums.CPlusPlus {
	[Generator(TargetLanguage.CPlusPlus)]
	sealed class CPlusPlusEnumsGenerator : EnumsGenerator {
		readonly IdentifierConverter idConverter;
		readonly Dictionary<TypeId, FullEnumFileInfo?> toFullFileInfo;
		readonly Dictionary<TypeId, PartialEnumFileInfo?> toPartialFileInfo;
		readonly CPlusPlusDocCommentWriter docWriter;
		readonly DeprecatedWriter deprecatedWriter;

		sealed class FullEnumFileInfo {
			public readonly string Filename;
			public readonly string Namespace;
			public readonly string? Define;
			public readonly string? BaseType;

			public FullEnumFileInfo(string filename, string @namespace, string? define = null, string? baseType = null) {
				Filename = filename;
				Namespace = @namespace;
				Define = define;
				BaseType = baseType;
			}
		}

		sealed class PartialEnumFileInfo {
			public readonly string Id;
			public readonly string Filename;
			public readonly string? BaseType;

			public PartialEnumFileInfo(string id, string filename, string? baseType) {
				Id = id;
				Filename = filename;
				BaseType = baseType;
			}
		}

		public CPlusPlusEnumsGenerator(GeneratorContext generatorContext)
			: base(generatorContext.Types) {
			idConverter = CPlusPlusIdentifierConverter.Create();
			docWriter = new CPlusPlusDocCommentWriter(idConverter);
			deprecatedWriter = new CPlusPlusDeprecatedWriter(idConverter);

			var dirs = genTypes.Dirs;
			toFullFileInfo = new();
			toFullFileInfo.Add(TypeIds.Code, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.Code) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.CodeSize, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.CodeSize) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.ConditionCode, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.ConditionCode) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.CpuidFeature, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.CpuidFeature) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.CpuidFeatureInternal, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, nameof(TypeIds.CpuidFeatureInternal) + ".g.h"), CPlusPlusConstants.InstructionInfoNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.DecoderError, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.DecoderError) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderDefine));
			toFullFileInfo.Add(TypeIds.DecoderOptions, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.DecoderOptions) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderDefine, baseType: "std::uint32_t"));
			toFullFileInfo.Add(TypeIds.EvexOpCodeHandlerKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.EvexOpCodeHandlerKind) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderEvexDefine, baseType: "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.MvexOpCodeHandlerKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.MvexOpCodeHandlerKind) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderMvexDefine, baseType: "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.HandlerFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.HandlerFlags) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderDefine, baseType: "std::uint32_t"));
			toFullFileInfo.Add(TypeIds.LegacyHandlerFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.LegacyHandlerFlags) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderDefine, baseType: "std::uint32_t"));
			toFullFileInfo.Add(TypeIds.MemorySize, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MemorySize) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.LegacyOpCodeHandlerKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.LegacyOpCodeHandlerKind) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderDefine, baseType: "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.PseudoOpsKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.FormatterNamespace, nameof(TypeIds.PseudoOpsKind) + ".g.h"), CPlusPlusConstants.FormatterNamespace, CPlusPlusConstants.AnyFormatterDefine));
			toFullFileInfo.Add(TypeIds.Register, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.Register) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.SerializedDataKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.SerializedDataKind) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderDefine, baseType: "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.TupleType, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.TupleType) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderOrEncoderOrOpCodeInfoDefine));
			toFullFileInfo.Add(TypeIds.VexOpCodeHandlerKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.DecoderNamespace, nameof(TypeIds.VexOpCodeHandlerKind) + ".g.h"), CPlusPlusConstants.DecoderNamespace, CPlusPlusConstants.DecoderVexOrXopDefine, baseType: "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.Mnemonic, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.Mnemonic) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.GasCtorKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.GasFormatterNamespace, "CtorKind.g.h"), CPlusPlusConstants.GasFormatterNamespace, CPlusPlusConstants.GasFormatterDefine));
			toFullFileInfo.Add(TypeIds.IntelCtorKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IntelFormatterNamespace, "CtorKind.g.h"), CPlusPlusConstants.IntelFormatterNamespace, CPlusPlusConstants.IntelFormatterDefine));
			toFullFileInfo.Add(TypeIds.MasmCtorKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.MasmFormatterNamespace, "CtorKind.g.h"), CPlusPlusConstants.MasmFormatterNamespace, CPlusPlusConstants.MasmFormatterDefine));
			toFullFileInfo.Add(TypeIds.NasmCtorKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "CtorKind.g.h"), CPlusPlusConstants.NasmFormatterNamespace, CPlusPlusConstants.NasmFormatterDefine));
			toFullFileInfo.Add(TypeIds.GasSizeOverride, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.GasFormatterNamespace, "SizeOverride.g.h"), CPlusPlusConstants.GasFormatterNamespace, CPlusPlusConstants.GasFormatterDefine));
			toFullFileInfo.Add(TypeIds.GasInstrOpInfoFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.GasFormatterNamespace, "InstrOpInfoFlags.g.h"), CPlusPlusConstants.GasFormatterNamespace, CPlusPlusConstants.GasFormatterDefine, "std::uint16_t"));
			toFullFileInfo.Add(TypeIds.IntelSizeOverride, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IntelFormatterNamespace, "SizeOverride.g.h"), CPlusPlusConstants.IntelFormatterNamespace, CPlusPlusConstants.IntelFormatterDefine));
			toFullFileInfo.Add(TypeIds.IntelBranchSizeInfo, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IntelFormatterNamespace, "BranchSizeInfo.g.h"), CPlusPlusConstants.IntelFormatterNamespace, CPlusPlusConstants.IntelFormatterDefine));
			toFullFileInfo.Add(TypeIds.IntelInstrOpInfoFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IntelFormatterNamespace, "InstrOpInfoFlags.g.h"), CPlusPlusConstants.IntelFormatterNamespace, CPlusPlusConstants.IntelFormatterDefine, "std::uint16_t"));
			toFullFileInfo.Add(TypeIds.MasmInstrOpInfoFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.MasmFormatterNamespace, "InstrOpInfoFlags.g.h"), CPlusPlusConstants.MasmFormatterNamespace, CPlusPlusConstants.MasmFormatterDefine, "std::uint16_t"));
			toFullFileInfo.Add(TypeIds.NasmSignExtendInfo, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "SignExtendInfo.g.h"), CPlusPlusConstants.NasmFormatterNamespace, CPlusPlusConstants.NasmFormatterDefine));
			toFullFileInfo.Add(TypeIds.NasmSizeOverride, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "SizeOverride.g.h"), CPlusPlusConstants.NasmFormatterNamespace, CPlusPlusConstants.NasmFormatterDefine));
			toFullFileInfo.Add(TypeIds.NasmBranchSizeInfo, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "BranchSizeInfo.g.h"), CPlusPlusConstants.NasmFormatterNamespace, CPlusPlusConstants.NasmFormatterDefine));
			toFullFileInfo.Add(TypeIds.NasmInstrOpInfoFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "InstrOpInfoFlags.g.h"), CPlusPlusConstants.NasmFormatterNamespace, CPlusPlusConstants.NasmFormatterDefine, "std::uint32_t"));
			toFullFileInfo.Add(TypeIds.FastFmtFlags, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.FastFormatterNamespace, "FastFmtFlags.g.h"), CPlusPlusConstants.FastFormatterNamespace, CPlusPlusConstants.FastFormatterDefine, "std::uint8_t"));
			toFullFileInfo.Add(TypeIds.RoundingControl, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.RoundingControl) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.OpKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.OpKind) + ".g.h"), CPlusPlusConstants.IcedNamespace));
			toFullFileInfo.Add(TypeIds.InstrScale, null);
			toFullFileInfo.Add(TypeIds.VectorLength, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.VectorLength) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderOrEncoderDefine));
			toFullFileInfo.Add(TypeIds.MandatoryPrefixByte, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MandatoryPrefixByte) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderOrEncoderDefine, "std::uint32_t"));// 'uint' not 'byte' since it gets zx to uint when OR'ing values
			toFullFileInfo.Add(TypeIds.EncodingKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.EncodingKind) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.DecoderOrEncoderOrInstrInfoOrOpCodeInfoDefine));
			toFullFileInfo.Add(TypeIds.FlowControl, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.FlowControl) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.OpCodeOperandKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.OpCodeOperandKind) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.OpCodeInfoDefine));
			toFullFileInfo.Add(TypeIds.MvexEHBit, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexEHBit) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));
			toFullFileInfo.Add(TypeIds.MvexInfoFlags1, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexInfoFlags1) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));
			toFullFileInfo.Add(TypeIds.MvexInfoFlags2, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexInfoFlags2) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));
			toFullFileInfo.Add(TypeIds.RflagsBits, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.RflagsBits) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.OpAccess, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.OpAccess) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.InstructionInfoDefine));
			toFullFileInfo.Add(TypeIds.MandatoryPrefix, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MandatoryPrefix) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.OpCodeInfoDefine));
			toFullFileInfo.Add(TypeIds.OpCodeTableKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.OpCodeTableKind) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.OpCodeInfoDefine));
			toFullFileInfo.Add(TypeIds.FormatterTextKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.FormatterTextKind) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.AnyFormatterDefine));
			toFullFileInfo.Add(TypeIds.MemorySizeOptions, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MemorySizeOptions) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.AnyFormatterDefine));
			toFullFileInfo.Add(TypeIds.CodeAsmMemoryOperandSize, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "MemoryOperandSize.g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.CodeAssemblerDefine));
			toFullFileInfo.Add(TypeIds.MvexConvFn, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexConvFn) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));
			toFullFileInfo.Add(TypeIds.MvexRegMemConv, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexRegMemConv) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));
			toFullFileInfo.Add(TypeIds.MvexTupleTypeLutKind, new FullEnumFileInfo(CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, nameof(TypeIds.MvexTupleTypeLutKind) + ".g.h"), CPlusPlusConstants.IcedNamespace, CPlusPlusConstants.MvexDefine));

			toPartialFileInfo = new();
			//toPartialFileInfo.Add(TypeIds.InstrFlags1, new PartialEnumFileInfo("InstrFlags1", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Instruction.h"), "std::uint32_t"));
			//toPartialFileInfo.Add(TypeIds.MvexInstrFlags, new PartialEnumFileInfo("MvexInstrFlags", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Instruction.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.OpSize, new PartialEnumFileInfo("OpSize", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Decoder.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.StateFlags, new PartialEnumFileInfo("StateFlags", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Decoder.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.ImpliedAccess, new PartialEnumFileInfo("ImpliedAccess", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.RflagsInfo, new PartialEnumFileInfo("RflagsInfo", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.OpInfo0, new PartialEnumFileInfo("OpInfo0", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.OpInfo1, new PartialEnumFileInfo("OpInfo1", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.OpInfo2, new PartialEnumFileInfo("OpInfo2", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.OpInfo3, new PartialEnumFileInfo("OpInfo3", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.OpInfo4, new PartialEnumFileInfo("OpInfo4", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), null));
			toPartialFileInfo.Add(TypeIds.InfoFlags1, new PartialEnumFileInfo("InfoFlags1", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.InfoFlags2, new PartialEnumFileInfo("InfoFlags2", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.InstructionInfoNamespace, "InfoHandlerFlags.h"), "std::uint32_t"));

			toPartialFileInfo.Add(TypeIds.LegacyOpCodeTable, new PartialEnumFileInfo("LegacyOpCodeTable", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.VexOpCodeTable, new PartialEnumFileInfo("VexOpCodeTable", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.XopOpCodeTable, new PartialEnumFileInfo("XopOpCodeTable", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.EvexOpCodeTable, new PartialEnumFileInfo("EvexOpCodeTable", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.MvexOpCodeTable, new PartialEnumFileInfo("MvexOpCodeTable", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));

			//toPartialFileInfo.Add(TypeIds.FormatterFlowControl, new PartialEnumFileInfo("FormatterFlowControl", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.FormatterNamespace, "FormatterUtils.h"), null));
			//toPartialFileInfo.Add(TypeIds.GasInstrOpKind, new PartialEnumFileInfo("InstrOpKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.GasFormatterNamespace, "InstrInfo.h"), "std::uint8_t"));
			//toPartialFileInfo.Add(TypeIds.IntelInstrOpKind, new PartialEnumFileInfo("InstrOpKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IntelFormatterNamespace, "InstrInfo.h"), "std::uint8_t"));
			//toPartialFileInfo.Add(TypeIds.MasmInstrOpKind, new PartialEnumFileInfo("InstrOpKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.MasmFormatterNamespace, "InstrInfo.h"), "std::uint8_t"));
			//toPartialFileInfo.Add(TypeIds.NasmInstrOpKind, new PartialEnumFileInfo("InstrOpKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "InstrInfo.h"), "std::uint8_t"));
			//toPartialFileInfo.Add(TypeIds.NasmMemorySizeInfo, new PartialEnumFileInfo("MemorySizeInfo", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "InstrInfo.h"), null));
			//toPartialFileInfo.Add(TypeIds.NasmFarMemorySizeInfo, new PartialEnumFileInfo("FarMemorySizeInfo", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.NasmFormatterNamespace, "InstrInfo.h"), null));
			toPartialFileInfo.Add(TypeIds.NumberBase, new PartialEnumFileInfo("NumberBase", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), null));

			toPartialFileInfo.Add(TypeIds.DisplSize, new PartialEnumFileInfo("DisplSize", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.ImmSize, new PartialEnumFileInfo("ImmSize", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), null));
			toPartialFileInfo.Add(TypeIds.EncoderFlags, new PartialEnumFileInfo("EncoderFlags", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.EncFlags1, new PartialEnumFileInfo("EncFlags1", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.EncFlags2, new PartialEnumFileInfo("EncFlags2", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.EncFlags3, new PartialEnumFileInfo("EncFlags3", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.OpCodeInfoFlags1, new PartialEnumFileInfo("OpCodeInfoFlags1", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "OpCodeInfosEnums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.OpCodeInfoFlags2, new PartialEnumFileInfo("OpCodeInfoFlags2", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "OpCodeInfosEnums.h"), "std::uint32_t"));
			//toPartialFileInfo.Add(TypeIds.DecOptionValue, null);
			toPartialFileInfo.Add(TypeIds.InstrStrFmtOption, new PartialEnumFileInfo("InstrStrFmtOption", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "OpCodeInfosEnums.h"), null));
			toPartialFileInfo.Add(TypeIds.WBit, new PartialEnumFileInfo("WBit", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.LBit, new PartialEnumFileInfo("LBit", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "Enums.h"), "std::uint32_t"));
			//toPartialFileInfo.Add(TypeIds.LKind, new PartialEnumFileInfo("LKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.EncoderNamespace, "OpCodeFormatter.h"), "std::uint8_t"));
			//toPartialFileInfo.Add(TypeIds.RepPrefixKind, new PartialEnumFileInfo("RepPrefixKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Instruction.Create.h"), null));
			toPartialFileInfo.Add(TypeIds.RelocKind, new PartialEnumFileInfo("RelocKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "BlockEncoder.h"), null));
			toPartialFileInfo.Add(TypeIds.BlockEncoderOptions, new PartialEnumFileInfo("BlockEncoderOptions", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "BlockEncoder.h"), null));
			toPartialFileInfo.Add(TypeIds.FormatMnemonicOptions, new PartialEnumFileInfo("FormatMnemonicOptions", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Formatter.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.PrefixKind, new PartialEnumFileInfo("PrefixKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOutput.h"), null));
			toPartialFileInfo.Add(TypeIds.DecoratorKind, new PartialEnumFileInfo("DecoratorKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOutput.h"), null));
			toPartialFileInfo.Add(TypeIds.NumberKind, new PartialEnumFileInfo("NumberKind", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOutput.h"), null));
			toPartialFileInfo.Add(TypeIds.SymbolFlags, new PartialEnumFileInfo("SymbolFlags", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "ISymbolResolver.h"), "std::uint32_t"));
			toPartialFileInfo.Add(TypeIds.CC_b, new PartialEnumFileInfo("CC_b", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_ae, new PartialEnumFileInfo("CC_ae", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_e, new PartialEnumFileInfo("CC_e", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_ne, new PartialEnumFileInfo("CC_ne", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_be, new PartialEnumFileInfo("CC_be", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_a, new PartialEnumFileInfo("CC_a", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_p, new PartialEnumFileInfo("CC_p", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_np, new PartialEnumFileInfo("CC_np", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_l, new PartialEnumFileInfo("CC_l", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_ge, new PartialEnumFileInfo("CC_ge", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_le, new PartialEnumFileInfo("CC_le", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
			toPartialFileInfo.Add(TypeIds.CC_g, new PartialEnumFileInfo("CC_g", CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "FormatterOptions.h"), "std::uint8_t"));
		}

		public override void Generate(EnumType enumType) {
			if (toFullFileInfo.TryGetValue(enumType.TypeId, out var fullFileInfo)) {
				if (fullFileInfo is not null)
					WriteFile(fullFileInfo, enumType);
			}
			else if (toPartialFileInfo.TryGetValue(enumType.TypeId, out var partialInfo)) {
				if (partialInfo is not null)
					new FileUpdater(TargetLanguage.CPlusPlus, partialInfo.Id, partialInfo.Filename).Generate(writer => WriteEnum(writer, partialInfo, enumType));
			}
		}

		void WriteEnum(FileWriter writer, EnumType enumType, string? baseType, bool isNested = false) {
			if (enumType.IsPublic && enumType.IsMissingDocs)
				docWriter.WriteSummary(writer, enumType.Documentation.GetComment(TargetLanguage.CPlusPlus), enumType.RawName);
			var theBaseType = baseType is not null ? $" : {baseType}" : string.Empty;
			string enumName = enumType.Name(idConverter);
			writer.WriteLine($"enum class {enumName}{theBaseType} {{");
			using (writer.Indent()) {
				uint expectedValue = 0;
				foreach (var value in enumType.Values) {
					docWriter.WriteSummary(writer, value.Documentation.GetComment(TargetLanguage.CPlusPlus), enumType.RawName);
					using var deprecatedFile = new StringWriter();
					using var dummyFile = new FileWriter(TargetLanguage.CPlusPlus, deprecatedFile);
					deprecatedWriter.WriteDeprecated(dummyFile, value);
					var deprecatedString = deprecatedFile.ToString().Trim();
					if (enumType.IsFlags)
						writer.WriteLine($"{value.Name(idConverter)} {deprecatedString} = 0x{value.Value:X8},");
					else if (expectedValue != value.Value || enumType.IsPublic)
						writer.WriteLine($"{value.Name(idConverter)} {deprecatedString} = {value.Value},");
					else
						writer.WriteLine($"{value.Name(idConverter)} {deprecatedString},");
					expectedValue = value.Value + 1;
				}
			}
			writer.WriteLine("};");

			baseType ??= "int";

			var friend = (isNested) ? "friend " : "";

			if (enumType.IsFlags) {
				foreach (var op in new[] { "^", "|", "&" }) {
					writer.WriteLine($"{friend}constexpr {enumName}& operator{op}=({enumName}& a, const {enumName}& b) {{ return a = ({enumName})(({baseType})a {op} ({baseType})b); }}");
					writer.WriteLine($"{friend}constexpr {enumName} operator{op}(const {enumName}& a, const {enumName}& b) {{ return ({enumName})(({baseType})a {op} ({baseType})b); }}");
				}
				foreach (var op in new[] { "~" }) {
					writer.WriteLine($"{friend}constexpr {enumName} operator{op}(const {enumName}& a) {{ return ({enumName})({op}(({baseType})a)); }}");
				}
			}

			foreach (var op in new[] { "+", "-" }) {
				writer.WriteLine($"{friend}constexpr {baseType} operator{op}(const {enumName}& a, const {enumName}& b) {{ return (({baseType})a {op} ({baseType})b); }}");
				writer.WriteLine($"{friend}constexpr {baseType} operator{op}(const {enumName}& a, const {baseType}& b) {{ return (({baseType})a {op} b); }}");
				writer.WriteLine($"{friend}constexpr {baseType} operator{op}(const {baseType}& a, const {enumName}& b) {{ return (a {op} ({baseType})b); }}");
			}

			foreach (var op in new[] { "++", "--" }) {
				writer.WriteLine($"{friend}constexpr {enumName} operator{op}({enumName}& a, int) {{ auto temp = a; a = {enumName}(a {op[0]} 1); return temp; }}");
				writer.WriteLine($"{friend}constexpr {enumName}& operator{op}({enumName}& a) {{ return a = {enumName}(a {op[0]} 1); }}");
			}

			foreach (var op in new[] { "==", ">=", "<=", ">", "<", "!=" }) {
				writer.WriteLine($"{friend}constexpr bool operator{op}(const {enumName}& a, const {baseType}& b) {{ return (({baseType})a {op} b); }}");
				writer.WriteLine($"{friend}constexpr bool operator{op}(const {baseType}& a, const {enumName}& b) {{ return (a {op} ({baseType})b); }}");
			}

			if (isNested) {
				writer.WriteLine($"friend constexpr std::string {CPlusPlusConstants.IcedNamespace}::ToString(const {enumName}& e);");
			}
		}

		void WriteEnumToString(FileWriter writer, string @namespace, EnumType enumType, string? baseType, bool isNested = false) {
			baseType ??= "int";
			string enumName = $"{@namespace}::{enumType.Name(idConverter)}";
			writer.WriteLine("template <>");
			writer.WriteLine($"constexpr std::string {CPlusPlusConstants.IcedNamespace}::ToString(const {enumName}& e) {{");
			using (writer.Indent()) {
				if (!enumType.IsFlags) {
					writer.WriteLine("switch (e) {");
					using (writer.Indent()) {
						foreach (var value in enumType.Values.Where(v => !v.DeprecatedInfo.IsDeprecated)) {
							var valueName = value.Name(idConverter);
							writer.WriteLine($"case {enumName}::{valueName}: return \"{valueName}\";");
						}
						writer.WriteLine($"default: return Internal::StringHelpers::ToDec(({baseType})e);");
					}
					writer.WriteLine("}");
				}
				else {
					writer.WriteLine("std::string result;");
					writer.WriteLine("auto temp = e;");
					var v0 = enumType.Values.Where(v => v.Value == 0).OrderBy(v => v.DeprecatedInfo.IsDeprecated).FirstOrDefault();
					if (v0 != null) {
						var valueName = v0.Name(idConverter);
						writer.WriteLine($"if (temp == {enumName}::{valueName}) {{");
						using (writer.Indent()) {
							writer.WriteLine($"return \"{valueName}\";");
						}
						writer.WriteLine("}");
					}
					foreach (var value in enumType.Values.Where(v => v.Value != 0).OrderByDescending(v => BitOperations.PopCount(v.Value)).ThenBy(v => v.Value)) {
						var valueName = value.Name(idConverter);
						writer.WriteLine($"if ((temp & {enumName}::{valueName}) == {enumName}::{valueName}) {{");
						using (writer.Indent()) {
							writer.WriteLine($"temp ^= {enumName}::{valueName};");
							writer.WriteLine($"result += \"{valueName}, \";");
						}
						writer.WriteLine("}");
					}
					writer.WriteLine($"if (temp != 0 || result.empty()) return Internal::StringHelpers::ToDec(({baseType})e);");
					writer.WriteLine($"return result.substr(0, result.size() - 2);");
				}
			}
			writer.WriteLine("}");
		}
		void WriteFile(FullEnumFileInfo info, EnumType enumType) {
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(info.Filename))) {
				writer.WriteFileHeader();
				if (info.Define is not null)
					writer.WriteLineNoIndent($"#if {info.Define}");


				writer.WriteLineNoIndent("#include <array>");

				if (info.BaseType is not null) {
					writer.WriteLineNoIndent("#include <cstdint>");
				}

				writer.WriteLineNoIndent("#include <stdexcept>");
				writer.WriteLineNoIndent("#include <string>");
				writer.WriteLineNoIndent($"#include \"{CPlusPlusConstants.GetRelativeInclude(genTypes, Path.GetDirectoryName(info.Filename)!, CPlusPlusConstants.IcedNamespace, "ToString.h")}\"");
				writer.WriteLineNoIndent($"#include \"{CPlusPlusConstants.GetRelativeInclude(genTypes, Path.GetDirectoryName(info.Filename)!, "Iced::Intel::Internal", "StringHelpers.h")}\"");

				writer.WriteLine($"namespace {info.Namespace} {{");

				using (writer.Indent())
					WriteEnum(writer, enumType, info.BaseType, false);

				writer.WriteLine("}");

				// Always write to global namespace
				WriteEnumToString(writer, info.Namespace, enumType, info.BaseType, false);

				if (info.Define is not null)
					writer.WriteLineNoIndent("#endif");
			}
		}

		void WriteEnum(FileWriter writer, PartialEnumFileInfo partialInfo, EnumType enumType) =>
			WriteEnum(writer, enumType, partialInfo.BaseType);
	}
}
