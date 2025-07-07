// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Rasters.Tiffs;

/// <summary>Specifies the photometric interpretation of pixel values in TIFF images.</summary>
public enum PhotometricInterpretation
{
	/// <summary>0 = white is zero, black is maximum value</summary>
	WhiteIsZero = 0,

	/// <summary>1 = black is zero, white is maximum value</summary>
	BlackIsZero = 1,

	/// <summary>2 = RGB color model</summary>
	Rgb = 2,

	/// <summary>3 = palette color (indexed color)</summary>
	Palette = 3,

	/// <summary>4 = transparency mask</summary>
	TransparencyMask = 4,

	/// <summary>5 = CMYK color model</summary>
	Cmyk = 5,

	/// <summary>6 = YCbCr color model</summary>
	YCbCr = 6,

	/// <summary>8 = CIE L*a*b* color model</summary>
	CieLab = 8,

	/// <summary>9 = ICC L*a*b* color model</summary>
	IccLab = 9,

	/// <summary>10 = CFA (Color Filter Array)</summary>
	Cfa = 32803,

	/// <summary>32844 = LogL</summary>
	LogL = 32844,

	/// <summary>32845 = LogLuv</summary>
	LogLuv = 32845
}
