// Explicit card profiles for Windows 98 on Intel architectures
//
//						Vendor			Renderer																		AllowOpenGL		AllowD3D		PreferOpenGL
//----------------------------------------------------------------------------------------------------------------------------------
addOSCardProfile(	"3Dfx",			"Banshee",																	true,				false,		true);
addOSCardProfile( "3Dfx",			"3Dfx/Voodoo3 (tm)/2 TMUs/16 MB SDRAM/ICD (Nov  2 2000)",	false,			true,			false);			// Voodoo3
addOSCardProfile(	"3Dfx",			"3Dfx/Voodoo3 (tm)/2 TMUs/16 MB SDRAM/ICD (Jan 17 2000)",	true,				false,		true);			// Voodoo3 3500 TV
addOSCardProfile(	"3Dfx",			"Voodoo4",																	false,			true,			false);
addOSCardProfile(	"3Dfx",			"Voodoo5",																	true,				true,			false);
addOSCardProfile(	"S3",				"Savage2000",																true,				false,		true);
addOSCardProfile(	"Imagination",	"PowerVR KYRO",															false,			true,			false);
addOSCardProfile( "3Dlabs",		"GLINT R3 PT",																false,			true,			false);