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
	enum class BranchSizeInfo {
		None ,
		Short ,
	};
	constexpr int operator+(const BranchSizeInfo& a, const BranchSizeInfo& b) { return ((int)a + (int)b); }
	constexpr int operator+(const BranchSizeInfo& a, const int& b) { return ((int)a + b); }
	constexpr int operator+(const int& a, const BranchSizeInfo& b) { return (a + (int)b); }
	constexpr int operator-(const BranchSizeInfo& a, const BranchSizeInfo& b) { return ((int)a - (int)b); }
	constexpr int operator-(const BranchSizeInfo& a, const int& b) { return ((int)a - b); }
	constexpr int operator-(const int& a, const BranchSizeInfo& b) { return (a - (int)b); }
	constexpr BranchSizeInfo operator++(BranchSizeInfo& a, int) { auto temp = a; a = BranchSizeInfo(a + 1); return temp; }
	constexpr BranchSizeInfo& operator++(BranchSizeInfo& a) { return a = BranchSizeInfo(a + 1); }
	constexpr BranchSizeInfo operator--(BranchSizeInfo& a, int) { auto temp = a; a = BranchSizeInfo(a - 1); return temp; }
	constexpr BranchSizeInfo& operator--(BranchSizeInfo& a) { return a = BranchSizeInfo(a - 1); }
	constexpr bool operator==(const BranchSizeInfo& a, const int& b) { return ((int)a == b); }
	constexpr bool operator==(const int& a, const BranchSizeInfo& b) { return (a == (int)b); }
	constexpr bool operator>=(const BranchSizeInfo& a, const int& b) { return ((int)a >= b); }
	constexpr bool operator>=(const int& a, const BranchSizeInfo& b) { return (a >= (int)b); }
	constexpr bool operator<=(const BranchSizeInfo& a, const int& b) { return ((int)a <= b); }
	constexpr bool operator<=(const int& a, const BranchSizeInfo& b) { return (a <= (int)b); }
	constexpr bool operator>(const BranchSizeInfo& a, const int& b) { return ((int)a > b); }
	constexpr bool operator>(const int& a, const BranchSizeInfo& b) { return (a > (int)b); }
	constexpr bool operator<(const BranchSizeInfo& a, const int& b) { return ((int)a < b); }
	constexpr bool operator<(const int& a, const BranchSizeInfo& b) { return (a < (int)b); }
	constexpr bool operator!=(const BranchSizeInfo& a, const int& b) { return ((int)a != b); }
	constexpr bool operator!=(const int& a, const BranchSizeInfo& b) { return (a != (int)b); }
}
template <>
constexpr std::string Iced::Intel::ToString(const Iced::Intel::IntelFormatterInternal::BranchSizeInfo& e) {
	switch (e) {
		case Iced::Intel::IntelFormatterInternal::BranchSizeInfo::None: return "None";
		case Iced::Intel::IntelFormatterInternal::BranchSizeInfo::Short: return "Short";
		default: return Internal::StringHelpers::ToDec((int)e);
	}
}
#endif