/* 
SPDX-License-Identifier: MIT
Copyright (C) 2018-present iced project and contributors
 */

// ⚠️This file was generated by GENERATOR!🦹‍♂️

#pragma once

#if defined(INTEL)
#include <array>
#include <stdexcept>
#include <string>
#include "../ToString.h"
#include "../Internal/StringHelpers.h"
namespace Iced::Intel::IntelFormatterInternal {
	enum class SizeOverride {
		None ,
		Size16 ,
		Size32 ,
		Size64 ,
	};
	constexpr int operator+(const SizeOverride& a, const SizeOverride& b) { return ((int)a + (int)b); }
	constexpr int operator+(const SizeOverride& a, const int& b) { return ((int)a + b); }
	constexpr int operator+(const int& a, const SizeOverride& b) { return (a + (int)b); }
	constexpr int operator-(const SizeOverride& a, const SizeOverride& b) { return ((int)a - (int)b); }
	constexpr int operator-(const SizeOverride& a, const int& b) { return ((int)a - b); }
	constexpr int operator-(const int& a, const SizeOverride& b) { return (a - (int)b); }
	constexpr SizeOverride operator++(SizeOverride& a, int) { auto temp = a; a = SizeOverride(a + 1); return temp; }
	constexpr SizeOverride& operator++(SizeOverride& a) { return a = SizeOverride(a + 1); }
	constexpr SizeOverride operator--(SizeOverride& a, int) { auto temp = a; a = SizeOverride(a - 1); return temp; }
	constexpr SizeOverride& operator--(SizeOverride& a) { return a = SizeOverride(a - 1); }
	constexpr bool operator==(const SizeOverride& a, const int& b) { return ((int)a == b); }
	constexpr bool operator==(const int& a, const SizeOverride& b) { return (a == (int)b); }
	constexpr bool operator>=(const SizeOverride& a, const int& b) { return ((int)a >= b); }
	constexpr bool operator>=(const int& a, const SizeOverride& b) { return (a >= (int)b); }
	constexpr bool operator<=(const SizeOverride& a, const int& b) { return ((int)a <= b); }
	constexpr bool operator<=(const int& a, const SizeOverride& b) { return (a <= (int)b); }
	constexpr bool operator>(const SizeOverride& a, const int& b) { return ((int)a > b); }
	constexpr bool operator>(const int& a, const SizeOverride& b) { return (a > (int)b); }
	constexpr bool operator<(const SizeOverride& a, const int& b) { return ((int)a < b); }
	constexpr bool operator<(const int& a, const SizeOverride& b) { return (a < (int)b); }
	constexpr bool operator!=(const SizeOverride& a, const int& b) { return ((int)a != b); }
	constexpr bool operator!=(const int& a, const SizeOverride& b) { return (a != (int)b); }
}
template <>
constexpr std::string Iced::Intel::ToString(const Iced::Intel::IntelFormatterInternal::SizeOverride& e) {
	switch (e) {
		case Iced::Intel::IntelFormatterInternal::SizeOverride::None: return "None";
		case Iced::Intel::IntelFormatterInternal::SizeOverride::Size16: return "Size16";
		case Iced::Intel::IntelFormatterInternal::SizeOverride::Size32: return "Size32";
		case Iced::Intel::IntelFormatterInternal::SizeOverride::Size64: return "Size64";
		default: return Internal::StringHelpers::ToDec((int)e);
	}
}
#endif