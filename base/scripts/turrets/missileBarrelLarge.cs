//--------------------------------------------------------------------------
// Large Missile Turret
// 
// 
//--------------------------------------------------------------------------

//--------------------------------------------------------------------------
// Sounds
//--------------------------------------
datablock AudioProfile(MBLSwitchSound)
{
   filename    = "fx/powered/turret_missile_activate.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(MBLFireSound)
{
   filename    = "fx/powered/turret_missile_fire.wav";
   description = AudioDefault3d;
   preload = true;
};

//--------------------------------------------------------------------------
// Particle effects: Note that we pull the below datablocks from
//  scripts/weapons/missileLauncher.cs
//--------------------------------------
//datablock ParticleData(MissileSmokeParticle)
//datablock ParticleEmitterData(MissileSmokeEmitter)


//--------------------------------------------------------------------------
// Explosion: from scripts/weapons/disc.cs
//--------------------------------------
//dataBlock ExplosionData(DiscExplosion)

//--------------------------------------------------------------------------
// Projectile
//--------------------------------------
datablock SeekerProjectileData(TurretMissile)
{
   casingShapeName     = "weapon_missile_casement.dts";
   projectileShapeName = "weapon_missile_projectile.dts";
   hasDamageRadius     = true;
   indirectDamage      = 1.0;
   damageRadius        = 4.0;
   radiusDamageType    = $DamageType::MissileTurret;
   kickBackStrength    = 2500;

   flareDistance = 200;
   flareAngle    = 30;
   minSeekHeat   = 0.7;

   explosion           = "MissileExplosion";
   velInheritFactor    = 0.2;

   splash              = MissileSplash;
   baseEmitter         = MissileSmokeEmitter;
   delayEmitter        = MissileFireEmitter;
   puffEmitter         = MissilePuffEmitter;

   lifetimeMS          = 20000;
   muzzleVelocity      = 65.0;
   turningSpeed        = 40.0;
   
   proximityRadius     = 4;

   terrainAvoidanceSpeed = 140;
   terrainScanAhead      = 30;
   terrainHeightFail     = 18;
   terrainAvoidanceRadius = 60;

   useFlechette = true;
   flechetteDelayMs = 550;
   casingDeb = FlechetteDebris;
};

//--------------------------------------------------------------------------
//-------------------------------------- Fusion Turret Image
//
datablock TurretImageData(MissileBarrelLarge)
{
   shapeFile = "turret_missile_large.dts";
   item      = MissileBarrelLargePack;

   projectile = TurretMissile;
   projectileType = SeekerProjectile;

   usesEnergy = true;
   fireEnergy = 60.0;
   minEnergy = 60.0;

   isSeeker     = true;
   seekRadius   = 300;
   maxSeekAngle = 30;
   seekTime     = 1.5;
   minSeekHeat  = 0.7;
   emap = true;
   minTargetingDistance = 40;

   // Turret parameters
   activationMS      = 3000;
   deactivateDelayMS = 2500;
   thinkTimeMS       = 200;
   degPerSecTheta    = 580;
   degPerSecPhi      = 1080;
   attackRadius      = 180;

   // State transitions
   stateName[0]                  = "Activate";
   stateTransitionOnNotLoaded[0] = "Dead";
   stateTransitionOnLoaded[0]    = "ActivateReady";

   stateName[1]                  = "ActivateReady";
   stateSequence[1]              = "Activate";
   stateSound[1]                 = MBLSwitchSound;
   stateTimeoutValue[1]          = 2;
   stateTransitionOnTimeout[1]   = "Ready";
   stateTransitionOnNotLoaded[1] = "Deactivate";
   stateTransitionOnNoAmmo[1]    = "NoAmmo";

   stateName[2]                    = "Ready";
   stateTransitionOnNotLoaded[2]   = "Deactivate";
   stateTransitionOnTriggerDown[2] = "Fire";
   stateTransitionOnNoAmmo[2]      = "NoAmmo";

   stateName[3]                = "Fire";
   stateTransitionOnTimeout[3] = "Reload";
   stateTimeoutValue[3]        = 0.3;
   stateFire[3]                = true;
   stateRecoil[3]              = LightRecoil;
   stateAllowImageChange[3]    = false;
   stateSequence[3]            = "Fire";
   stateSound[3]               = MBLFireSound;
   stateScript[3]              = "onFire";

   stateName[4]                  = "Reload";
   stateTimeoutValue[4]          = 3.5;
   stateAllowImageChange[4]      = false;
   stateSequence[4]              = "Reload";
   stateTransitionOnTimeout[4]   = "Ready";
   stateTransitionOnNotLoaded[4] = "Deactivate";
   stateTransitionOnNoAmmo[4]    = "NoAmmo";

   stateName[5]                = "Deactivate";
   stateSequence[5]            = "Activate";
   stateDirection[5]           = false;
   stateTimeoutValue[5]        = 2;
   stateTransitionOnLoaded[5]  = "ActivateReady";
   stateTransitionOnTimeout[5] = "Dead";

   stateName[6]               = "Dead";
   stateTransitionOnLoaded[6] = "ActivateReady";

   stateName[7]             = "NoAmmo";
   stateTransitionOnAmmo[7] = "Reload";
   stateSequence[7]         = "NoAmmo";
};

