/* 
SPDX-License-Identifier: MIT
Copyright (C) 2018-present iced project and contributors
 */

// ⚠️This file was generated by GENERATOR!🦹‍♂️

#pragma once

#if defined(NASM)
#include <array>
#include <stdexcept>
#include <string>
#include "../ToString.h"
#include "../Internal/StringHelpers.h"
namespace Iced::Intel::NasmFormatterInternal {
	enum class CtorKind {
		Previous ,
		Normal_1 ,
		Normal_2 ,
		AamAad ,
		asz ,
		String ,
		STIG2_2b ,
		bcst ,
		bnd ,
		SignExt_4 ,
		DeclareData ,
		XLAT ,
		er_2 ,
		er_3 ,
		far ,
		far_mem ,
		invlpga ,
		maskmovq ,
		SignExt_3 ,
		STIG1 ,
		STIG2_2a ,
		movabs ,
		sae ,
		nop ,
		OpSize ,
		OpSize2_bnd ,
		OpSize3 ,
		os_2 ,
		os_3 ,
		os_call ,
		push_imm ,
		CC_1 ,
		CC_2 ,
		CC_3 ,
		os_jcc_a_1 ,
		os_jcc_a_2 ,
		os_jcc_a_3 ,
		os_jcc_b_1 ,
		os_jcc_b_2 ,
		os_jcc_b_3 ,
		os_loopcc ,
		os_loop ,
		os_mem ,
		os_mem_reg16 ,
		os_mem2 ,
		pblendvb ,
		push_imm8 ,
		pclmulqdq ,
		pops ,
		imul ,
		Reg16 ,
		Reg32 ,
		reverse ,
	};
	constexpr int operator+(const CtorKind& a, const CtorKind& b) { return ((int)a + (int)b); }
	constexpr int operator+(const CtorKind& a, const int& b) { return ((int)a + b); }
	constexpr int operator+(const int& a, const CtorKind& b) { return (a + (int)b); }
	constexpr int operator-(const CtorKind& a, const CtorKind& b) { return ((int)a - (int)b); }
	constexpr int operator-(const CtorKind& a, const int& b) { return ((int)a - b); }
	constexpr int operator-(const int& a, const CtorKind& b) { return (a - (int)b); }
	constexpr CtorKind operator++(CtorKind& a, int) { auto temp = a; a = CtorKind(a + 1); return temp; }
	constexpr CtorKind& operator++(CtorKind& a) { return a = CtorKind(a + 1); }
	constexpr CtorKind operator--(CtorKind& a, int) { auto temp = a; a = CtorKind(a - 1); return temp; }
	constexpr CtorKind& operator--(CtorKind& a) { return a = CtorKind(a - 1); }
	constexpr bool operator==(const CtorKind& a, const int& b) { return ((int)a == b); }
	constexpr bool operator==(const int& a, const CtorKind& b) { return (a == (int)b); }
	constexpr bool operator>=(const CtorKind& a, const int& b) { return ((int)a >= b); }
	constexpr bool operator>=(const int& a, const CtorKind& b) { return (a >= (int)b); }
	constexpr bool operator<=(const CtorKind& a, const int& b) { return ((int)a <= b); }
	constexpr bool operator<=(const int& a, const CtorKind& b) { return (a <= (int)b); }
	constexpr bool operator>(const CtorKind& a, const int& b) { return ((int)a > b); }
	constexpr bool operator>(const int& a, const CtorKind& b) { return (a > (int)b); }
	constexpr bool operator<(const CtorKind& a, const int& b) { return ((int)a < b); }
	constexpr bool operator<(const int& a, const CtorKind& b) { return (a < (int)b); }
	constexpr bool operator!=(const CtorKind& a, const int& b) { return ((int)a != b); }
	constexpr bool operator!=(const int& a, const CtorKind& b) { return (a != (int)b); }
}
template <>
constexpr std::string Iced::Intel::ToString(const Iced::Intel::NasmFormatterInternal::CtorKind& e) {
	switch (e) {
		case Iced::Intel::NasmFormatterInternal::CtorKind::Previous: return "Previous";
		case Iced::Intel::NasmFormatterInternal::CtorKind::Normal_1: return "Normal_1";
		case Iced::Intel::NasmFormatterInternal::CtorKind::Normal_2: return "Normal_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::AamAad: return "AamAad";
		case Iced::Intel::NasmFormatterInternal::CtorKind::asz: return "asz";
		case Iced::Intel::NasmFormatterInternal::CtorKind::String: return "String";
		case Iced::Intel::NasmFormatterInternal::CtorKind::STIG2_2b: return "STIG2_2b";
		case Iced::Intel::NasmFormatterInternal::CtorKind::bcst: return "bcst";
		case Iced::Intel::NasmFormatterInternal::CtorKind::bnd: return "bnd";
		case Iced::Intel::NasmFormatterInternal::CtorKind::SignExt_4: return "SignExt_4";
		case Iced::Intel::NasmFormatterInternal::CtorKind::DeclareData: return "DeclareData";
		case Iced::Intel::NasmFormatterInternal::CtorKind::XLAT: return "XLAT";
		case Iced::Intel::NasmFormatterInternal::CtorKind::er_2: return "er_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::er_3: return "er_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::far: return "far";
		case Iced::Intel::NasmFormatterInternal::CtorKind::far_mem: return "far_mem";
		case Iced::Intel::NasmFormatterInternal::CtorKind::invlpga: return "invlpga";
		case Iced::Intel::NasmFormatterInternal::CtorKind::maskmovq: return "maskmovq";
		case Iced::Intel::NasmFormatterInternal::CtorKind::SignExt_3: return "SignExt_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::STIG1: return "STIG1";
		case Iced::Intel::NasmFormatterInternal::CtorKind::STIG2_2a: return "STIG2_2a";
		case Iced::Intel::NasmFormatterInternal::CtorKind::movabs: return "movabs";
		case Iced::Intel::NasmFormatterInternal::CtorKind::sae: return "sae";
		case Iced::Intel::NasmFormatterInternal::CtorKind::nop: return "nop";
		case Iced::Intel::NasmFormatterInternal::CtorKind::OpSize: return "OpSize";
		case Iced::Intel::NasmFormatterInternal::CtorKind::OpSize2_bnd: return "OpSize2_bnd";
		case Iced::Intel::NasmFormatterInternal::CtorKind::OpSize3: return "OpSize3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_2: return "os_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_3: return "os_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_call: return "os_call";
		case Iced::Intel::NasmFormatterInternal::CtorKind::push_imm: return "push_imm";
		case Iced::Intel::NasmFormatterInternal::CtorKind::CC_1: return "CC_1";
		case Iced::Intel::NasmFormatterInternal::CtorKind::CC_2: return "CC_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::CC_3: return "CC_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_a_1: return "os_jcc_a_1";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_a_2: return "os_jcc_a_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_a_3: return "os_jcc_a_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_b_1: return "os_jcc_b_1";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_b_2: return "os_jcc_b_2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_jcc_b_3: return "os_jcc_b_3";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_loopcc: return "os_loopcc";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_loop: return "os_loop";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_mem: return "os_mem";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_mem_reg16: return "os_mem_reg16";
		case Iced::Intel::NasmFormatterInternal::CtorKind::os_mem2: return "os_mem2";
		case Iced::Intel::NasmFormatterInternal::CtorKind::pblendvb: return "pblendvb";
		case Iced::Intel::NasmFormatterInternal::CtorKind::push_imm8: return "push_imm8";
		case Iced::Intel::NasmFormatterInternal::CtorKind::pclmulqdq: return "pclmulqdq";
		case Iced::Intel::NasmFormatterInternal::CtorKind::pops: return "pops";
		case Iced::Intel::NasmFormatterInternal::CtorKind::imul: return "imul";
		case Iced::Intel::NasmFormatterInternal::CtorKind::Reg16: return "Reg16";
		case Iced::Intel::NasmFormatterInternal::CtorKind::Reg32: return "Reg32";
		case Iced::Intel::NasmFormatterInternal::CtorKind::reverse: return "reverse";
		default: return Internal::StringHelpers::ToDec((int)e);
	}
}
#endif