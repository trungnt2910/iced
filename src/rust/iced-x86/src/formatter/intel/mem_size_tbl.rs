// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

use crate::formatter::fmt_consts::*;
use crate::formatter::intel::FormatterString;
use crate::iced_constants::IcedConstants;
use alloc::boxed::Box;
use alloc::vec::Vec;
use core::convert::TryInto;
use lazy_static::lazy_static;

pub(super) struct Info {
	pub(super) bcst_to: &'static FormatterString,
	pub(super) keywords: &'static [&'static FormatterString],
}

// GENERATOR-BEGIN: ConstData
// ⚠️This was generated by GENERATOR!🦹‍♂️
const BROADCAST_TO_KIND_SHIFT: u32 = 5;
const MEMORY_KEYWORDS_MASK: u8 = 31;
// GENERATOR-END: ConstData

// GENERATOR-BEGIN: MemorySizes
// ⚠️This was generated by GENERATOR!🦹‍♂️
#[rustfmt::skip]
static MEM_SIZE_TBL_DATA: [u8; 160] = [
	0x00,
	0x01,
	0x0A,
	0x02,
	0x08,
	0x08,
	0x0B,
	0x0C,
	0x0D,
	0x01,
	0x0A,
	0x02,
	0x08,
	0x0B,
	0x0C,
	0x0D,
	0x02,
	0x07,
	0x09,
	0x0A,
	0x02,
	0x08,
	0x02,
	0x08,
	0x08,
	0x0B,
	0x07,
	0x07,
	0x0A,
	0x02,
	0x08,
	0x09,
	0x0B,
	0x0A,
	0x03,
	0x04,
	0x06,
	0x05,
	0x00,
	0x00,
	0x00,
	0x00,
	0x09,
	0x0D,
	0x00,
	0x09,
	0x0E,
	0x0D,
	0x0A,
	0x0A,
	0x02,
	0x02,
	0x02,
	0x02,
	0x02,
	0x02,
	0x08,
	0x08,
	0x08,
	0x08,
	0x08,
	0x08,
	0x08,
	0x08,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0B,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0C,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x0D,
	0x2A,
	0x22,
	0x22,
	0x4A,
	0x22,
	0x6A,
	0x6A,
	0x42,
	0x42,
	0x28,
	0x28,
	0x28,
	0x6A,
	0x42,
	0x28,
	0x42,
	0x28,
	0x28,
	0x42,
	0x42,
	0x8A,
	0x8A,
	0x62,
	0x62,
	0x48,
	0x48,
	0x48,
	0x8A,
	0x62,
	0x48,
	0x62,
	0x48,
	0x48,
	0x62,
	0x62,
	0xAA,
	0xAA,
	0x82,
	0x82,
	0x68,
	0x68,
	0x68,
	0xAA,
	0x82,
	0x68,
	0x82,
	0x82,
	0x68,
	0x68,
	0x82,
];
// GENERATOR-END: MemorySizes

lazy_static! {
	pub(super) static ref MEM_SIZE_TBL: Box<[Info; IcedConstants::MEMORY_SIZE_ENUM_COUNT]> = {
		let mut v = Vec::with_capacity(IcedConstants::MEMORY_SIZE_ENUM_COUNT);
		let c = &*FORMATTER_CONSTANTS;
		let ac = &*ARRAY_CONSTS;
		for &d in MEM_SIZE_TBL_DATA.iter() {
			let keywords: &'static [&'static FormatterString] = match d & MEMORY_KEYWORDS_MASK {
				// GENERATOR-BEGIN: MemoryKeywordsMatch
				// ⚠️This was generated by GENERATOR!🦹‍♂️
				0x00 => &ac.nothing,
				0x01 => &ac.byte_ptr,
				0x02 => &ac.dword_ptr,
				0x03 => &ac.fpuenv14_ptr,
				0x04 => &ac.fpuenv28_ptr,
				0x05 => &ac.fpustate108_ptr,
				0x06 => &ac.fpustate94_ptr,
				0x07 => &ac.fword_ptr,
				0x08 => &ac.qword_ptr,
				0x09 => &ac.tbyte_ptr,
				0x0A => &ac.word_ptr,
				0x0B => &ac.xmmword_ptr,
				0x0C => &ac.ymmword_ptr,
				0x0D => &ac.zmmword_ptr,
				0x0E => &ac.mem384_ptr,
				// GENERATOR-END: MemoryKeywordsMatch
				_ => unreachable!(),
			};
			let bcst_to = match d >> BROADCAST_TO_KIND_SHIFT {
				// GENERATOR-BEGIN: BroadcastToKindMatch
				// ⚠️This was generated by GENERATOR!🦹‍♂️
				0x00 => &c.empty,
				0x01 => &c.b1to2,
				0x02 => &c.b1to4,
				0x03 => &c.b1to8,
				0x04 => &c.b1to16,
				0x05 => &c.b1to32,
				// GENERATOR-END: BroadcastToKindMatch
				_ => unreachable!(),
			};
			v.push(Info { bcst_to, keywords });
		}
		#[allow(clippy::unwrap_used)]
		v.into_boxed_slice().try_into().ok().unwrap()
	};
}
