// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

use crate::formatter::regs_tbl::MAX_STRING_LENGTH;
use crate::formatter::regs_tbl_ls::REGS_TBL;
use crate::formatter::FormatterString;
use crate::iced_constants::IcedConstants;
use crate::Register;
use alloc::boxed::Box;
use alloc::string::String;
use alloc::vec::Vec;
use core::convert::TryInto;
use core::fmt::Write;
use lazy_static::lazy_static;

lazy_static! {
	pub(super) static ref ALL_REGISTERS: Box<[FormatterString; IcedConstants::REGISTER_ENUM_COUNT]> = {
		let mut v: Vec<_> = (&*REGS_TBL).to_vec();
		debug_assert_eq!(v.len(), IcedConstants::REGISTER_ENUM_COUNT);
		let mut s = String::with_capacity(MAX_STRING_LENGTH);
		for i in 0..8usize {
			write!(s, "st{}", i).unwrap();
			v[Register::ST0 as usize + i] = FormatterString::new(s.clone());
			s.clear();
		}
		#[allow(clippy::unwrap_used)]
		v.into_boxed_slice().try_into().ok().unwrap()
	};
}
