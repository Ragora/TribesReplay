//--------------------------------------------------------------------------
// ELF Gun
//--------------------------------------------------------------------------
datablock EffectProfile(ELFGunSwitchEffect)
{
   effectname = "weapons/generic_switch";
   minDistance = 2.5;
};

datablock EffectProfile(ELFGunFireEffect)
{
   effectname = "weapons/ELF_fire";
   minDistance = 10.0;
};

datablock EffectProfile(ElfFireWetEffect)
{
   effectname = "weapons/ELF_underwater";
   minDistance = 10.0;
};

datablock AudioProfile(ELFGunSwitchSound)
{
   filename    = "fx/weapons/generic_switch.wav";
   description = AudioClosest3d;
   preload = true;
   effect = ELFGunSwitchEffect;
};

datablock AudioProfile(ELFGunFireSound)
{
   filename    = "fx/weapons/ELF_fire.wav";
   description = CloseLooping3d;
   preload = true;
   effect = ELFGunFireEffect;
};

datablock AudioProfile(ElfFireWetSound)
{
   filename    = "fx/weapons/ELF_underwater.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(ELFHitTargetSound)
{
   filename    = "fx/weapons/ELF_hit.wav";
   description = CloseLooping3d;
   preload = true;
};

//--------------------------------------
// Projectile
//--------------------------------------
datablock ELFProjectileData(BasicELF)
{
   beamRange         = 37;
   numControlPoints  = 8;
   restorativeFactor = 3.75;
   dragFactor        = 4.5;
   endFactor         = 2.25;
   randForceFactor   = 2;
   randForceTime     = 0.125;
	drainEnergy			= 1.0;
	drainHealth			= 0.0;
   directDamageType  = $DamageType::ELF;
	mainBeamWidth     = 0.1;           // width of blue wave beam
	mainBeamSpeed     = 9.0;            // speed that the beam travels forward
	mainBeamRepeat    = 0.25;           // number of times the texture repeats
   lightningWidth    = 0.1;
   lightningDist      = 0.15;           // distance of lightning from main beam

   fireSound    = ElfGunFireSound;
   wetFireSound = ElfFireWetSound;

	textures[0] = "special/ELFBeam";
   textures[1] = "special/ELFLightning";
   textures[2] = "special/BlueImpact";

};

//--------------------------------------
// Rifle and item...
//--------------------------------------
datablock ItemData(ELFGun)
{
   className    = Weapon;
   catagory     = "Spawn Items";
   shapeFile    = "weapon_elf.dts";
   image        = ELFGunImage;
   mass         = 1;
   elasticity   = 0.2;
   friction     = 0.6;
   pickupRadius = 2;
	pickUpName = "an ELF gun";

   computeCRC = true;
   emap = true;
};

datablock ShapeBaseImageData(ELFGunImage)
{
   className = WeaponImage;

   shapeFile = "weapon_elf.dts";
   item = ELFGun;
   offset = "0 0 0";

   projectile = BasicELF;
   projectileType = ELFProjectile;
   deleteLastProjectile = true;
   emap = true;
   

	usesEnergy = true;
 	minEnergy = 3;

   stateName[0]                     = "Activate";
   stateSequence[0]                 = "Activate";
	stateSound[0]                    = ELFGunSwitchSound;
   stateTimeoutValue[0]             = 0.5;
   stateTransitionOnTimeout[0]      = "ActivateReady";

   stateName[1]                     = "ActivateReady";
   stateTransitionOnAmmo[1]         = "Ready";
   stateTransitionOnNoAmmo[1]       = "NoAmmo";

   stateName[2]                     = "Ready";
   stateTransitionOnNoAmmo[2]       = "NoAmmo";
   stateTransitionOnTriggerDown[2]  = "CheckWet";

   stateName[3]                     = "Fire";
	stateEnergyDrain[3]              = 5;
   stateFire[3]                     = true;
   stateAllowImageChange[3]         = false;
   stateScript[3]                   = "onFire";
   stateTransitionOnTriggerUp[3]    = "Deconstruction";
   stateTransitionOnNoAmmo[3]       = "Deconstruction";
   //stateSound[3]                    = ElfFireWetSound;

   stateName[4]                     = "NoAmmo";
   stateTransitionOnAmmo[4]         = "Ready";

   stateName[5]                     = "Deconstruction";
   stateScript[5]                   = "deconstruct";
   stateTransitionOnTimeout[5]      = "Ready";
   stateTransitionOnNoAmmo[6]       = "NoAmmo";
   
   stateName[6]                     = "DryFire";
   stateSound[6]                    = ElfFireWetSound;
   stateTimeoutValue[6]             = 0.5;
   stateTransitionOnTimeout[6]      = "Ready";
   
   stateName[7]                     = "CheckWet";
   stateTransitionOnWet[7]          = "DryFire";
   stateTransitionOnNotWet[7]       = "Fire";
};
