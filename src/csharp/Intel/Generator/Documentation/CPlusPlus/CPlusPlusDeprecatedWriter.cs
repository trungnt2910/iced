// SPDX-License-Identifier: MIT
// Copyright (C) 2018-present iced project and contributors

using Generator.Constants;
using Generator.Enums;
using Generator.IO;

namespace Generator.Documentation.CPlusPlus {
	sealed class CPlusPlusDeprecatedWriter : DeprecatedWriter {
		readonly IdentifierConverter idConverter;

		public CPlusPlusDeprecatedWriter(IdentifierConverter idConverter) =>
			this.idConverter = idConverter;

		public override void WriteDeprecated(FileWriter writer, EnumValue value) {
			if (value.DeprecatedInfo.IsDeprecated) {
				if (value.DeprecatedInfo.NewName is not null) {
					var newValue = value.DeclaringType[value.DeprecatedInfo.NewName];
					WriteDeprecated(writer, newValue.Name(idConverter), value.DeprecatedInfo.Description);
				}
				else
					WriteDeprecated(writer, null, value.DeprecatedInfo.Description);
			}
		}

		public override void WriteDeprecated(FileWriter writer, Constant value) {
			if (value.DeprecatedInfo.IsDeprecated) {
				if (value.DeprecatedInfo.NewName is not null) {
					var newValue = value.DeclaringType[value.DeprecatedInfo.NewName];
					WriteDeprecated(writer, newValue.Name(idConverter), value.DeprecatedInfo.Description);
				}
				else
					WriteDeprecated(writer, null, value.DeprecatedInfo.Description);
			}
		}

		public void WriteDeprecated(FileWriter writer, string? newMember, string? description) {
			string deprecStr;
			if (description is not null)
				deprecStr = description;
			else if (newMember is not null) {
				deprecStr = $"Use {newMember} instead";
			}
			else {
				// Our code still sometimes needs to reference the deprecated values so don't pass in 'true'
				deprecStr = "DEPRECATED. Don't use it!";
			}
			writer.WriteLine($"[[deprecated(\"{deprecStr}\")]]");
		}
	}
}
