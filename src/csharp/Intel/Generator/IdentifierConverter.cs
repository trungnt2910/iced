// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

using System;
using System.Collections.Generic;
using System.Text;
using Generator.Enums;

namespace Generator {
	/// <summary>
	/// Converts C# PascalCase identifiers to eg. snake_case
	/// </summary>
	abstract class IdentifierConverter {
		readonly StringBuilder sb = new();

		protected abstract string EnumSeparator { get; }
		public abstract string Type(string name);
		public abstract string Field(string name);
		public abstract string EnumField(string name);
		public abstract string PropertyDoc(string name);
		public abstract string MethodDoc(string name);
		public abstract string Method(string name);
		public abstract string Constant(string name);
		public abstract string Static(string name);
		public abstract string Namespace(string name);
		public abstract string Argument(string name);
		public string ToDeclTypeAndValue(EnumValue value) =>
			$"{value.DeclaringType.Name(this)}{EnumSeparator}{value.Name(this)}";

		protected string ToSnakeCase(string name) => ToSnakeCase(name, upper: false);

		protected string ToScreamingSnakeCase(string name) =>
			name switch {
				"Fxsave_512Byte" => "FXSAVE_512BYTE",
				"Fxsave64_512Byte" => "FXSAVE64_512BYTE",
				"Cmpxchg486A" => "CMPXCHG486A",
				"NoMPFX_0FBC" => "NO_MPFX_0FBC",
				"NoMPFX_0FBD" => "NO_MPFX_0FBD",
				"NoLahfSahf64" => "NO_LAHF_SAHF_64",
				"OpKind_MemoryESDI" => "OP_KIND_MEMORY_ES_DI",
				"OpKind_MemoryESEDI" => "OP_KIND_MEMORY_ES_EDI",
				"OpKind_MemoryESRDI" => "OP_KIND_MEMORY_ES_RDI",
				"HighLegacy8BitRegs" => "HIGH_LEGACY_8_BIT_REGS",
				"Handlers_MAP0Index" => "HANDLERS_MAP0_INDEX",
				"Handlers_0FIndex" => "HANDLERS_0F_INDEX",
				"Handlers_0F38Index" => "HANDLERS_0F38_INDEX",
				"Handlers_0F3AIndex" => "HANDLERS_0F3A_INDEX",
				"Handler66Reg" => "HANDLER_66_REG",
				"Handler66Mem" => "HANDLER_66_MEM",
				"Cyrix_SMINT_0F7E" => "CYRIX_SMINT_0F7E",
				_ => ToSnakeCase(name, upper: true),
			};

		string ToSnakeCase(string name, bool upper) {
			sb.Clear();
			bool prevWasUpper = false;
			foreach (var c in name) {
				bool isUpper = char.IsUpper(c);
				if (isUpper && !prevWasUpper) {
					if (sb.Length > 0 && sb[^1] != '_')
						sb.Append('_');
				}
				prevWasUpper = isUpper;
				sb.Append(upper ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
			}
			return sb.ToString();
		}

		protected string ToLowerCamelCase(string name) {
			sb.Clear();
			int i = 0;
			while (i < name.Length && char.IsUpper(name[i]))
				sb.Append(char.ToLowerInvariant(name[i++]));
			sb.Append(name, i, name.Length - i);
			return sb.ToString();
		}
	}

	sealed class CSharpIdentifierConverter : IdentifierConverter {
		public static IdentifierConverter Create() => new CSharpIdentifierConverter();
		CSharpIdentifierConverter() { }
		protected override string EnumSeparator => ".";
		public override string Type(string name) => name;
		public override string Field(string name) => Escape(name);
		public override string EnumField(string name) => Escape(name);
		public override string PropertyDoc(string name) => name;
		public override string MethodDoc(string name) => name;
		public override string Method(string name) => Escape(name);
		public override string Constant(string name) => Escape(name);
		public override string Static(string name) => Escape(name);
		public override string Namespace(string name) => Escape(name);
		public override string Argument(string name) => Escape(name);

		static readonly HashSet<string> keywords = new(StringComparer.Ordinal) {
			"abstract", "as", "base", "bool",
			"break", "byte", "case", "catch",
			"char", "checked", "class", "const",
			"continue", "decimal", "default", "delegate",
			"do", "double", "else", "enum",
			"event", "explicit", "extern", "false",
			"finally", "fixed", "float", "for",
			"foreach", "goto", "if", "implicit",
			"in", "int", "interface", "internal",
			"is", "lock", "long", "namespace",
			"new", "null", "object", "operator",
			"out", "override", "params", "private",
			"protected", "public", "readonly", "ref",
			"return", "sbyte", "sealed", "short",
			"sizeof", "stackalloc", "static", "string",
			"struct", "switch", "this", "throw",
			"true", "try", "typeof", "uint",
			"ulong", "unchecked", "unsafe", "ushort",
			"using", "using", "static", "virtual", "void",
			"volatile", "while",
		};

		static string Escape(string name) => keywords.Contains(name) ? "@" + name : name;
	}

	sealed class RustIdentifierConverter : IdentifierConverter {
		public static IdentifierConverter Create() => new RustIdentifierConverter();
		RustIdentifierConverter() { }
		protected override string EnumSeparator => "::";
		public override string Type(string name) => name;
		public override string Field(string name) => ToSnakeCase(name);
		public override string EnumField(string name) => name;
		public override string PropertyDoc(string name) => ToSnakeCase(name) + "()";
		public override string MethodDoc(string name) => ToSnakeCase(name) + "()";
		public override string Method(string name) => ToSnakeCase(name);
		public override string Constant(string name) => ToScreamingSnakeCase(name);
		public override string Static(string name) => ToScreamingSnakeCase(name);
		public override string Namespace(string name) => ToSnakeCase(name);
		public override string Argument(string name) => ToSnakeCase(name);
	}

	sealed class RustJSIdentifierConverter : IdentifierConverter {
		public static IdentifierConverter Create() => new RustJSIdentifierConverter();
		RustJSIdentifierConverter() { }
		protected override string EnumSeparator => "::";
		public override string Type(string name) => name;
		public override string Field(string name) => ToLowerCamelCase(name);
		public override string EnumField(string name) => name;
		public override string PropertyDoc(string name) => ToLowerCamelCase(name);
		public override string MethodDoc(string name) => ToLowerCamelCase(name) + "()";
		public override string Method(string name) => ToLowerCamelCase(name);
		public override string Constant(string name) => name;
		public override string Static(string name) => name;
		public override string Namespace(string name) => name;
		public override string Argument(string name) => ToLowerCamelCase(name);
	}

	sealed class PythonIdentifierConverter : IdentifierConverter {
		public static IdentifierConverter Create() => new PythonIdentifierConverter();
		PythonIdentifierConverter() { }
		protected override string EnumSeparator => ".";
		public override string Type(string name) => name;
		public override string Field(string name) => "__" + ToSnakeCase(name);
		public override string EnumField(string name) => ToScreamingSnakeCase(name);
		public override string PropertyDoc(string name) => ToSnakeCase(name);
		public override string MethodDoc(string name) => ToSnakeCase(name);
		public override string Method(string name) => ToSnakeCase(name);
		public override string Constant(string name) => ToScreamingSnakeCase(name);
		public override string Static(string name) => ToScreamingSnakeCase(name);
		public override string Namespace(string name) => ToSnakeCase(name);
		public override string Argument(string name) => ToSnakeCase(name);
	}

	sealed class CPlusPlusIdentifierConverter : IdentifierConverter {
		public static IdentifierConverter Create() => new CPlusPlusIdentifierConverter();
		CPlusPlusIdentifierConverter() { }

		protected override string EnumSeparator => "::";
		public override string Type(string name) => Escape(name);
		public override string Field(string name) => Escape(name);
		public override string EnumField(string name) => Escape(name);
		public override string PropertyDoc(string name) => Escape(name);
		public override string MethodDoc(string name) => Escape(name);
		public override string Method(string name) => Escape(ToSnakeCase(name));
		public override string Constant(string name) => Escape(ToScreamingSnakeCase(name));
		public override string Static(string name) => Escape(ToSnakeCase(name));
		public override string Namespace(string name) => Escape(name);
		public override string Argument(string name) => Escape(ToLowerCamelCase(name));

		static readonly HashSet<string> keywords = new(StringComparer.Ordinal) {
			"alignas", "alignof", "asm", "auto", "bool", "break", "case", "catch",
			"char", "char16_t", "char32_t", "class", "const", "constexpr", "const_cast",
			"continue", "decltype", "default", "delete", "do", "double", "dynamic_cast", 
			"else", "enum", "explicit", "export", "extern", "false", "float", "for", 
			"friend", "goto", "if", "inline", "int", "long", "mutable", "namespace", 
			"new", "noexcept", "nullptr", "operator", "private", "protected", "public", 
			"register", "reinterpret_cast", "return", "short", "signed", "sizeof", 
			"static", "static_assert", "static_cast", "struct", "switch", "template",
			"this", "thread_local", "throw", "true", "try", "typedef", "typeid", 
			"typename", "union", "unsigned", "using", "virtual", "void", "volatile", 
			"wchar_t", "while", "and", "and_eq", "bitand", "bitor", "compl", "not", 
			"not_eq", "or", "or_eq", "xor", "xor_eq",
		};

		static string Escape(string name) => keywords.Contains(name) ? name+"_" : name;
	}
}
