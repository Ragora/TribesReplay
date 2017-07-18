// Explicit card profiles
//
// SafeMode -- forces destruction of rendering context before resolution change
// LockArray -- inhibits compiled vertex arrays if available
// SubImage -- inhibits glTexSubImage*
// FogTexture -- require bound texture for combine extension
// NoEnvColor -- card doesn't support texture environment color
// ClipHigh -- clip off high resolutions (> 1152x864)
// DeleteContext -- delete rendering context (ignored for all OSs except W2K)
// TexCompress -- inhibits texture compression if available
// InteriorLock -- lock arrays rendering Interiors
// SkipFirstFog -- skip first two-pass fog (dumb 3Dfx hack)
// Only16 -- inhibit the 32-bit resolutions
// NoArraysAlpha -- don't use glDrawArrays with a GL_ALPHA texture
// ProFile -- explicit file of graphic settings
//
//             Vendor			Renderer								SafeMode    LockArray   SubImage	FogTexture	NoEnvColor	ClipHigh		DeleteContext	TexCompress	InteriorLock	SkipFirstFog	Only16		NoArraysAlpha	ProFile
//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
addCardProfile("VA Linux Systems, Inc.",			"Banshee",							true,       true,			true,		true,			true,			false,		false,			true,			true,				true,				false,		true,									"DRI-Voodoo3");
addCardProfile("VA Linux Systems, Inc.",			"Voodoo3",							true,       true,			true,		true,			true,			false,		true,				true,			true,				true,				false,		true,									"DRI-Voodoo3");
addCardProfile("VA Linux Systems, Inc.",   		"Voodoo4",							true,       false,      true,		false,		true,			false,		false,			true,			true,				false,			false,		false,								"DRI-Voodoo5");
addCardProfile("VA Linux Systems, Inc.",   		"Voodoo5",							true,       false,      true,		false,		true,			false,		false,			true,			true,				false,			false,		false,								"DRI-Voodoo5");
addCardProfile("VA Linux Systems, Inc.",   		"Rage128",							true,       true,       true,		true,			true,			false,		true,				true,			true,				false,			false,		false,								"DRI-Rage128");
addCardProfile("VA Linux Systems, Inc.",			"Radeon",							true,       true,       true,		false,		false,		false,		true,				false,		true,				false,			false,		false,								"DRI-Radeon");
addCardProfile("VA Linux Systems, Inc.",		"G400",								true,       true,       false,	false,		true,			false,		true,				true,			true,				false,			false,		false,								"DRI-Matrox");
addCardProfile("VA Linux Systems, Inc.",		"G450",									true,			true,			false,	false,		true,			false,		true,				true,			true,				false,			false,		false,								"DRI-Matrox");
