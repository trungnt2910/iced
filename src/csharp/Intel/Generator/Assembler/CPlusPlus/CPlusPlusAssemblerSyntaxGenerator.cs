// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Generator.Documentation.CPlusPlus;
using Generator.Enums;
using Generator.IO;
using Generator.Tables;

namespace Generator.Assembler.CPlusPlus {
	[Generator(TargetLanguage.CPlusPlus)]
	sealed class CPlusPlusAssemblerSyntaxGenerator : AssemblerSyntaxGenerator {
		readonly IdentifierConverter idConverter;
		readonly CPlusPlusDocCommentWriter docWriter;
		readonly EnumType registerType;
		readonly EnumType memoryOperandSizeType;

		static readonly List<(string, int, string[], string)> declareDataList = new() {
			("db", 1, new[] { "std::uint8_t", "std::int8_t" }, "CreateDeclareByte"),
			("dw", 2, new[] { "std::uint16_t", "std::int16_t" }, "CreateDeclareWord"),
			("dd", 4, new[] { "std::uint32_t", "std::int32_t", "float" }, "CreateDeclareDword"),
			("dq", 8, new[] { "std::uint64_t", "std::int64_t", "double" }, "CreateDeclareQword"),
		};

		public CPlusPlusAssemblerSyntaxGenerator(GeneratorContext generatorContext)
			: base(generatorContext.Types) {
			idConverter = CPlusPlusIdentifierConverter.Create();
			docWriter = new CPlusPlusDocCommentWriter(idConverter);
			registerType = genTypes[TypeIds.Register];
			memoryOperandSizeType = genTypes[TypeIds.CodeAsmMemoryOperandSize];
		}

		static string GetRegisterSuffix(RegisterDef def) => GetRegisterSuffix(def.GetRegisterKind());
		static string GetRegisterSuffix(RegisterKind kind) =>
			kind switch {
				RegisterKind.GPR8 => "8",
				RegisterKind.GPR16 => "16",
				RegisterKind.GPR32 => "32",
				RegisterKind.GPR64 => "64",
				RegisterKind.IP => "IP",
				RegisterKind.Segment => "Segment",
				RegisterKind.ST => "ST",
				RegisterKind.CR => "CR",
				RegisterKind.DR => "DR",
				RegisterKind.TR => "TR",
				RegisterKind.BND => "BND",
				RegisterKind.K => "K",
				RegisterKind.MM => "MM",
				RegisterKind.XMM => "XMM",
				RegisterKind.YMM => "YMM",
				RegisterKind.ZMM => "ZMM",
				RegisterKind.TMM => "TMM",
				_ => throw new InvalidOperationException(),
			};

		protected override void GenerateRegisters((RegisterKind kind, RegisterDef[] regs)[] regGroups) {
			var filename = CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "AssemblerRegisters.g.h");
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(filename))) {
				writer.WriteFileHeader();
				writer.WriteLineNoIndent($"#if {CPlusPlusConstants.CodeAssemblerDefine}");

				writer.WriteLine("#include \"../Register.g.h\"");
				writer.WriteLine("#include \"../AssemblerRegister.g.h\"");
				writer.WriteLine();

				writer.WriteLine($"namespace {CPlusPlusConstants.IcedNamespace} {{");
				//writer.WriteLine(CPlusPlusConstants.PragmaMissingDocsDisable);
				using (writer.Indent()) {
					writer.WriteLine("namespace AssemblerRegisters {");
					using (writer.Indent()) {
						foreach (var regDef in regGroups.SelectMany(a => a.regs)) {
							var asmRegName = GetAsmRegisterName(regDef);
							var registerTypeName = $"AssemblerRegister{GetRegisterSuffix(regDef)}";
							writer.WriteLine($"inline constexpr {registerTypeName} {asmRegName} = {registerTypeName}({idConverter.ToDeclTypeAndValue(regDef.Register)});");
						}
					}
					writer.WriteLine("}");
				}
				writer.WriteLine("}");
				writer.WriteLineNoIndent("#endif");
			}
		}

		protected override void GenerateRegisterClasses(RegisterClassInfo[] infos) {
			var filename = CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "AssemblerRegister.defs.g.h");
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(filename))) {
				writer.WriteFileHeader();
				writer.WriteLineNoIndent($"#if {CPlusPlusConstants.CodeAssemblerDefine}");
				writer.WriteLine("#include <cstdint>");
				writer.WriteLine("#include \"../Register.g.h\"");
				writer.WriteLine("#include \"AssemblerMemoryOperand.h\"");
				writer.WriteLine();

				writer.WriteLine($"namespace {CPlusPlusConstants.IcedNamespace} {{");
				writer.WriteLine();
				using (writer.Indent()) {
					var regNoneName = idConverter.ToDeclTypeAndValue(registerType[nameof(Register.None)]);
					var registerTypeName = registerType.Name(idConverter);
					var memOpNoneName = idConverter.ToDeclTypeAndValue(memoryOperandSizeType[nameof(MemoryOperandSize.None)]);

					for (int i = 0; i < infos.Length; ++i) {
						// In C++, we need the forward declarations.
						writer.WriteLine($"struct AssemblerRegister{GetRegisterSuffix(infos[i].Kind)};");
					}
					writer.WriteLine($"struct AssemblerMemoryOperand;");

					// Then write the class declarations.
					for (int i = 0; i < infos.Length; ++i) {
						var reg = infos[i];
						var className = $"AssemblerRegister{GetRegisterSuffix(reg.Kind)}";
						var isName = reg.Kind switch {
							RegisterKind.None => throw new InvalidOperationException(),
							RegisterKind.GPR8 => "GPR8",
							RegisterKind.GPR16 => "GPR16",
							RegisterKind.GPR32 => "GPR32",
							RegisterKind.GPR64 => "GPR64",
							RegisterKind.IP => "IP",
							RegisterKind.Segment => "SegmentRegister",
							RegisterKind.ST => "ST",
							RegisterKind.CR => "CR",
							RegisterKind.DR => "DR",
							RegisterKind.TR => "TR",
							RegisterKind.BND => "BND",
							RegisterKind.K => "K",
							RegisterKind.MM => "MM",
							RegisterKind.XMM => "XMM",
							RegisterKind.YMM => "YMM",
							RegisterKind.ZMM => "ZMM",
							RegisterKind.TMM => "TMM",
							_ => throw new InvalidOperationException(),
						};

						writer.WriteLine();

						writer.WriteLine("/// <summary>");
						writer.WriteLine("/// An assembler register used with <see cref=\"Assembler\"/>.");
						writer.WriteLine("/// </summary>");
						writer.WriteLine($"struct {className} {{");
						using (writer.Indent()) {
							writer.WriteLine("public:");
							// Public fields
							using (writer.Indent()) {
								writer.WriteLine("/// <summary>");
								writer.WriteLine("/// The register value.");
								writer.WriteLine("/// </summary>");
								writer.WriteLine($"const {registerTypeName} Value;");
								if (reg.NeedsState) {
									writer.WriteLine();
									writer.WriteLine("const AssemblerOperandFlags Flags;");
								}
							}
							writer.WriteLine();
							writer.WriteLine("public:");
							using (writer.Indent()) {
								writer.WriteLine("/// <summary>");
								writer.WriteLine("/// Creates a new instance.");
								writer.WriteLine("/// </summary>");
								writer.WriteLine("/// <param name=\"value\">A Register</param>");
								writer.WriteLine($"constexpr {className}({registerTypeName} value);");
								writer.WriteLine();
								if (reg.NeedsState) {
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Creates a new instance.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine("/// <param name=\"value\">A register</param>");
									writer.WriteLine("/// <param name=\"flags\">The mask</param>");
									writer.WriteLine($"constexpr {className}({registerTypeName} value, AssemblerOperandFlags flags);");
									for (int j = 1; j <= 7; j++) {
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine($"/// Apply mask Register K{j}.");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} k{j}() const;");
									}
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Apply mask Zeroing.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine($"constexpr {className} z() const;");
									if (reg.HasSaeOrEr) {
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine("/// Suppress all exceptions");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} sae() const;");
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine("/// Round to nearest (even)");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} rn_sae() const;");
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine("/// Round down (toward -inf)");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} rd_sae() const;");
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine("/// Round up (toward +inf)");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} ru_sae() const;");
										writer.WriteLine();
										writer.WriteLine("/// <summary>");
										writer.WriteLine("/// Round toward zero (truncate)");
										writer.WriteLine("/// </summary>");
										writer.WriteLine($"constexpr {className} rz_sae() const;");
									}
								}
								writer.WriteLine();
								writer.WriteLine("/// <summary>");
								writer.WriteLine($"/// Converts a <see cref=\"{className}\"/> to a <see cref=\"{registerTypeName}\"/>.");
								writer.WriteLine("/// </summary>");
								writer.WriteLine($"/// <param name=\"reg\">{className}</param>");
								writer.WriteLine("/// <returns></returns>");
								writer.WriteLine($"constexpr operator {registerTypeName}({className} reg) const;");
								if (reg.IsGPR16_32_64) {
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Adds a register (base) to another register (index) and return a memory operand.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine("/// <param name=\"right\">The index register</param>");
									writer.WriteLine("/// <returns></returns>");
									writer.WriteLine($"constexpr AssemblerMemoryOperand operator +(const {className}& right) const;");
									if (reg.IsGPR32_64) {
										foreach (var mm in new[] { "XMM", "YMM", "ZMM" }) {
											writer.WriteLine();
											writer.WriteLine("/// <summary>");
											writer.WriteLine("/// Adds a register (base) to another register (index) and return a memory operand.");
											writer.WriteLine("/// </summary>");
											writer.WriteLine("/// <param name=\"right\">The index register</param>");
											writer.WriteLine("/// <returns></returns>");
											writer.WriteLine($"constexpr AssemblerMemoryOperand operator +(const AssemblerRegister{mm}& right) const;");
										}
									}
								}
								if (reg.IsGPR16_32_64 || reg.IsVector) {
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Adds a register (base) with a displacement and return a memory operand.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine("/// <param name=\"displacement\">The displacement</param>");
									writer.WriteLine("/// <returns></returns>");
									writer.WriteLine($"constexpr AssemblerMemoryOperand operator +(std::int64_t displacement) const;");
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Subtracts a register (base) with a displacement and return a memory operand.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine("/// <param name=\"displacement\">The displacement</param>");
									writer.WriteLine("/// <returns></returns>");
									writer.WriteLine($"constexpr AssemblerMemoryOperand operator -(std::int64_t displacement) const;");
									writer.WriteLine();
									writer.WriteLine("/// <summary>");
									writer.WriteLine("/// Multiplies an index register by a scale and return a memory operand.");
									writer.WriteLine("/// </summary>");
									writer.WriteLine("/// <param name=\"scale\">The scale</param>");
									writer.WriteLine("/// <returns></returns>");
									writer.WriteLine($"constexpr AssemblerMemoryOperand operator *(std::int32_t scale) const;");
								}
								writer.WriteLine();
								writer.WriteLine("/// <inheritdoc />");
								writer.WriteLine("constexpr int GetHashCode() const;");
								writer.WriteLine();
								writer.WriteLine("/// <summary>");
								writer.WriteLine($"/// Equality operator for <see cref=\"{className}\"/>");
								writer.WriteLine("/// </summary>");
								writer.WriteLine("/// <param name=\"right\">Register</param>");
								writer.WriteLine("/// <returns></returns>");
								writer.WriteLine($"constexpr bool operator ==(const {className}& right) const;");
								writer.WriteLine();
								writer.WriteLine("/// <summary>");
								writer.WriteLine($"/// Inequality operator for <see cref=\"{className}\"/>");
								writer.WriteLine("/// </summary>");
								writer.WriteLine("/// <param name=\"right\">Register</param>");
								writer.WriteLine("/// <returns></returns>");
								writer.WriteLine($"constexpr bool operator !=(const {className}& right) const;");
								writer.WriteLine();
								writer.WriteLine("/// <summary>");
								writer.WriteLine($"/// Inequality operator for <see cref=\"{className}\"/> with <see cref=\"Register\"/>");
								writer.WriteLine("/// </summary>");
								writer.WriteLine("/// <param name=\"right\">Register</param>");
								writer.WriteLine("/// <returns></returns>");
								writer.WriteLine($"constexpr bool operator ==(const {registerTypeName}& right) const;");
							}
						}
						writer.WriteLine("}");
					}

					//for (int i = 0; i < infos.Length; i++) {
					//	var reg = infos[i];
					//	var className = $"AssemblerRegister{GetRegisterSuffix(reg.Kind)}";
					//	var isName = reg.Kind switch {
					//		RegisterKind.None => throw new InvalidOperationException(),
					//		RegisterKind.GPR8 => "GPR8",
					//		RegisterKind.GPR16 => "GPR16",
					//		RegisterKind.GPR32 => "GPR32",
					//		RegisterKind.GPR64 => "GPR64",
					//		RegisterKind.IP => "IP",
					//		RegisterKind.Segment => "SegmentRegister",
					//		RegisterKind.ST => "ST",
					//		RegisterKind.CR => "CR",
					//		RegisterKind.DR => "DR",
					//		RegisterKind.TR => "TR",
					//		RegisterKind.BND => "BND",
					//		RegisterKind.K => "K",
					//		RegisterKind.MM => "MM",
					//		RegisterKind.XMM => "XMM",
					//		RegisterKind.YMM => "YMM",
					//		RegisterKind.ZMM => "ZMM",
					//		RegisterKind.TMM => "TMM",
					//		_ => throw new InvalidOperationException(),
					//	};

					//	if (i != 0)
					//		writer.WriteLine();
					//	writer.WriteLine("/// <summary>");
					//	writer.WriteLine("/// An assembler register used with <see cref=\"Assembler\"/>.");
					//	writer.WriteLine("/// </summary>");
					//	writer.WriteLine($"struct {className} {{");
					//	using (writer.Indent()) {
					//		writer.WriteLine("/// <summary>");
					//		writer.WriteLine("/// Creates a new instance.");
					//		writer.WriteLine("/// </summary>");
					//		writer.WriteLine("/// <param name=\"value\">A Register</param>");
					//		writer.WriteLine($"public {className}({registerTypeName} value) {{");
					//		using (writer.Indent()) {
					//			writer.WriteLine($"if (!value.Is{isName}())");
					//			using (writer.Indent())
					//				writer.WriteLine($"throw new ArgumentOutOfRangeException($\"Invalid register {{value}}. Must be a {isName} register\", nameof(value));");
					//			writer.WriteLine("Value = value;");
					//			if (reg.NeedsState)
					//				writer.WriteLine("Flags = AssemblerOperandFlags.None;");
					//		}
					//		writer.WriteLine("}");
					//		writer.WriteLine();
					//		writer.WriteLine("/// <summary>");
					//		writer.WriteLine("/// The register value.");
					//		writer.WriteLine("/// </summary>");
					//		writer.WriteLine($"public readonly {registerTypeName} Value;");
					//		if (reg.NeedsState) {
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Creates a new instance.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"value\">A register</param>");
					//			writer.WriteLine("/// <param name=\"flags\">The mask</param>");
					//			writer.WriteLine($"public {className}({registerTypeName} value, AssemblerOperandFlags flags) {{");
					//			using (writer.Indent()) {
					//				writer.WriteLine("Value = value;");
					//				writer.WriteLine("Flags = flags;");
					//			}
					//			writer.WriteLine("}");
					//			writer.WriteLine();
					//			writer.WriteLine("internal readonly AssemblerOperandFlags Flags;");
					//			for (int j = 1; j <= 7; j++) {
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine($"/// Apply mask Register K{j}.");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} k{j} => new {className}(Value, (Flags & ~AssemblerOperandFlags.RegisterMask) | AssemblerOperandFlags.K{j});");
					//			}
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Apply mask Zeroing.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine($"public {className} z => new {className}(Value, Flags | AssemblerOperandFlags.Zeroing);");
					//			if (reg.HasSaeOrEr) {
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine("/// Suppress all exceptions");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} sae => new {className}(Value, Flags | AssemblerOperandFlags.SuppressAllExceptions);");
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine("/// Round to nearest (even)");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} rn_sae => new {className}(Value, (Flags & ~AssemblerOperandFlags.RoundControlMask) | AssemblerOperandFlags.RoundToNearest);");
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine("/// Round down (toward -inf)");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} rd_sae => new {className}(Value, (Flags & ~AssemblerOperandFlags.RoundControlMask) | AssemblerOperandFlags.RoundDown);");
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine("/// Round up (toward +inf)");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} ru_sae => new {className}(Value, (Flags & ~AssemblerOperandFlags.RoundControlMask) | AssemblerOperandFlags.RoundUp);");
					//				writer.WriteLine();
					//				writer.WriteLine("/// <summary>");
					//				writer.WriteLine("/// Round toward zero (truncate)");
					//				writer.WriteLine("/// </summary>");
					//				writer.WriteLine($"public {className} rz_sae => new {className}(Value, (Flags & ~AssemblerOperandFlags.RoundControlMask) | AssemblerOperandFlags.RoundTowardZero);");
					//			}
					//		}
					//		writer.WriteLine();
					//		writer.WriteLine("/// <summary>");
					//		writer.WriteLine($"/// Converts a <see cref=\"{className}\"/> to a <see cref=\"{registerTypeName}\"/>.");
					//		writer.WriteLine("/// </summary>");
					//		writer.WriteLine($"/// <param name=\"reg\">{className}</param>");
					//		writer.WriteLine("/// <returns></returns>");
					//		writer.WriteLine($"public static implicit operator {registerTypeName}({className} reg) => reg.Value;");
					//		if (reg.IsGPR16_32_64) {
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Adds a register (base) to another register (index) and return a memory operand.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"left\">The base register.</param>");
					//			writer.WriteLine("/// <param name=\"right\">The index register</param>");
					//			writer.WriteLine("/// <returns></returns>");
					//			writer.WriteLine($"public static AssemblerMemoryOperand operator +({className} left, {className} right) =>");
					//			using (writer.Indent())
					//				writer.WriteLine($"new AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, left, right, 1, 0, AssemblerOperandFlags.None);");
					//			if (reg.IsGPR32_64) {
					//				foreach (var mm in new[] { "XMM", "YMM", "ZMM" }) {
					//					writer.WriteLine();
					//					writer.WriteLine("/// <summary>");
					//					writer.WriteLine("/// Adds a register (base) to another register (index) and return a memory operand.");
					//					writer.WriteLine("/// </summary>");
					//					writer.WriteLine("/// <param name=\"left\">The base register.</param>");
					//					writer.WriteLine("/// <param name=\"right\">The index register</param>");
					//					writer.WriteLine("/// <returns></returns>");
					//					writer.WriteLine($"public static AssemblerMemoryOperand operator +({className} left, AssemblerRegister{mm} right) =>");
					//					using (writer.Indent())
					//						writer.WriteLine($"new AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, left, right, 1, 0, AssemblerOperandFlags.None);");
					//				}
					//			}
					//		}
					//		if (reg.IsGPR16_32_64 || reg.IsVector) {
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Adds a register (base) with a displacement and return a memory operand.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"left\">The base register</param>");
					//			writer.WriteLine("/// <param name=\"displacement\">The displacement</param>");
					//			writer.WriteLine("/// <returns></returns>");
					//			writer.WriteLine($"public static AssemblerMemoryOperand operator +({className} left, long displacement) =>");
					//			using (writer.Indent())
					//				writer.WriteLine($"new AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, left, {regNoneName}, 1, displacement, AssemblerOperandFlags.None);");
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Subtracts a register (base) with a displacement and return a memory operand.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"left\">The base register</param>");
					//			writer.WriteLine("/// <param name=\"displacement\">The displacement</param>");
					//			writer.WriteLine("/// <returns></returns>");
					//			writer.WriteLine($"public static AssemblerMemoryOperand operator -({className} left, long displacement) =>");
					//			using (writer.Indent())
					//				writer.WriteLine($"new AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, left, {regNoneName}, 1, -displacement, AssemblerOperandFlags.None);");
					//			writer.WriteLine();
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Multiplies an index register by a scale and return a memory operand.");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"left\">The base register</param>");
					//			writer.WriteLine("/// <param name=\"scale\">The scale</param>");
					//			writer.WriteLine("/// <returns></returns>");
					//			writer.WriteLine($"public static AssemblerMemoryOperand operator *({className} left, int scale) =>");
					//			using (writer.Indent())
					//				writer.WriteLine($"new AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, {regNoneName}, left, scale, 0, AssemblerOperandFlags.None);");
					//		}
					//		if (reg.NeedsState) {
					//			writer.WriteLine();
					//			writer.WriteLine("/// <inheritdoc />");
					//			writer.WriteLine($"public bool Equals({className} other) => Value == other.Value && Flags == other.Flags;");
					//			writer.WriteLine();
					//			writer.WriteLine("/// <inheritdoc />");
					//			writer.WriteLine("public override int GetHashCode() => ((int)Value * 397) ^ (int)Flags;");
					//		}
					//		else {
					//			writer.WriteLine();
					//			writer.WriteLine("/// <inheritdoc />");
					//			writer.WriteLine($"public bool Equals({className} other) => Value == other.Value;");
					//			writer.WriteLine();
					//			writer.WriteLine("/// <inheritdoc />");
					//			writer.WriteLine("public override int GetHashCode() => (int)Value;");
					//		}
					//		writer.WriteLine();
					//		writer.WriteLine("/// <inheritdoc />");
					//		writer.WriteLine($"public override bool Equals(object? obj) => obj is {className} other && Equals(other);");
					//		writer.WriteLine();
					//		writer.WriteLine("/// <summary>");
					//		writer.WriteLine($"/// Equality operator for <see cref=\"{className}\"/>");
					//		writer.WriteLine("/// </summary>");
					//		writer.WriteLine("/// <param name=\"left\">Register</param>");
					//		writer.WriteLine("/// <param name=\"right\">Register</param>");
					//		writer.WriteLine("/// <returns></returns>");
					//		writer.WriteLine($"public static bool operator ==({className} left, {className} right) => left.Equals(right);");
					//		writer.WriteLine();
					//		writer.WriteLine("/// <summary>");
					//		writer.WriteLine($"/// Inequality operator for <see cref=\"{className}\"/>");
					//		writer.WriteLine("/// </summary>");
					//		writer.WriteLine("/// <param name=\"left\">Register</param>");
					//		writer.WriteLine("/// <param name=\"right\">Register</param>");
					//		writer.WriteLine("/// <returns></returns>");
					//		writer.WriteLine($"public static bool operator !=({className} left, {className} right) => !left.Equals(right);");
					//	}
					//	writer.WriteLine("}");
					//	if (reg.IsGPR16_32_64) {
					//		writer.WriteLine();
					//		writer.WriteLine("public readonly partial struct AssemblerMemoryOperandFactory {");
					//		using (writer.Indent()) {
					//			writer.WriteLine("/// <summary>");
					//			writer.WriteLine("/// Specify a base register used with this memory operand (Base + Index * Scale + Displacement)");
					//			writer.WriteLine("/// </summary>");
					//			writer.WriteLine("/// <param name=\"register\">Size of this memory operand.</param>");
					//			writer.WriteLine($"public AssemblerMemoryOperand this[{className} register] => new AssemblerMemoryOperand(Size, Segment, register, {regNoneName}, 1, 0, Flags);");
					//		}
					//		writer.WriteLine("}");
					//	}
					//}
				}
				writer.WriteLine("}");
				writer.WriteLineNoIndent("#endif");
			}

			filename = CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "AssemblerRegister.g.h");
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(filename))) {
				var regNoneName = idConverter.ToDeclTypeAndValue(registerType[nameof(Register.None)]);
				var registerTypeName = registerType.Name(idConverter);
				var memOpNoneName = idConverter.ToDeclTypeAndValue(memoryOperandSizeType[nameof(MemoryOperandSize.None)]);

				writer.WriteFileHeader();
				writer.WriteLineNoIndent($"#if {CPlusPlusConstants.CodeAssemblerDefine}");
				writer.WriteLine("#include <cstdint>");
				writer.WriteLine("#include <stdexcept>");
				writer.WriteLine("#include \"../Register.g.h\"");
				writer.WriteLine("#include \"AssemblerMemoryOperand.h\"");
				writer.WriteLine();

				writer.WriteLine($"namespace {CPlusPlusConstants.IcedNamespace} {{");
				writer.WriteLine();

				using (writer.Indent()) {
					for (int i = 0; i < infos.Length; i++) {
						var reg = infos[i];
						var className = $"AssemblerRegister{GetRegisterSuffix(reg.Kind)}";
						var isName = reg.Kind switch {
							RegisterKind.None => throw new InvalidOperationException(),
							RegisterKind.GPR8 => "GPR8",
							RegisterKind.GPR16 => "GPR16",
							RegisterKind.GPR32 => "GPR32",
							RegisterKind.GPR64 => "GPR64",
							RegisterKind.IP => "IP",
							RegisterKind.Segment => "SegmentRegister",
							RegisterKind.ST => "ST",
							RegisterKind.CR => "CR",
							RegisterKind.DR => "DR",
							RegisterKind.TR => "TR",
							RegisterKind.BND => "BND",
							RegisterKind.K => "K",
							RegisterKind.MM => "MM",
							RegisterKind.XMM => "XMM",
							RegisterKind.YMM => "YMM",
							RegisterKind.ZMM => "ZMM",
							RegisterKind.TMM => "TMM",
							_ => throw new InvalidOperationException(),
						};

						if (i != 0)
							writer.WriteLine();

						writer.WriteLine($"constexpr {className}::{className}({registerTypeName} value) : Value(value){(reg.NeedsState ? ", Flags(AssemblerOperandFlags::None)" : "")} {{");
						using (writer.Indent()) {
							writer.WriteLine($"if (!RegisterExtensions::Is{isName}(value))");
							using (writer.Indent())
								writer.WriteLine($"throw std::invalid_argument(\"Invalid register value. Must be a {isName} register\");");
						}
						writer.WriteLine("}");
						writer.WriteLine();
						if (reg.NeedsState) {
							writer.WriteLine();
							writer.WriteLine($"constexpr {className}::{className}({registerTypeName} value, AssemblerOperandFlags flags) : Value(value), Flags(flags) {{");
							using (writer.Indent()) {
								writer.WriteLine($"if (!RegisterExtensions::Is{isName}(value))");
								using (writer.Indent())
									writer.WriteLine($"throw std::invalid_argument($\"Invalid register value. Must be a {isName} register\");");
							}
							writer.WriteLine("}");
							for (int j = 1; j <= 7; j++) {
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::k{j}() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, (Flags & ~AssemblerOperandFlags::RegisterMask) | AssemblerOperandFlags::K{j});");
								}
							}
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr {className} {className}::z() const {{");
							using (writer.Indent()) {
								writer.WriteLine($"return {className}(Value, Flags | AssemblerOperandFlags::Zeroing);");
							}
							writer.WriteLine("}");
							writer.WriteLine();
							if (reg.HasSaeOrEr) {
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::sae() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, Flags | AssemblerOperandFlags::SuppressAllExceptions);");
								}
								writer.WriteLine("}");
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::rn_sae() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, (Flags & ~AssemblerOperandFlags::RoundControlMask) | AssemblerOperandFlags::RoundToNearest);");
								}
								writer.WriteLine("}");
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::rd_sae() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, (Flags & ~AssemblerOperandFlags::RoundControlMask) | AssemblerOperandFlags::RoundDown);");
								}
								writer.WriteLine("}");
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::ru_sae() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, (Flags & ~AssemblerOperandFlags::RoundControlMask) | AssemblerOperandFlags::RoundUp);");
								}
								writer.WriteLine("}");
								writer.WriteLine();
								writer.WriteLine($"constexpr {className} {className}::rz_sae() const {{");
								using (writer.Indent()) {
									writer.WriteLine($"return {className}(Value, (Flags & ~AssemblerOperandFlags::RoundControlMask) | AssemblerOperandFlags::RoundTowardZero);");
								}
								writer.WriteLine("}");
							}
						}
						writer.WriteLine();
						writer.WriteLine($"constexpr {className}::operator {registerTypeName}() const {{");
						using (writer.Indent()) {
							writer.WriteLine($"return Value;");
						}
						writer.WriteLine("}");
						if (reg.IsGPR16_32_64) {
							writer.WriteLine();
							writer.WriteLine($"constexpr AssemblerMemoryOperand {className}::operator +(const {className}& right) const {{");
							using (writer.Indent()) {
								writer.WriteLine($"return AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, *this, right, 1, 0, AssemblerOperandFlags::None);");
							}
							writer.WriteLine("}");
							if (reg.IsGPR32_64) {
								foreach (var mm in new[] { "XMM", "YMM", "ZMM" }) {
									writer.WriteLine();
									writer.WriteLine($"constexpr AssemblerMemoryOperand {className}::operator +(const AssemblerRegister{mm}& right) const {{");
									using (writer.Indent()) {
										writer.WriteLine($"return AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, *this, right, 1, 0, AssemblerOperandFlags::None);");
									}
									writer.WriteLine("}");
								}
							}
						}
						if (reg.IsGPR16_32_64 || reg.IsVector) {
							writer.WriteLine();
							writer.WriteLine($"constexpr AssemblerMemoryOperand {className}::operator +(std::int64_t displacement) const {{");
							using (writer.Indent())
								writer.WriteLine($"return AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, *this, {regNoneName}, 1, displacement, AssemblerOperandFlags::None);");
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr AssemblerMemoryOperand {className}::operator -(std::int64_t displacement) const {{");
							using (writer.Indent())
								writer.WriteLine($"return AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, *this, {regNoneName}, 1, -displacement, AssemblerOperandFlags::None);");
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr AssemblerMemoryOperand {className}::operator *(std::int32_t scale) const {{");
							using (writer.Indent())
								writer.WriteLine($"return AssemblerMemoryOperand({memOpNoneName}, {regNoneName}, {regNoneName}, *this, scale, 0, AssemblerOperandFlags::None);");
							writer.WriteLine("}");
						}
						if (reg.NeedsState) {
							writer.WriteLine();
							writer.WriteLine("/// <inheritdoc />");
							writer.WriteLine($"constexpr int {className}::GetHashCode() const {{");
							using (writer.Indent()) {
								writer.WriteLine("return ((int)Value * 397) ^ (int)Flags;");
							}
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr bool {className}:: ==(const {className}& right) const {{");
							using (writer.Indent()) {
								writer.WriteLine("return Value == right.Value && Flags == right.Flags;");
							}
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr bool {className}::operator !=(const {className}& right) const {{");
							using (writer.Indent()) {
								writer.WriteLine("return Value != right.Value || Flags != right.Flags;");
							}
							writer.WriteLine("}");
						}
						else {
							writer.WriteLine();
							writer.WriteLine("/// <inheritdoc />");
							writer.WriteLine($"constexpr int {className}::GetHashCode() const {{");
							using (writer.Indent()) {
								writer.WriteLine("return Value;");
							}
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr bool {className}::operator ==(const {className}& right) const {{");
							using (writer.Indent()) {
								writer.WriteLine("return Value == right.Value;");
							}
							writer.WriteLine("}");
							writer.WriteLine();
							writer.WriteLine($"constexpr bool {className}::operator !=(const {className}& right) const {{");
							using (writer.Indent()) {
								writer.WriteLine("return Value != right.Value;");
							}
							writer.WriteLine("}");
						}
						writer.WriteLine();
						writer.WriteLine($"constexpr bool {className}::operator ==(const {registerTypeName}& right) const {{");
						using (writer.Indent()) {
							writer.WriteLine("return Value == right;");
						}
						writer.WriteLine("}");
						writer.WriteLine();
						if (reg.IsGPR16_32_64) {
							writer.WriteLine("/// <summary>");
							writer.WriteLine($"/// Adds a {reg.Kind switch { RegisterKind.GPR16 => "16", RegisterKind.GPR32 => "32", RegisterKind.GPR64 => "64", _ => throw new InvalidOperationException() }}-bit memory operand with an new base or index.");
							writer.WriteLine("/// </summary>");
							writer.WriteLine("/// <param name=\"left\">The base or index.</param>");
							writer.WriteLine("/// <param name=\"right\">The memory operand.</param>");
							writer.WriteLine("/// <returns></returns>");
							writer.WriteLine($"constexpr AssemblerMemoryOperand operator + ({className} left, AssemblerMemoryOperand right) {{");
							using (writer.Indent()) {
								writer.WriteLine($"auto hasBase = right.Base != {regNoneName};");
								writer.WriteLine($"return AssemblerMemoryOperand(right.Size, {regNoneName}, hasBase ? right.Base : left.Value, hasBase ? left.Value : right.Index, right.Scale, right.Displacement, right.Flags);");
							}
							writer.WriteLine("}");
						}
					}
				}
				writer.WriteLine("}");
				writer.WriteLineNoIndent("#endif");
			}
		}

		static string GetName(MemorySizeFuncInfo fn) {
			var name = fn.Kind switch {
				MemorySizeFnKind.Ptr => string.Empty,
				_ => fn.Name.Replace(' ', '_'),
			};
			return "__" + name;
		}

		protected override void GenerateMemorySizeFunctions(MemorySizeFuncInfo[] infos) {
			var filename = CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "AssemblerRegisters2.g.h");
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(filename))) {
				writer.WriteFileHeader();
				writer.WriteLineNoIndent($"#if {CPlusPlusConstants.CodeAssemblerDefine}");
				writer.WriteLine("#include \"AssemblerMemoryOperandFactory.h\"");
				writer.WriteLine("#include \"../Registers.h\"");

				var regNoneStr = idConverter.ToDeclTypeAndValue(registerType[nameof(Register.None)]);

				writer.WriteLine($"namespace {CPlusPlusConstants.IcedNamespace} {{");
				using (writer.Indent()) {
					writer.WriteLine("/// <summary>");
					writer.WriteLine("/// Registers used for <see cref=\"Assembler\"/>. ");
					writer.WriteLine("/// </summary>");
					writer.WriteLine("namespace AssemblerRegisters {");
					using (writer.Indent()) {
						for (int i = 0; i < infos.Length; i++) {
							var info = infos[i];
							var doc = info.GetMethodDocs("Gets", s => $"<c>{s}</c>");
							var name = GetName(info);

							if (i != 0)
								writer.WriteLine();
							writer.WriteLine("/// <summary>");
							writer.WriteLine($"/// {doc}");
							writer.WriteLine("/// </summary>");
							var enumValueStr = idConverter.ToDeclTypeAndValue(memoryOperandSizeType[info.Size.ToString()]);
							if (info.IsBroadcast)
								writer.WriteLine($"inline constexpr AssemblerMemoryOperandFactory {name} = AssemblerMemoryOperandFactory({enumValueStr}, {regNoneStr}, AssemblerOperandFlags::Broadcast);");
							else
								writer.WriteLine($"inline constexpr AssemblerMemoryOperandFactory {name} = AssemblerMemoryOperandFactory({enumValueStr});");
						}
					}
					writer.WriteLine("}");
				}
				writer.WriteLine("}");
				writer.WriteLineNoIndent("#endif");
			}
		}

		protected override void Generate(Dictionary<GroupKey, OpCodeInfoGroup> map, OpCodeInfoGroup[] groups) {
			GenerateCode(groups);
		}

		void GenerateCode(OpCodeInfoGroup[] groups) {
			var filename = CPlusPlusConstants.GetFilename(genTypes, CPlusPlusConstants.IcedNamespace, "Assembler", "Assembler.g.h");
			using (var writer = new FileWriter(TargetLanguage.CPlusPlus, FileUtils.OpenWrite(filename))) {
				//writer.WriteFileHeader();
				//writer.WriteLineNoIndent($"#if {CPlusPlusConstants.CodeAssemblerDefine}");
				//writer.WriteLine($"namespace {CPlusPlusConstants.IcedNamespace} {{");
				//using (writer.Indent()) {
				//	writer.WriteLine("public partial class Assembler {");
				//	using (writer.Indent()) {
						bool methodSep = false;
						foreach (var group in groups) {
							if (methodSep)
								writer.WriteLine();
							methodSep = true;
							var renderArgs = GetRenderArgs(group);
							var methodName = idConverter.Method(group.Name);
							GenerateAssemblerCode(writer, methodName, group, renderArgs);
						}

						GenerateDeclareDataCode(writer);
				//	}
				//	writer.WriteLine("}");
				//}
				//writer.WriteLine("}");
				//writer.WriteLineNoIndent("#endif");
			}
		}

		void GenerateDeclareDataCode(FileWriter writer) {
			foreach (var (name, size, types, methodName) in declareDataList) {
				int maxSize = 16;
				int argCount = maxSize / size;

				for (var typeIndex = 0; typeIndex < types.Length; typeIndex++) {
					var type = types[typeIndex];
					bool isUnsafe = type == "float" || type == "double";
					for (int i = 1; i <= argCount; i++) {
						writer.WriteLine();
						docWriter.WriteSummary(writer, $"Creates a {name} asm directive with the type {type}.", "");
						writer.Write($"constexpr void {name}(");
						for (int j = 0; j < i; j++) {
							if (j > 0) writer.Write(", ");
							writer.Write($"{type} imm{j}");
						}

						writer.WriteLine(") {");
						using (writer.Indent()) {
							writer.Write($"AddInstruction(Instruction::{methodName}(");
							for (int j = 0; j < i; j++) {
								if (j > 0) writer.Write(", ");
								if (typeIndex == 0)
									writer.Write($"imm{j}");
								else
									writer.Write(isUnsafe ? $"*({types[0]}*)&imm{j}" : $"({types[0]})imm{j}");
							}

							writer.WriteLine("));");
						}

						writer.WriteLine("}");
					}
				}
			}
		}

		static RenderArg[] GetRenderArgs(OpCodeInfoGroup group) {
			var renderArgs = new RenderArg[group.Signature.ArgCount];
			int immArg = 0;
			var signature = group.Signature;
			for (int i = 0; i < signature.ArgCount; i++) {
				string argName = i == 0 ? "dst" : "src";
				int maxArgSize = group.MaxArgSizes[i];
				var argKind = signature.GetArgKind(i);

				if (signature.ArgCount > 2 && i >= 1)
					argName = $"src{i}";

				string argType;
				switch (argKind) {
				case ArgKind.Register8:
					argType = "AssemblerRegister8";
					break;
				case ArgKind.Register16:
					argType = "AssemblerRegister16";
					break;
				case ArgKind.Register32:
					argType = "AssemblerRegister32";
					break;
				case ArgKind.Register64:
					argType = "AssemblerRegister64";
					break;
				case ArgKind.RegisterMm:
					argType = "AssemblerRegisterMM";
					break;
				case ArgKind.RegisterXmm:
					argType = "AssemblerRegisterXMM";
					break;
				case ArgKind.RegisterYmm:
					argType = "AssemblerRegisterYMM";
					break;
				case ArgKind.RegisterZmm:
					argType = "AssemblerRegisterZMM";
					break;
				case ArgKind.RegisterTmm:
					argType = "AssemblerRegisterTMM";
					break;
				case ArgKind.RegisterK:
					argType = "AssemblerRegisterK";
					break;
				case ArgKind.RegisterCr:
					argType = "AssemblerRegisterCR";
					break;
				case ArgKind.RegisterTr:
					argType = "AssemblerRegisterTR";
					break;
				case ArgKind.RegisterDr:
					argType = "AssemblerRegisterDR";
					break;
				case ArgKind.RegisterSt:
					argType = "AssemblerRegisterST";
					break;
				case ArgKind.RegisterBnd:
					argType = "AssemblerRegisterBND";
					break;
				case ArgKind.RegisterSegment:
					argType = "AssemblerRegisterSegment";
					break;

				case ArgKind.Label:
					argType = "Label";
					break;

				case ArgKind.LabelU64:
					argType = "ulong";
					break;

				case ArgKind.Memory:
					argType = "AssemblerMemoryOperand";
					break;

				case ArgKind.Immediate:
				case ArgKind.ImmediateUnsigned:
					argName = $"imm{(immArg == 0 ? "" : immArg.ToString(CultureInfo.InvariantCulture))}";
					immArg++;
					if (argKind == ArgKind.Immediate) {
						argType = maxArgSize switch {
							1 => "sbyte",
							2 => "short",
							4 => "int",
							8 => "long",
							_ => throw new InvalidOperationException(),
						};
					}
					else {
						argType = maxArgSize switch {
							1 => "byte",
							2 => "ushort",
							4 => "uint",
							8 => "ulong",
							_ => throw new InvalidOperationException(),
						};
					}
					break;

				default:
					throw new ArgumentOutOfRangeException($"{argKind}");
				}

				renderArgs[i] = new RenderArg(argName, argType, argKind);
			}
			return renderArgs;
		}

		void GenerateAssemblerCode(FileWriter writer, string methodName, OpCodeInfoGroup group, RenderArg[] renderArgs) {
			// Write documentation
			var methodDoc = new StringBuilder();
			methodDoc.Append($"{group.Name} instruction.");
			foreach (var def in group.GetDefsAndParentDefs()) {
				if (def.Code.Documentation.GetComment(TargetLanguage.CPlusPlus) is string comment) {
					methodDoc.Append("#(p:)#");
					methodDoc.Append(comment);
				}
			}

			docWriter.WriteSummary(writer, methodDoc.ToString(), "");

			writer.Write($"constexpr void {methodName}(");
			for (var i = 0; i < renderArgs.Length; i++) {
				var renderArg = renderArgs[i];
				if (i > 0)
					writer.Write(", ");
				writer.Write($"{renderArg.Type} {renderArg.Name}");
			}

			static void WriteArg(FileWriter writer, string argExpr, ArgKind kind) {
				writer.Write(argExpr);
				if (kind == ArgKind.Label)
					writer.Write(".Id");
				else if (kind == ArgKind.Memory)
					writer.Write(".ToMemoryOperand(Bitness)");
			}

			writer.WriteLine(") {");
			using (writer.Indent()) {
				if ((group.Flags & OpCodeArgFlags.HasSpecialInstructionEncoding) != 0) {
					writer.Write($"AddInstruction(Instruction::Create{group.MnemonicName}(Bitness");
					for (var i = 0; i < renderArgs.Length; i++) {
						var renderArg = renderArgs[i];
						writer.Write(", ");
						WriteArg(writer, renderArg.Name, renderArg.Kind);
					}
					writer.WriteLine("));");
				}
				else if (group.ParentPseudoOpsKind is not null) {
					writer.Write($"{group.ParentPseudoOpsKind.Name}(");
					for (var i = 0; i < renderArgs.Length; i++) {
						var renderArg = renderArgs[i];
						if (i > 0)
							writer.Write(", ");
						writer.Write(renderArg.Name);
					}
					writer.Write(", ");
					writer.Write($"{group.PseudoOpsKindImmediateValue}");
					writer.WriteLine(");");
				}
				else {
					string codeExpr;
					if (group.RootOpCodeNode.Def is InstructionDef def)
						codeExpr = idConverter.ToDeclTypeAndValue(def.Code);
					else {
						codeExpr = "code";
						writer.WriteLine($"Code {codeExpr};");
						GenerateOpCodeSelector(writer, group, renderArgs);
					}

					if (group.HasLabel)
						writer.Write("AddInstruction(Instruction::CreateBranch(");
					else
						writer.Write("AddInstruction(Instruction::Create(");
					writer.Write(codeExpr);

					for (var i = 0; i < renderArgs.Length; i++) {
						var renderArg = renderArgs[i];
						writer.Write(", ");

						var argExpr = renderArg.Name;

						// Perform casting for unsigned
						if (renderArg.Kind == ArgKind.ImmediateUnsigned) {
							if (renderArg.Type != "std::uint64_t" && renderArg.Type != "std::uint32_t")
								argExpr = $"(std::uint32_t){argExpr}";
						}

						WriteArg(writer, argExpr, renderArg.Kind);
					}

					writer.Write(")");

					var stateArgsList = new List<string>();
					foreach (var index in GetStateArgIndexes(group))
						stateArgsList.Add($"{renderArgs[index].Name}.Flags");
					if (stateArgsList.Count > 0) {
						var s = string.Join(" | ", stateArgsList);
						writer.Write(", " + s);
					}

					writer.WriteLine(");");
				}
			}

			writer.WriteLine("}");
		}

		void RenderTests(int bitness, FileWriter writer, OpCodeInfoGroup group, RenderArg[] renderArgs) {
			var fullMethodName = new StringBuilder();
			fullMethodName.Append(idConverter.Method(group.Name));
			foreach (var renderArg in renderArgs) {
				fullMethodName.Append('_');
				fullMethodName.Append(GetTestMethodArgName(renderArg.Kind));
			}

			var fullMethodNameStr = fullMethodName.ToString();
			if (ignoredTestsPerBitness.TryGetValue(bitness, out var ignoredTests) && ignoredTests.Contains(fullMethodNameStr))
				return;
			writer.WriteLine("[Fact]");
			writer.WriteLine($"public void {fullMethodNameStr}() {{");
			using (writer.Indent()) {
				var args = new TestArgValues(renderArgs.Length);

				if (group.ParentPseudoOpsKind is not null)
					GenerateTestAssemblerForOpCode(writer, bitness, group, args, OpCodeArgFlags.None, group.ParentPseudoOpsKind.Defs[0]);
				else
					GenerateOpCodeTest(writer, bitness, group, group.RootOpCodeNode, renderArgs, args, OpCodeArgFlags.None);
			}
			writer.WriteLine("}");
			writer.WriteLine();
		}

		void GenerateOpCodeTest(FileWriter writer, int bitness, OpCodeInfoGroup group, OpCodeNode node, RenderArg[] renderArgs,
			TestArgValues args, OpCodeArgFlags contextFlags) {
			if (node.Def is InstructionDef def)
				GenerateTestAssemblerForOpCode(writer, bitness, group, args, contextFlags, def);
			else if (node.Selector is OpCodeSelector selector) {
				var maxArgSize = selector.ArgIndex >= 0 ? group.MaxArgSizes[selector.ArgIndex] : 0;
				var argKind = selector.ArgIndex >= 0 ? renderArgs[selector.ArgIndex] : default;
				var condition = GetArgConditionForOpCodeKind(argKind, selector.Kind);
				var isSelectorSupportedByBitness = IsSelectorSupportedByBitness(bitness, selector.Kind, out var continueElse);
				var (contextIfFlags, contextElseFlags) = GetIfElseContextFlags(selector.Kind);
				if (isSelectorSupportedByBitness) {
					writer.WriteLine($"{{ /* if ({condition}) */");
					using (writer.Indent()) {
						foreach (var argValue in GetArgValue(selector.Kind, false, selector.ArgIndex, group.Signature, maxArgSize * 8)) {
							var oldValue = args.Set(selector.ArgIndex, argValue);
							GenerateOpCodeTest(writer, bitness, group, selector.IfTrue, renderArgs, args, contextFlags | contextIfFlags);
							args.Restore(selector.ArgIndex, oldValue);
						}
					}
				}
				else
					writer.WriteLine($"{{ // skip ({condition}) not supported by this Assembler bitness");

				if (!selector.IfFalse.IsEmpty) {
					if (continueElse) {
						writer.Write("} /* else */ ");
						foreach (var argValue in GetArgValue(selector.Kind, true, selector.ArgIndex, group.Signature, maxArgSize * 8)) {
							var oldValue = args.Set(selector.ArgIndex, argValue);
							GenerateOpCodeTest(writer, bitness, group, selector.IfFalse, renderArgs, args, contextFlags | contextElseFlags);
							args.Restore(selector.ArgIndex, oldValue);
						}
					}
					else
						writer.WriteLine($"}} /* else skip !({condition}) not supported by this Assembler bitness */");
				}
				else {
					writer.WriteLine("}");
					if (isSelectorSupportedByBitness && selector.ArgIndex >= 0) {
						var newArg = GetInvalidArgValue(selector.Kind, selector.ArgIndex);
						if (newArg is not null) {
							writer.WriteLine("{");
							using (writer.Indent()) {
								int testBitness = GetInvalidTestBitness(bitness, group);
								writer.WriteLine("AssertInvalid(() => {");
								using (writer.Indent()) {
									var oldValue = args.Set(selector.ArgIndex, newArg);
									GenerateOpCodeTest(writer, testBitness, group, selector.IfTrue, renderArgs, args, contextFlags | contextIfFlags);
									args.Restore(selector.ArgIndex, oldValue);
								}
								writer.WriteLine("});");
							}
							writer.WriteLine("}");
						}
					}
				}
			}
			else
				throw new InvalidOperationException();
		}

		bool GenerateTestAssemblerForOpCode(FileWriter writer, int bitness, OpCodeInfoGroup group, TestArgValues args,
			OpCodeArgFlags contextFlags, InstructionDef def) {
			if (!IsBitnessSupported(bitness, def.Flags1)) {
				writer.WriteLine($"// Skipping {def.Code.Name(idConverter)} - Not supported by current bitness");
				return false;
			}

			var withFns = new List<(string pre, string post)>();
			var asmArgs = new List<string>();
			var withArgs = new List<string>();
			int argBitness = GetArgBitness(bitness, def);
			if ((group.Flags & OpCodeArgFlags.HasSpecialInstructionEncoding) != 0)
				withArgs.Add(bitness.ToString(CultureInfo.InvariantCulture));
			else
				withArgs.Add(idConverter.ToDeclTypeAndValue(def.Code));
			for (var i = 0; i < args.Args.Count; i++) {
				var argKind = group.Signature.GetArgKind(i);
				var asmArg = args.GetArgValue(argBitness, i)?.AsmStr;
				var withArg = args.GetArgValue(argBitness, i)?.WithStr;

				if (asmArg is null || withArg is null) {
					var argValue = GetDefaultArgument(def.OpKindDefs[group.NumberOfLeadingArgsToDiscard + i], i, argKind, group.MaxArgSizes[i] * 8);
					asmArg = argValue.Get(argBitness).AsmStr;
					withArg = argValue.Get(argBitness).WithStr;
				}

				if ((def.Flags1 & InstructionDefFlags1.OpMaskRegister) != 0 && i == 0) {
					asmArg += ".k1";
					var opMask = idConverter.ToDeclTypeAndValue(GetRegisterDef(Register.K1).Register);
					withFns.Add(("ApplyK(", $", {opMask})"));
				}

				asmArgs.Add(asmArg);
				withArgs.Add(withArg);
			}
			if (group.ParentPseudoOpsKind is not null)
				withArgs.Add($"{group.PseudoOpsKindImmediateValue}");
			if (group.HasLabel && (group.Flags & OpCodeArgFlags.HasLabelUlong) == 0)
				withFns.Add(("AssignLabel(", $", {withArgs[1]})"));

			string withFnName;
			if ((group.Flags & OpCodeArgFlags.HasSpecialInstructionEncoding) != 0)
				withFnName = $"Create{group.MnemonicName}";
			else if (group.HasLabel)
				withFnName = "CreateBranch";
			else
				withFnName = "Create";
			var asmName = idConverter.Method(group.Name);
			var asmArgsStr = string.Join(", ", asmArgs);
			var instrFlags = GetInstrTestFlags(def, group, contextFlags);
			var testInstrFlagsStr = instrFlags.Count > 0 ?
				$", {string.Join(" | ", instrFlags.Select(x => idConverter.ToDeclTypeAndValue(x)))}" : string.Empty;
			var decoderOptions = GetDecoderOptions(bitness, def);
			string decoderOptionsStr;
			if (decoderOptions.Count != 0) {
				var options = string.Join(" | ", decoderOptions.Select(x => idConverter.ToDeclTypeAndValue(x)));
				decoderOptionsStr = $", decoderOptions: {options}";
			}
			else
				decoderOptionsStr = string.Empty;

			var withArgsStr = string.Join(", ", withArgs);
			var withFnsPreStr = string.Join(string.Empty, ((IEnumerable<(string pre, string post)>)withFns).Reverse().Select(x => x.pre));
			var withFnsPostStr = string.Join(string.Empty, withFns.Select(x => x.post));
			writer.WriteLine($"TestAssembler(c => c.{asmName}({asmArgsStr}), {withFnsPreStr}Instruction.{withFnName}({withArgsStr}){withFnsPostStr}{testInstrFlagsStr}{decoderOptionsStr});");
			return true;
		}

		void GenerateOpCodeSelector(FileWriter writer, OpCodeInfoGroup group, RenderArg[] args) =>
			GenerateOpCodeSelector(writer, group, true, group.RootOpCodeNode, args);

		void GenerateOpCodeSelector(FileWriter writer, OpCodeInfoGroup group, bool isLeaf, OpCodeNode node, RenderArg[] args) {
			if (node.Def is InstructionDef def) {
				if (isLeaf)
					writer.Write("code = ");
				writer.Write($"Code.{def.Code.Name(idConverter)}");
				if (isLeaf)
					writer.WriteLine(";");
			}
			else if (node.Selector is OpCodeSelector selector) {
				var condition = GetArgConditionForOpCodeKind(selector.ArgIndex >= 0 ? args[selector.ArgIndex] : default, selector.Kind);
				if (selector.IsConditionInlineable) {
					writer.Write($"code = {condition} ? ");
					GenerateOpCodeSelector(writer, group, false, selector.IfTrue, args);
					writer.Write(" : ");
					GenerateOpCodeSelector(writer, group, false, selector.IfFalse, args);
					writer.WriteLine(";");
				}
				else {
					writer.WriteLine($"if ({condition}) {{");
					using (writer.Indent())
						GenerateOpCodeSelector(writer, group, true, selector.IfTrue, args);

					writer.Write("} else ");
					if (!selector.IfFalse.IsEmpty)
						GenerateOpCodeSelector(writer, group, true, selector.IfFalse, args);
					else {
						writer.WriteLine("{");
						using (writer.Indent()) {
							writer.Write($"throw NoOpCodeFoundFor(Mnemonic.{group.MnemonicName}");
							for (var i = 0; i < args.Length; i++) {
								var renderArg = args[i];
								writer.Write(", ");
								writer.Write(renderArg.Name);
							}

							writer.WriteLine(");");
						}

						writer.WriteLine("}");
					}
				}
			}
			else
				throw new InvalidOperationException();
		}

		string GetArgConditionForOpCodeKind(RenderArg arg, OpCodeSelectorKind selectorKind) {
			var argName = arg.Name;
			var otherArgName = arg.Name == "src" ? "dst" : "src";
			return selectorKind switch {
				OpCodeSelectorKind.MemOffs64_RAX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.RAX))} && Bitness == 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs64_EAX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.EAX))} && Bitness == 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs64_AX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.AX))} && Bitness == 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs64_AL => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.AL))} && Bitness == 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs_RAX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.RAX))} && Bitness < 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs_EAX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.EAX))} && Bitness < 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs_AX => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.AX))} && Bitness < 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.MemOffs_AL => $"{otherArgName}.Value == {GetRegisterString(nameof(Register.AL))} && Bitness < 64 && {argName}.IsDisplacementOnly",
				OpCodeSelectorKind.Bitness64 => "Bitness == 64",
				OpCodeSelectorKind.Bitness32 => "Bitness >= 32",
				OpCodeSelectorKind.Bitness16 => "Bitness >= 16",
				OpCodeSelectorKind.ShortBranch => "PreferShortBranch",
				OpCodeSelectorKind.ImmediateByteEqual1 => $"{argName} == 1",
				OpCodeSelectorKind.ImmediateByteSigned8To32 or OpCodeSelectorKind.ImmediateByteSigned8To64 => arg.Kind == ArgKind.ImmediateUnsigned ?
					$"{argName} <= ({arg.Type})sbyte.MaxValue || 0xFFFF_FF80 <= {argName}" :
					$"{argName} >= sbyte.MinValue && {argName} <= sbyte.MaxValue",
				OpCodeSelectorKind.ImmediateByteSigned8To16 => arg.Kind == ArgKind.ImmediateUnsigned ?
					$"{argName} <= ({arg.Type})sbyte.MaxValue || (0xFF80 <= {argName} && {argName} <= 0xFFFF)" :
					$"{argName} >= sbyte.MinValue && {argName} <= sbyte.MaxValue",
				OpCodeSelectorKind.Vex => "InstructionPreferVex",
				OpCodeSelectorKind.EvexBroadcastX or OpCodeSelectorKind.EvexBroadcastY or OpCodeSelectorKind.EvexBroadcastZ =>
					$"{argName}.IsBroadcast",
				OpCodeSelectorKind.RegisterCL => $"{argName} == {GetRegisterString(nameof(Register.CL))}",
				OpCodeSelectorKind.RegisterAL => $"{argName} == {GetRegisterString(nameof(Register.AL))}",
				OpCodeSelectorKind.RegisterAX => $"{argName} == {GetRegisterString(nameof(Register.AX))}",
				OpCodeSelectorKind.RegisterEAX => $"{argName} == {GetRegisterString(nameof(Register.EAX))}",
				OpCodeSelectorKind.RegisterRAX => $"{argName} == {GetRegisterString(nameof(Register.RAX))}",
				OpCodeSelectorKind.RegisterBND => $"{argName}.IsBND()",
				OpCodeSelectorKind.RegisterES => $"{argName} == {GetRegisterString(nameof(Register.ES))}",
				OpCodeSelectorKind.RegisterCS => $"{argName} == {GetRegisterString(nameof(Register.CS))}",
				OpCodeSelectorKind.RegisterSS => $"{argName} == {GetRegisterString(nameof(Register.SS))}",
				OpCodeSelectorKind.RegisterDS => $"{argName} == {GetRegisterString(nameof(Register.DS))}",
				OpCodeSelectorKind.RegisterFS => $"{argName} == {GetRegisterString(nameof(Register.FS))}",
				OpCodeSelectorKind.RegisterGS => $"{argName} == {GetRegisterString(nameof(Register.GS))}",
				OpCodeSelectorKind.RegisterDX => $"{argName} == {GetRegisterString(nameof(Register.DX))}",
				OpCodeSelectorKind.Register8 => $"{argName}.IsGPR8()",
				OpCodeSelectorKind.Register16 => $"{argName}.IsGPR16()",
				OpCodeSelectorKind.Register32 => $"{argName}.IsGPR32()",
				OpCodeSelectorKind.Register64 => $"{argName}.IsGPR64()",
				OpCodeSelectorKind.RegisterK => $"{argName}.IsK()",
				OpCodeSelectorKind.RegisterST0 => $"{argName} == {GetRegisterString(nameof(Register.ST0))}",
				OpCodeSelectorKind.RegisterST => $"{argName}.IsST()",
				OpCodeSelectorKind.RegisterSegment => $"{argName}.IsSegmentRegister()",
				OpCodeSelectorKind.RegisterCR => $"{argName}.IsCR()",
				OpCodeSelectorKind.RegisterDR => $"{argName}.IsDR()",
				OpCodeSelectorKind.RegisterTR => $"{argName}.IsTR()",
				OpCodeSelectorKind.RegisterMM => $"{argName}.IsMM()",
				OpCodeSelectorKind.RegisterXMM => $"{argName}.IsXMM()",
				OpCodeSelectorKind.RegisterYMM => $"{argName}.IsYMM()",
				OpCodeSelectorKind.RegisterZMM => $"{argName}.IsZMM()",
				OpCodeSelectorKind.RegisterTMM => $"{argName}.IsTMM()",
				OpCodeSelectorKind.Memory8 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Byte))}",
				OpCodeSelectorKind.Memory16 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Word))}",
				OpCodeSelectorKind.Memory32 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Dword))}",
				OpCodeSelectorKind.Memory48 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Fword))}",
				OpCodeSelectorKind.Memory64 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Qword))}",
				OpCodeSelectorKind.Memory80 => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Tbyte))}",
				OpCodeSelectorKind.MemoryMM => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Qword))}",
				OpCodeSelectorKind.MemoryXMM => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Xword))}",
				OpCodeSelectorKind.MemoryYMM => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Yword))}",
				OpCodeSelectorKind.MemoryZMM => $"{argName}.Size == {GetMemOpSizeString(nameof(MemoryOperandSize.Zword))}",
				OpCodeSelectorKind.MemoryIndex32Xmm or OpCodeSelectorKind.MemoryIndex64Xmm => $"{argName}.Index.IsXMM()",
				OpCodeSelectorKind.MemoryIndex32Ymm or OpCodeSelectorKind.MemoryIndex64Ymm => $"{argName}.Index.IsYMM()",
				OpCodeSelectorKind.MemoryIndex32Zmm or OpCodeSelectorKind.MemoryIndex64Zmm => $"{argName}.Index.IsZMM()",
				_ => throw new InvalidOperationException(),
			};
		}

		string GetRegisterString(string fieldName) =>
			idConverter.ToDeclTypeAndValue(registerType[fieldName]);

		string GetMemOpSizeString(string fieldName) =>
			idConverter.ToDeclTypeAndValue(memoryOperandSizeType[fieldName]);

		protected override TestArgValueBitness MemToTestArgValue(MemorySizeFuncInfo size, int bitness, ulong address) {
			var memName = GetName(size);
			var asmStr = $"{memName}[0x{address:X}]";
			var withStr = $"new MemoryOperand(0x{address:X}, {bitness / 8})";
			return new TestArgValueBitness(asmStr, withStr);
		}

		protected override TestArgValueBitness MemToTestArgValue(MemorySizeFuncInfo size, Register @base, Register index, int scale, int displ) {
			if (scale != 1 && scale != 2 && scale != 4 && scale != 8)
				throw new InvalidOperationException();
			var sb = new StringBuilder();
			sb.Append(GetName(size));
			sb.Append('[');
			bool plus = false;
			if (@base != Register.None) {
				plus = true;
				sb.Append(GetAsmRegisterName(GetRegisterDef(@base)));
			}
			if (index != Register.None) {
				if (plus)
					sb.Append('+');
				plus = true;
				sb.Append(GetAsmRegisterName(GetRegisterDef(index)));
				if (scale > 1) {
					sb.Append('*');
					sb.Append(scale);
				}
			}
			if (displ != 0) {
				bool isNeg = displ < 0;
				if (isNeg)
					displ = -displ;
				if (plus)
					sb.Append(isNeg ? '-' : '+');
				sb.Append("0x");
				sb.Append(displ.ToString("X", CultureInfo.InvariantCulture));
			}
			sb.Append(']');
			var asmStr = sb.ToString();

			var baseStr = idConverter.ToDeclTypeAndValue(GetRegisterDef(@base).Register);
			var indexStr = idConverter.ToDeclTypeAndValue(GetRegisterDef(index).Register);
			var displStr = displ < 0 ?
				"-0x" + (-displ).ToString("X", CultureInfo.InvariantCulture) :
				"0x" + displ.ToString("X", CultureInfo.InvariantCulture);
			var displSize = displ == 0 ? "0" : "1";
			var isBcstStr = size.IsBroadcast ? "true" : "false";
			var regNoneStr = idConverter.ToDeclTypeAndValue(GetRegisterDef(Register.None).Register);
			var withStr = $"new MemoryOperand({baseStr}, {indexStr}, {scale}, {displStr}, {displSize}, {isBcstStr}, {regNoneStr})";

			return new(asmStr, withStr);
		}

		protected override TestArgValueBitness RegToTestArgValue(Register register) {
			var regDef = GetRegisterDef(register);
			var asmReg = GetAsmRegisterName(regDef);
			var withReg = idConverter.ToDeclTypeAndValue(regDef.Register);
			return new(asmReg, withReg);
		}

		static string AddCastOrSuffix(string number, string castOrSuffix) {
			if (castOrSuffix.StartsWith("("))
				return castOrSuffix + number;
			return number + castOrSuffix;
		}

		protected override TestArgValueBitness UnsignedImmToTestArgValue(ulong immediate, int encImmSizeBits, int immSizeBits, int argSizeBits) {
			if (encImmSizeBits > immSizeBits)
				throw new InvalidOperationException();
			var (asmCastType, withCastType, mask) = immSizeBits switch {
				4 => (argSizeBits == 8 ? "(byte)" : "U", "U", byte.MaxValue),
				8 => (argSizeBits == immSizeBits ? "(byte)" : "U", "U", byte.MaxValue),
				16 => (argSizeBits == immSizeBits ? "(ushort)" : "U", "U", ushort.MaxValue),
				32 => ("U", "U", uint.MaxValue),
				64 => ("UL", "UL", ulong.MaxValue),
				_ => throw new InvalidOperationException(),
			};
			immediate &= mask;
			string numStr;
			if (immediate <= 9)
				numStr = immediate.ToString(CultureInfo.InvariantCulture);
			else
				numStr = "0x" + immediate.ToString("X", CultureInfo.InvariantCulture);

			var asmStr = AddCastOrSuffix(numStr, asmCastType);
			var withStr = AddCastOrSuffix(numStr, withCastType);

			return new(asmStr, withStr);
		}

		protected override TestArgValueBitness SignedImmToTestArgValue(long immediate, int encImmSizeBits, int immSizeBits, int argSizeBits) {
			if (encImmSizeBits > immSizeBits)
				throw new InvalidOperationException();
			bool isNeg = immediate < 0;
			if (isNeg)
				immediate = -immediate;
			string numStr;
			if ((ulong)immediate <= 9)
				numStr = immediate.ToString(CultureInfo.InvariantCulture);
			else
				numStr = "0x" + immediate.ToString("X", CultureInfo.InvariantCulture);
			if (isNeg)
				numStr = "-" + numStr;

			return new(numStr);
		}

		protected override TestArgValueBitness LabelToTestArgValue() => new("CreateAndEmitLabel(c)", "FirstLabelId");

		readonly struct RenderArg {
			public RenderArg(string name, string type, ArgKind kind) {
				Name = name;
				Type = type;
				Kind = kind;
			}

			public readonly string Name;
			public readonly string Type;
			public readonly ArgKind Kind;
		}
	}
}
