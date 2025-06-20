// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Common.Domain.Generic;

// https://tabler.io/docs/base/colors
[Flags]
public enum Color
{
	None   = 0,      // 0000_0000_0000_0000
	White  = 1 << 0, // 0000_0000_0000_0001
	Gray   = 1 << 1, // 0000_0000_0000_0010
	Black  = 1 << 2, // 0000_0000_0000_0100
	Blue   = 1 << 3, // 0000_0000_0000_1000
	Azure  = 1 << 4, // 0000_0000_0001_0000
	Indigo = 1 << 5, // 0000_0000_0010_0000
	Purple = 1 << 6, // 0000_0000_0100_0000
	Pink   = 1 << 7, // 0000_0000_1000_0000
	Red    = 1 << 8, // 0000_0001_0000_0000
	Orange = 1 << 9, // 0000_0010_0000_0000
	Yellow = 1 << 10,// 0000_0100_0000_0000
	Lime   = 1 << 11,// 0000_1000_0000_0000
	Green  = 1 << 12,// 0001_0000_0000_0000
	Teal   = 1 << 13,// 0010_0000_0000_0000
	Cyan   = 1 << 14,// 0100_0000_0000_0000
	Dark   = 1 << 15,// 1000_0000_0000_0000
}
