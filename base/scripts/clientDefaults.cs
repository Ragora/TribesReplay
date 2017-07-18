$JoinGamePort = 28000;
$pref::Audio::activeDriver = "default";
if ( $platform $= "linux" ) {
   $pref::Audio::drivers = "OpenAL";
} else {
   $pref::Audio::drivers = "Miles";
}
$pref::Audio::frequency = 22100;
$pref::Audio::sampleBits = 16;
$pref::Audio::channels = 2;
$pref::Audio::enableVoiceCapture = 1;
$pref::Audio::voiceChannels = 1;
$pref::Audio::encodingLevel = 0;             
$pref::Audio::decodingMask = 1;
$pref::Audio::forceMaxDistanceUpdate = 0;
$pref::Audio::environmentEnabled = 0;
$pref::Audio::musicEnabled = 1;
$pref::Audio::musicVolume = 0.8;
$pref::Audio::masterVolume = 0.8;
$pref::Audio::effectsVolume = 1.0;
$pref::Audio::voiceVolume = 1.0;
$pref::Audio::guiVolume = 0.8;
$pref::Audio::radioVolume = 0.8;
$pref::Audio::captureGainScale = 1.0;
$pref::currentChatMenu = "defaultChatMenu";
$pref::Email::Column0 = 30;
$pref::Email::Column1 = 250;
$pref::Email::Column2 = 250;
$pref::Email::Column3 = 150;
$pref::Email::SortColumnKey = 3;
$pref::Email::SortInc = 1;
$pref::EnableBadWordFilter = 1;
$pref::FavNames0 = "SCOUT ASSASSIN";
$pref::FavNames1 = "SCOUT SNIPER";
$pref::FavNames2 = "SCOUT DEFENSE";
$pref::FavNames3 = "ASSAULT OFFENSE";
$pref::FavNames4 = "ASSAULT DEPLOYER";
$pref::FavNames5 = "ASSAULT DEFENSE";
$pref::FavNames6 = "TAILGUNNER";
$pref::FavNames7 = "JUGGERNAUT OFFENSE";
$pref::FavNames8 = "JUGGERNAUT DEPLOYER";
$pref::FavNames9 = "JUGGERNAUT DEFENSE";
$pref::FavNames10 = "SCOUT FLAG CHASER";
$pref::FavNames11 = "ASSAULT DESTROYER";
$pref::FavNames12 = "LANDSPIKE DEPLOYER";
$pref::FavNames13 = "INVENTORY DEPLOYER";
$pref::FavNames14 = "FORWARD ASSAULT";
$pref::FavNames15 = "EARLY WARNING";
$pref::FavNames16 = "DECOY";
$pref::FavNames17 = "HEAVY LOVE";
$pref::FavNames18 = "FLAG DEFENDER";
$pref::FavNames19 = "INFILTRATOR";
$pref::Favorite0 = "armor\tScout\tweapon\tPlasma Rifle\tweapon\tChaingun\tweapon\tShocklance\tpack\tCloak Pack\tGrenade\tWhiteout Grenade\tMine\tMine";
$pref::Favorite1 = "armor\tScout\tweapon\tLaser Rifle\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tpack\tEnergy Pack\tGrenade\tFlare Grenade\tMine\tMine";
$pref::Favorite2 = "armor\tScout\tweapon\tBlaster\tweapon\tSpinfusor\tweapon\tPlasma Rifle\tpack\tShield Pack\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite3 = "armor\tAssault\tweapon\tPlasma Rifle\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tweapon\tChaingun\tpack\tShield Pack\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite4 = "armor\tAssault\tweapon\tPlasma Rifle\tweapon\tBlaster\tweapon\tSpinfusor\tweapon\tMissile Launcher\tpack\tInventory Station\tGrenade\tDeployable Camera\tMine\tMine";
$pref::Favorite5 = "armor\tAssault\tweapon\tChaingun\tweapon\tSpinfusor\tweapon\tMissile Launcher\tweapon\tELF Projector\tpack\tShield Pack\tGrenade\tConcussion Grenade\tMine\tMine";
$pref::Favorite6 = "armor\tAssault\tweapon\tChaingun\tweapon\tGrenade Launcher\tweapon\tBlaster\tweapon\tMissile Launcher\tpack\tAmmunition Pack\tGrenade\tFlare Grenade\tMine\tMine";
$pref::Favorite7 = "armor\tJuggernaut\tweapon\tPlasma Rifle\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tweapon\tFusion Mortar\tweapon\tMissile Launcher\tpack\tAmmunition Pack\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite8 = "armor\tJuggernaut\tweapon\tChaingun\tweapon\tMissile Launcher\tweapon\tFusion Mortar\tweapon\tPlasma Rifle\tweapon\tBlaster\tpack\tInventory Station\tGrenade\tDeployable Camera\tMine\tMine";
$pref::Favorite9 = "armor\tJuggernaut\tweapon\tBlaster\tweapon\tPlasma Rifle\tweapon\tGrenade Launcher\tweapon\tMissile Launcher\tweapon\tFusion Mortar\tpack\tShield Pack\tGrenade\tConcussion Grenade\tMine\tMine";
$pref::Favorite10 = "armor\tScout\tweapon\tChaingun\tweapon\tGrenade Launcher\tweapon\tELF Projector\tpack\tEnergy Pack\tGrenade\tConcussion Grenade\tMine\tMine";
$pref::Favorite11 = "armor\tAssault\tweapon\tBlaster\tweapon\tPlasma Rifle\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tpack\tShield Pack\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite12 = "armor\tAssault\tweapon\tChaingun\tweapon\tSpinfusor\tweapon\tPlasma Rifle\tweapon\tGrenade Launcher\tpack\tLandspike Turret\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite13 = "armor\tAssault\tweapon\tPlasma Rifle\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tweapon\tChaingun\tpack\tInventory Station\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite14 = "armor\tAssault\tweapon\tELF Projector\tweapon\tGrenade Launcher\tweapon\tSpinfusor\tweapon\tMissile Launcher\tpack\tEnergy Pack\tGrenade\tWhiteout Grenade\tMine\tMine";
$pref::Favorite15 = "armor\tAssault\tweapon\tChaingun\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tweapon\tELF Projector\tpack\tPulse Sensor Pack\tGrenade\tConcussion Grenade\tMine\tMine";
$pref::Favorite16 = "armor\tScout\tweapon\tChaingun\tweapon\tGrenade Launcher\tweapon\tSpinfusor\tpack\tSensor Jammer Pack\tGrenade\tWhiteout Grenade\tMine\tMine";
$pref::Favorite17 = "armor\tJuggernaut\tweapon\tChaingun\tweapon\tSpinfusor\tweapon\tPlasma Rifle\tweapon\tFusion Mortar\tweapon\tMissile Launcher\tpack\tShield Pack\tGrenade\tConcussion Grenade\tMine\tMine";
$pref::Favorite18 = "armor\tJuggernaut\tweapon\tSpinfusor\tweapon\tGrenade Launcher\tweapon\tFusion Mortar\tweapon\tPlasma Rifle\tweapon\tChaingun\tpack\tShield Pack\tGrenade\tGrenade\tMine\tMine";
$pref::Favorite19 = "armor\tScout\tweapon\tChaingun\tweapon\tSpinfusor\tweapon\tShocklance\tpack\tSatchel Charge\tGrenade\tDeployable Camera\tMine\tMine";
$pref::Forum::Column0 = 290;
$pref::Forum::Column1 = 265;
$pref::Forum::Column2 = 159;
$pref::Forum::CacheSize = 100;
$pref::HudMessageLogSize = 40;
$pref::Input::ActiveConfig = "MyConfig";
$pref::Input::LinkMouseSensitivity = 1;
$pref::Input::KeyboardTurnSpeed = 0.1;
$pref::Interior::TexturedFog = 0;
$pref::IRCClient::autoreconnect = 1;
$pref::IRCClient::awaymsg = "Don't be alarmed.  I'm going to step away from my computer.";
$pref::IRCClient::banmsg = "Get out.  And stay out!";
$pref::IRCClient::kickmsg = "Alright, you're outta here!";
$pref::IRCClient::hostmsg = "left to host a game.";
$pref::IRCClient::showJoin = true;
$pref::IRCClient::showLeave = true;
$pref::Lobby::Column1 = 120;
$pref::Lobby::Column2 = 120;
$pref::Lobby::Column3 = 50;
$pref::Lobby::Column4 = 50;
$pref::Lobby::Column5 = 50;
$pref::Lobby::SortColumnKey = 3;
$pref::Lobby::SortInc = 0;
$pref::Net::simPacketLoss = 0;
$pref::Net::simPing = 0;
$pref::Net::DisplayOnMaster = "Always";
$pref::Net::RegionMask = 2;
$pref::Net::CheckEmail = 0;
$pref::Net::DisconnectChat = 0;
$pref::Net::LagThreshold = 400;
$pref::overrideTeamSkins = 0;
$pref::Player::Count = 0;
$pref::Player::Current = 0;
$pref::Player::defaultFov = 90;
$pref::Player::zoomSpeed = 0;
$pref::RememberPassword = 0;
$pref::sceneLighting::cacheSize = 60000;
$pref::sceneLighting::purgeMethod = "lastModified";
$pref::sceneLighting::cacheLighting = 1;
$pref::sceneLighting::terrainGenerateLevel = 1;
$pref::ServerBrowser::activeFilter = 0;
$pref::ServerBrowser::Column0 = "0 183";
$pref::ServerBrowser::Column1 = "1 30";
$pref::ServerBrowser::Column2 = "2 30";
$pref::ServerBrowser::Column3 = "3 45";
$pref::ServerBrowser::Column4 = "5 143";
$pref::ServerBrowser::Column5 = "7 56";
$pref::ServerBrowser::Column6 = "8 56";
$pref::ServerBrowser::Column7 = "9 155";
$pref::ServerBrowser::Column8 = "4 89";
$pref::ServerBrowser::Column9 = "6 74";
$pref::ServerBrowser::Column10 = "10 70";
$pref::ServerBrowser::FavoriteCount = 0;
$pref::ServerBrowser::SortColumnKey = 0;
$pref::ServerBrowser::SortInc = 1;
$pref::ServerBrowser::InfoWindowOpen = 0;
$pref::ServerBrowser::InfoWindowPos = "145 105";
$pref::ServerBrowser::InfoWindowExtent = "350 270";
$pref::Shell::lastBackground = 0;
$pref::Terrain::DynamicLights = 1;
$pref::toggleVehicleView     = 0;
$pref::Topics::Column0 = 245;
$pref::Topics::Column1 = 50;
$pref::Topics::Column2 = 125;
$pref::Topics::Column3 = 134;
$pref::Topics::SortColumnKey = 3;
$pref::Topics::SortInc = 0;
$pref::Video::displayDevice = "OpenGL";
$pref::chatHudLength = 1;
$pref::useImmersion = 1;
$pref::Video::allowOpenGL = 1;
$pref::Video::allowD3D = 1;
$pref::Video::preferOpenGL = 1;
$pref::Video::appliedPref = 0;
$pref::Video::disableVerticalSync = 1;
$pref::VisibleDistanceMod = 1.0;
$pref::OpenGL::force16BitTexture = "0";
$pref::OpenGL::forcePalettedTexture = "1";
$pref::OpenGL::maxHardwareLights = 3;
$pref::OpenGL::allowTexGen = "1";
