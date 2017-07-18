$teamSkin[0] = 'blank';
$teamSkin[1] = 'base';
$teamSkin[2] = 'baseb';
$teamSkin[3] = 'swolf';
$teamSkin[4] = 'dsword';
$teamSkin[5] = 'beagle';
$teamSkin[6] = 'cotp';

$teamName[0] = 'Unassigned';
$teamName[1] = 'Storm';
$teamName[2] = 'Inferno';
$teamName[3] = 'Star Wolf';
$teamName[4] = 'Diamond Sword';
$teamName[5] = 'Blood Eagle';
$teamName[6] = 'Phoenix';

$holoName[0] = "";
$holoName[1] = "Storm";
$holoName[2] = "Inferno";
$holoName[3] = "Starwolf";
$holoName[4] = "DSword";
$holoName[5] = "BloodEagle";
$holoName[6] = "Harbinger";
                  
$Host::AdminList = "";       // all players that will be automatically an admin upon joining server
$Host::SuperAdminList = "";  // all players that will be automatically a super admin upon joining server               
$Host::BindAddress = "";     // set to an ip address if the server wants to specify which NIC/IP to use
$Host::RulesSet = "Base";                        
$Host::Port = 28000;
$Host::GameName = "Tribes 2 Server";
$Host::Password = "";
$Host::AdminPassword = "";
$Host::Dedicated = 0;
$Host::MissionType = "CTF";
$Host::Map = "Katabatic";
$Host::MaxPlayers = 64;
$Host::TimeLimit = 20;
$Host::BotCount = 2;
$Host::BotsEnabled = 0;
$Host::MinBotDifficulty = 0.5;
$Host::MaxBotDifficulty = 0.75;
$Host::Info = "This is a Tribes 2 Server.";
$Host::NoSmurfs = 0;
$Host::VoteTime = 20;               // amount of time before votes are calculated
$Host::VotePassPercent = 60;        // percent needed to pass a vote
$Host::KickBanTime = 300;           // specified in seconds
$Host::BanTime = 1800;              // specified in seconds
$Host::PlayerRespawnTimeout = 60;   // time before a dead player is forced into observer mode
$Host::warmupTime = 20;
$Host::TournamentMode = 0;
$Host::allowAdminPlayerVotes = 1;
$Host::FloodProtectionEnabled = 1;
$Host::MaxMessageLen = 120;

$MasterServerAddress = "IP:198.74.40.152:28000";

$Audio::voiceCodec = ".v12";   // .v12 (1.2 kbits/sec), .v24 (2.4 kbits/sec), .v29 (2.9kbits/sec)
