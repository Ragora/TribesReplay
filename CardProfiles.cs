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
addCardProfile("3Dfx",			"Banshee",							true,       true,			true,		true,			true,			false,		false,			true,			true,				true,				false,		true,									"Voodoo3");
addCardProfile("3Dfx",			"Voodoo3",							true,       true,			true,		true,			true,			false,		true,				true,			true,				true,				false,		true,									"Voodoo3");
addCardProfile("3Dfx",   		"Voodoo4",							true,       false,      true,		false,		true,			false,		false,			true,			true,				false,			false,		false,								"Voodoo5");
addCardProfile("3Dfx",   		"Voodoo5",							true,       false,      true,		false,		true,			false,		false,			true,			true,				false,			false,		false,								"Voodoo5");
addCardProfile("ATI",			"RAGE 128 Pro Dual",				true,			true,			true,		true,			true,			false,		true,				true,			true,				false,			true,			false,								"Rage128");
addCardProfile("ATI",   		"RAGE 128",							true,       true,       true,		true,			true,			false,		true,				true,			true,				false,			false,		false,								"Rage128");
addCardProfile("NVIDIA",		"RIVA TNT/",						true,       true,			true,		false,		false,		true,			true,				true,			false,			false,			false,		false,								"TNT");
addCardProfile("NVIDIA",		"RIVA TNT2/",						true,       true,			true,		false,		false,		false,		true,				true,			false,			false,			false,		false,								"TNT");
addCardProfile("NVIDIA",		"*",									true,			true,			true,		false,		false,		false,		true,				true,			true,				false,			false,		false,								"GeForce");
addCardProfile("Matrox",		"G200",								true,       false,      false,	false,		true,			false,		true,				true,			true,				false,			false,		false,								"Matrox");
addCardProfile("Matrox",		"G400",								true,       true,       false,	false,		true,			false,		true,				true,			true,				false,			false,		false,								"Matrox");
addCardProfile("Matrox",		"*",									true,			true,			false,	false,		true,			false,		true,				true,			true,				false,			false,		false,								"Matrox");
addCardProfile("S3",				"Savage2000",						true,       false,      true,		false,		true,			false,		true,				true,			true,				false,			false,		false,								"Viper");
addCardProfile("ATI",			"Radeon DDR x86/MMX/3DNow!",	true,			true,			true,		false,		false,		false,		true,				false,		false,			false,			false,		false,								"Radeon");
addCardProfile("ATI",			"Radeon",							true,       true,       true,		false,		false,		false,		true,				false,		true,				false,			false,		false,								"Radeon");
addCardProfile("3Dlabs",		"GLINT R3 PT",						true,			true,			true,		false,		false,		false,		true,				true,			true,				false,			false,		false,								"Permedia3");
addCardProfile("Imagination",	"PowerVR KYRO",					true,			true,			true,		false,		false,		false,		true,				true,			true,				false,			false,		false,								"Kyro");