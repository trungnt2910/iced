/* 
SPDX-License-Identifier: MIT
Copyright (C) 2018-present iced project and contributors
 */

// ⚠️This file was generated by GENERATOR!🦹‍♂️

#pragma once

#if defined(DECODER)
#include <array>
#include <cstdint>
#include <stdexcept>
#include <string>
#include "../ToString.h"
#include "../Internal/StringHelpers.h"
namespace Iced::Intel::DecoderInternal {
	enum class SerializedDataKind : std::uint8_t {
		HandlerReference ,
		ArrayReference ,
	};
	constexpr std::uint8_t operator+(const SerializedDataKind& a, const SerializedDataKind& b) { return ((std::uint8_t)a + (std::uint8_t)b); }
	constexpr std::uint8_t operator+(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a + b); }
	constexpr std::uint8_t operator+(const std::uint8_t& a, const SerializedDataKind& b) { return (a + (std::uint8_t)b); }
	constexpr std::uint8_t operator-(const SerializedDataKind& a, const SerializedDataKind& b) { return ((std::uint8_t)a - (std::uint8_t)b); }
	constexpr std::uint8_t operator-(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a - b); }
	constexpr std::uint8_t operator-(const std::uint8_t& a, const SerializedDataKind& b) { return (a - (std::uint8_t)b); }
	constexpr SerializedDataKind operator++(SerializedDataKind& a, int) { auto temp = a; a = SerializedDataKind(a + 1); return temp; }
	constexpr SerializedDataKind& operator++(SerializedDataKind& a) { return a = SerializedDataKind(a + 1); }
	constexpr SerializedDataKind operator--(SerializedDataKind& a, int) { auto temp = a; a = SerializedDataKind(a - 1); return temp; }
	constexpr SerializedDataKind& operator--(SerializedDataKind& a) { return a = SerializedDataKind(a - 1); }
	constexpr bool operator==(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a == b); }
	constexpr bool operator==(const std::uint8_t& a, const SerializedDataKind& b) { return (a == (std::uint8_t)b); }
	constexpr bool operator>=(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a >= b); }
	constexpr bool operator>=(const std::uint8_t& a, const SerializedDataKind& b) { return (a >= (std::uint8_t)b); }
	constexpr bool operator<=(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a <= b); }
	constexpr bool operator<=(const std::uint8_t& a, const SerializedDataKind& b) { return (a <= (std::uint8_t)b); }
	constexpr bool operator>(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a > b); }
	constexpr bool operator>(const std::uint8_t& a, const SerializedDataKind& b) { return (a > (std::uint8_t)b); }
	constexpr bool operator<(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a < b); }
	constexpr bool operator<(const std::uint8_t& a, const SerializedDataKind& b) { return (a < (std::uint8_t)b); }
	constexpr bool operator!=(const SerializedDataKind& a, const std::uint8_t& b) { return ((std::uint8_t)a != b); }
	constexpr bool operator!=(const std::uint8_t& a, const SerializedDataKind& b) { return (a != (std::uint8_t)b); }
}
template <>
constexpr std::string Iced::Intel::ToString(const Iced::Intel::DecoderInternal::SerializedDataKind& e) {
	switch (e) {
		case Iced::Intel::DecoderInternal::SerializedDataKind::HandlerReference: return "HandlerReference";
		case Iced::Intel::DecoderInternal::SerializedDataKind::ArrayReference: return "ArrayReference";
		default: return Internal::StringHelpers::ToDec((std::uint8_t)e);
	}
}
#endif