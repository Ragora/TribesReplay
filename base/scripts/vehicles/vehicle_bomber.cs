//**************************************************************
// THUNDERSWORD BOMBER
//**************************************************************
//**************************************************************
// SOUNDS
//**************************************************************

datablock AudioProfile(BomberFlyerEngineSound)
{
   filename    = "fx/vehicles/bomber_engine.wav";
   description = AudioDefaultLooping3d;
   preload = true;
};

datablock AudioProfile(BomberFlyerThrustSound)
{
   filename    = "fx/vehicles/bomber_boost.wav";
   description = AudioDefaultLooping3d;
   preload = true;
};

datablock AudioProfile(FusionExpSound)
// Sound played when mortar impacts on target
{
   filename    = "fx/powered/turret_mortar_explode.wav";
   description = "AudioBIGExplosion3d";
   preload = true;
};

datablock AudioProfile(BomberTurretFireSound)
{
   filename    = "fx/vehicles/bomber_turret_fire.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberTurretActivateSound)
{
   filename    = "fx/vehicles/bomber_turret_activate.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberTurretReloadSound)
{
   filename    = "fx/vehicles/bomber_turret_reload.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberTurretIdleSound)
{
   filename    = "fx/misc/diagnostic_on.wav";
   description = ClosestLooping3d;
   preload = true;
};

datablock AudioProfile(BomberTurretDryFireSound)
{
   filename    = "fx/vehicles/bomber_turret_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};
   
datablock AudioProfile(BomberBombReloadSound)
{
   filename    = "fx/vehicles/bomber_bomb_reload.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberBombProjectileSound)
{
   filename    = "fx/vehicles/bomber_bomb_projectile.wav";
   description = AudioDefaultLooping3d;
   preload = true;
};

datablock AudioProfile(BomberBombDryFireSound)
{
   filename    = "fx/vehicles/bomber_bomb_dryfire.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberBombFireSound)
{
   filename    = "fx/vehicles/bomber_bomb_reload.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(BomberBombIdleSound)
{
   filename    = "fx/misc/diagnostic_on.wav";
   description = ClosestLooping3d;
   preload = true;
};

//**************************************************************
// VEHICLE CHARACTERISTICS
//**************************************************************

datablock FlyingVehicleData(BomberFlyer) : BomberDamageProfile
{
   spawnOffset = "0 0 2";

   catagory = "Vehicles";
   shapeFile = "vehicle_air_bomber.dts";
   multipassenger = true;
   computeCRC = true;

   weaponNode = 1;

   debrisShapeName = "vehicle_air_bomber_debris.dts";
   debris = ShapeDebris;

   drag    = 0.2;
   density = 1.0;

   mountPose[0] = sitting;
   mountPose[1] = sitting;
   numMountPoints = 3;  
   isProtectedMountPoint[0] = true;
   isProtectedMountPoint[1] = true;
   isProtectedMountPoint[2] = true;

   cameraDefaultFov = 90.0;
   cameraMinFov = 5.0;
   cameraMaxFov = 120.0;
   
   cameraMaxDist = 22;
   cameraOffset = 5;
   cameraLag = 1.0;
   explosion = LargeAirVehicleExplosion;
	explosionDamage = 0.5;
	explosionRadius = 5.0;

   maxDamage = 2.80;     // Total health
   destroyedLevel = 2.80;   // Damage textures show up at this health level

   isShielded = true;
   energyPerDamagePoint = 150;
   maxEnergy = 400;      // Afterburner and any energy weapon pool
   minDrag = 60;           // Linear Drag (eventually slows you down when not thrusting...constant drag)
   rotationalDrag = 1800;        // Angular Drag (dampens the drift after you stop moving the mouse...also tumble drag)
   rechargeRate = 0.8;

   // Auto stabilize speed
   maxAutoSpeed = 15;       // Autostabilizer kicks in when less than this speed. (meters/second)
   autoAngularForce = 1500;      // Angular stabilizer force (this force levels you out when autostabilizer kicks in)
   autoLinearForce = 300;        // Linear stabilzer force (this slows you down when autostabilizer kicks in)
   autoInputDamping = 0.95;      // Dampen control input so you don't whack out at very slow speeds
   
   // Maneuvering
   maxSteeringAngle = 5;    // Max radiens you can rotate the wheel. Smaller number is more maneuverable.
   horizontalSurfaceForce = 5;   // Horizontal center "wing" (provides "bite" into the wind for climbing/diving and turning)
   verticalSurfaceForce = 8;     // Vertical center "wing" (controls side slip. lower numbers make MORE slide.)
   maneuveringForce = 4700;      // Horizontal jets (W,S,D,A key thrust)
   steeringForce = 1100;         // Steering jets (force applied when you move the mouse)
   steeringRollForce = 300;      // Steering jets (how much you heel over when you turn)
   rollForce = 8;                // Auto-roll (self-correction to right you after you roll/invert)
   hoverHeight = 5;        // Height off the ground at rest
   createHoverHeight = 3;  // Height off the ground when created

   // Turbo Jet
   jetForce = 3000;      // Afterburner thrust (this is in addition to normal thrust)
   minJetEnergy = 40.0;     // Afterburner can't be used if below this threshhold.
   jetEnergyDrain = 3.0;       // Energy use of the afterburners (low number is less drain...can be fractional)
   vertThrustMultiple = 2.0;

   dustEmitter = LargeVehicleLiftoffDustEmitter;
   triggerDustHeight = 4.0;
   dustHeight = 2.0;

   damageEmitter[0] = LightDamageSmoke;
   damageEmitter[1] = HeavyDamageSmoke;
   damageEmitter[2] = DamageBubbles;
   damageEmitterOffset[0] = "3.0 -3.0 0.0 ";
   damageEmitterOffset[1] = "-3.0 -3.0 0.0 ";
   damageLevelTolerance[0] = 0.3;
   damageLevelTolerance[1] = 0.7;
   numDmgEmitterAreas = 2;

   // Rigid body
   mass = 350;        // Mass of the vehicle
   bodyFriction = 0;     // Don't mess with this.
   bodyRestitution = 0.5;   // When you hit the ground, how much you rebound. (between 0 and 1)
   minRollSpeed = 0;     // Don't mess with this.
   softImpactSpeed = 20;       // Sound hooks. This is the soft hit.
   hardImpactSpeed = 25;    // Sound hooks. This is the hard hit.

   // Ground Impact Damage (uses DamageType::Ground)
   minImpactSpeed = 20;      // If hit ground at speed above this then it's an impact. Meters/second
   speedDamageScale = 0.150;

   // Object Impact Damage (uses DamageType::Impact)
   collDamageThresholdVel = 25;
   collDamageMultiplier   = 0.030;

   //
   minTrailSpeed = 15;      // The speed your contrail shows up at.
   trailEmitter = ContrailEmitter;
   forwardJetEmitter = FlyerJetEmitter;
   downJetEmitter = FlyerJetEmitter;

   //
   jetSound = BomberFlyerThrustSound;
   engineSound = BomberFlyerEngineSound;
   softImpactSound = SoftImpactSound;
   hardImpactSound = HardImpactSound;
   //wheelImpactSound = WheelImpactSound;

   //
   softSplashSoundVelocity = 15.0; 
   mediumSplashSoundVelocity = 20.0;   
   hardSplashSoundVelocity = 30.0;   
   exitSplashSoundVelocity = 10.0;
   
   exitingWater      = VehicleExitWaterHardSound;
   impactWaterEasy   = VehicleImpactWaterSoftSound;
   impactWaterMedium = VehicleImpactWaterMediumSound;
   impactWaterHard   = VehicleImpactWaterHardSound;
   waterWakeSound    = VehicleWakeHardSplashSound; 
  
   minMountDist = 4;

   splashEmitter[0] = VehicleFoamDropletsEmitter;
   splashEmitter[1] = VehicleFoamEmitter;

   shieldImpact = VehicleShieldImpact;

   cmdCategory = "Tactical";
   cmdIcon = CMDFlyingBomberIcon;
   cmdMiniIconName = "commander/MiniIcons/com_bomber_grey";
   targetNameTag = 'Thundersword';
   targetTypeTag = 'Bomber';
   sensorData = VehiclePulseSensor;

   checkRadius = 7.1895;
   observeParameters = "1 10 10";
};

//**************************************************************
// WEAPONS
//**************************************************************

//-------------------------------------
// BOMBER BELLY TURRET GUN (projectile)
//-------------------------------------

datablock ShockwaveData(BomberFusionShockwave)
{
   width = 0.5;
   numSegments = 13;
   numVertSegments = 1;
   velocity = 0.5;
   acceleration = 2.0;
   lifetimeMS = 900;
   height = 0.1;
   verticalCurve = 0.5;

   mapToTerrain = false;
   renderBottom = false;
   orientToNormal = true;

   texture[0] = "special/shockwave5";
   texture[1] = "special/gradient";
   texWrap = 3.0;

   times[0] = 0.0;
   times[1] = 0.5;
   times[2] = 1.0;

   colors[0] = "0.6 0.6 1.0 1.0";
   colors[1] = "0.6 0.3 1.0 0.5";
   colors[2] = "0.0 0.0 1.0 0.0";
};

datablock ParticleData(BomberFusionExplosionParticle1)
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = -0.0;
   lifetimeMS           = 600;
   lifetimeVarianceMS   = 000;
   textureName          = "special/crescent4";
   colors[0] = "0.6 0.6 1.0 1.0";
   colors[1] = "0.6 0.3 1.0 1.0";
   colors[2] = "0.0 0.0 1.0 0.0";
   sizes[0]      = 0.25;
   sizes[1]      = 0.5;
   sizes[2]      = 1.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(BomberFusionExplosionEmitter)
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 0;
   ejectionVelocity = 2;
   velocityVariance = 1.5;
   ejectionOffset   = 0.0;
   thetaMin         = 80;
   thetaMax         = 90;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "BomberFusionExplosionParticle1";
};

datablock ExplosionData(BomberFusionBoltExplosion)
{
   soundProfile   = blasterExpSound;
   shockwave      = BomberFusionShockwave;
   emitter[0]     = BomberFusionExplosionEmitter;
};


datablock LinearFlareProjectileData(BomberFusionBolt)
{
   projectileShapeName = "";
   directDamage        = 0.35;
   directDamageType    = $DamageType::BellyTurret;
   hasDamageRadius     = false;
   explosion           = BomberFusionBoltExplosion;
   sound               = BlasterProjectileSound;

   dryVelocity       = 75.0;
   wetVelocity       = 75.0;
   velInheritFactor  = 1.0;
   fizzleTimeMS      = 2000;
   lifetimeMS        = 3000;
   explodeOnDeath    = false;
   reflectOnWaterImpactAngle = 0.0;
   explodeOnWaterImpact      = true;
   deflectionOnWaterImpact   = 0.0;
   fizzleUnderwaterMS        = -1;

   activateDelayMS = 100;

   numFlares         = 0;
   size              = 0.15;
   flareColor        = "0.7 0.3 1.0";
   flareModTexture   = "flaremod";
   flareBaseTexture  = "flarebase";
};

//-------------------------------------
// BOMBER BELLY TURRET CHARACTERISTICS
//-------------------------------------

datablock TurretData(BomberTurret) : TurretDamageProfile
{
   className      = VehicleTurret;
   catagory       = "Turrets";
   shapeFile      = "turret_belly_base.dts";
   preload        = true;

   mass           = 1.0;  // Not really relevant
   repairRate     = 0;
   maxDamage      = BomberFlyer.maxDamage;
   destroyedLevel = BomberFlyer.destroyedLevel;

   thetaMin = 90;
   thetaMax = 180;
   
   inheritEnergyFromMount = true;
   firstPersonOnly = true;
   useEyePoint = true;
   numWeapons = 3;

   targetNameTag = 'Thundersword Belly';
   targetTypeTag = 'Turret';
};

datablock TurretImageData(BomberTurretBarrelPair)
{
   shapeFile      = "turret_belly_barrell.dts";
   mountPoint = 0;

   projectile = BomberFusionBolt;
   projectileType = LinearFlareProjectile;

   usesEnergy = true;
   useMountEnergy = true;
   fireEnergy = 6.25;
   minEnergy = 25.0;

   // Turret parameters
   activationMS      = 1000;
   deactivateDelayMS = 1500;
   thinkTimeMS       = 200;
   degPerSecTheta    = 360;
   degPerSecPhi      = 360;
   
   attackRadius      = 75;

   // State transitions
   stateName[0]                = "Activate";
   stateTransitionOnTimeout[0] = "WaitFire";
   stateTimeoutValue[0]        = 0.5;
   stateSequence[0]            = "Activate";
   stateSound[0]           = BomberTurretActivateSound;

   stateName[1] = "WaitFire";
   stateTransitionOnTriggerDown[1] = "InitFire";
   stateTransitionOnNoAmmo[1]      = "NoAmmo";
//   stateSound[1]                   = BomberTurretIdleSound;

   stateName[2] = "InitFire";
   stateWaitForTimeout[2]        = false;
   stateTransitionOnTimeout[2]   = "Fire";
 
   stateName[3]                = "Fire";
   stateTransitionOnTimeout[3] = "Reload";
   stateTimeoutValue[3]        = 0.35;
   stateFire[3]                = true;
   stateRecoil[3]              = LightRecoil;
   stateAllowImageChange[3]    = false;
   stateSequence[3]            = "Fire";
   stateScript[3]              = "onFire";
   stateSound[3]               = BomberTurretFireSound;

   stateName[4]                  = "Reload";
   stateTimeoutValue[4]          = 0.05;
   stateAllowImageChange[4]      = false;
   stateSequence[4]              = "Reload";
   stateTransitionOnTimeout[4]   = "WaitFire";
   stateTransitionOnNotLoaded[4] = "NoAmmo";
//   stateSound[4]                 = BomberTurretReloadSound;

   stateName[5]                    = "NoAmmo";
   stateTransitionOnAmmo[5]        = "Reload";
   stateSequence[5]                = "NoAmmo";
   stateTransitionOnTriggerDown[5] = "DryFire";

   stateName[6]                = "DryFire";
   stateSound[6]               = BomberTurretDryFireSound;
   stateTimeoutValue[6]        = 1.5;
   stateTransitionOnTimeout[6] = "NoAmmo";
   
};

datablock TurretImageData(BomberTurretBarrel) : BomberTurretBarrelPair 
{
   shapeFile      = "turret_belly_barrelr.dts";
   mountPoint = 1;
   stateScript[2]           = "onTriggerDown";
   stateScript[4]           = "onTriggerUp";
};

datablock TurretImageData(AIAimingTurretBarrel) 
{
   shapeFile = "turret_muzzlepoint.dts";
   mountPoint = 3;

   projectile = BomberFusionBolt;

   // Turret parameters
   activationMS      = 1000;
   deactivateDelayMS = 1500;
   thinkTimeMS       = 200;
   degPerSecTheta    = 500;
   degPerSecPhi      = 800;
   
   attackRadius      = 75;
};

//-------------------------------------
// BOMBER BOMB PROJECTILE
//-------------------------------------

datablock BombProjectileData(BomberBomb)
{
   projectileShapeName = "bomb.dts";
   emitterDelay        = -1;
   directDamage        = 0.0;
   hasDamageRadius     = true;
   indirectDamage      = 1.1;
   damageRadius        = 30;
   radiusDamageType    = $DamageType::BomberBombs;
   kickBackStrength    = 2500;

   explosion           = "VehicleBombExplosion";
   velInheritFactor    = 1.0;

   grenadeElasticity = 0.25;
   grenadeFriction   = 0.4;
   armingDelayMS     = 2000;
   muzzleVelocity    = 0.1;
   drag              = 0.3;

   minRotSpeed       = "60.0 0.0 0.0";
   maxRotSpeed       = "80.0 0.0 0.0";
   scale             = "1.0 1.0 1.0";
   
   sound = BomberBombProjectileSound;
};

//-------------------------------------
// BOMBER BOMB CHARACTERISTICS
//-------------------------------------

datablock ItemData(BombAmmo)
{
   className = Ammo;
   catagory = "Ammo";
   shapeFile = "repair_kit.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   computeCRC = true;
};

datablock StaticShapeData(DropBombs)
{
   catagory       = "Turrets";
   shapeFile      = "bombers_eye.dts";
   maxDamage     = 1.0;
   disabledLevel  = 0.6;
   destroyedLevel = 0.8;
};

datablock ShapeBaseImageData(BomberBombPairImage)
{
   className = WeaponImage;
   shapeFile = "turret_muzzlepoint.dts";
//   ammo = BombAmmo;
   offset = "2 -4 -0.5";
   mountPoint = 10;

   projectile = BomberBomb;
   projectileType = BombProjectile;
   usesEnergy = true;
   useMountEnergy = true;

   fireEnergy = 37.50;
   minEnergy = 37.50;

   stateName[0] = "Activate";
//   stateTransitionOnTimeout[0] = "ActivateReady";
   stateTransitionOnTimeout[0] = "WaitFire";
   stateTimeoutValue[0] = 0.5;
   stateSequence[0] = "Activate";
//   stateSound[0] = MortarSwitchSound;

   stateName[1] = "ActivateReady";
   stateTransitionOnLoaded[1] = "WaitFire";
//   stateTransitionOnNoAmmo[1] = "NoAmmo";

   stateName[2] = "WaitFire";
//   stateSound[2] = BomberBombIdleSound;
   stateTransitionOnNoAmmo[2] = "NoAmmo";
   stateTransitionOnTriggerDown[2] = "InitFire";
      
   stateName[3] = "InitFire";
   stateWaitForTimeout[3]        = false;
   stateTransitionOnTimeout[3]   = "Fire";
   
   stateName[4] = "Fire";
   stateTransitionOnTimeout[4] = "Reload";
   stateTimeoutValue[4] = 0.35;
   stateFire[4] = true;
   stateRecoil[4] = LightRecoil;
   stateAllowImageChange[4] = false;
   stateScript[4] = "onFire";
   stateSound[4] = BomberBombFireSound;

   stateName[5] = "Reload";
   stateTransitionOnNoAmmo[5] = "NoAmmo";
   stateTransitionOnTimeout[5] = "WaitFire";
   stateTimeoutValue[5] = 0.5;
   stateAllowImageChange[5] = false;
   stateSequence[5] = "Reload";
//   stateSound[5] = BomberBombReloadSound;
   
   stateName[6] = "NoAmmo";
   stateTransitionOnAmmo[6] = "Reload";
   stateSequence[6] = "NoAmmo";
   stateTransitionOnTriggerDown[6] = "DryFire";

   stateName[7]       = "DryFire";
   stateSound[7]      = BomberBombDryFireSound;
   stateTimeoutValue[7]        = 1.5;
   stateTransitionOnTimeout[7] = "NoAmmo";
   
};

datablock ShapeBaseImageData(BomberBombImage) : BomberBombPairImage
{
   offset = "-2 -4 -0.5";
   stateScript[3]           = "onTriggerDown";
   stateScript[5]           = "onTriggerUp";
};

//**************************************************************
// WEAPONS SPECIAL EFFECTS
//**************************************************************

//-------------------------------------
// BOMBER BELLY TURRET GUN (explosion)
//-------------------------------------

datablock ParticleData(FusionExplosionParticle)
{
   dragCoefficient      = 2;
   gravityCoefficient   = 0.2;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 750;
   lifetimeVarianceMS   = 150;
   textureName          = "particleTest";
   colors[0]     = "0.56 0.36 0.26 1.0";
   colors[1]     = "0.56 0.36 0.26 0.0";
   sizes[0]      = 1;
   sizes[1]      = 2;
};

datablock ParticleEmitterData(FusionExplosionEmitter)
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 0;
   ejectionVelocity = 12;
   velocityVariance = 1.75;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 60;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   particles = "FusionExplosionParticle";
};

datablock ExplosionData(FusionBoltExplosion)
{
   explosionShape = "effect_plasma_explosion.dts";
   soundProfile   = FusionExpSound;
   particleEmitter = FusionExplosionEmitter;
   particleDensity = 250;
   particleRadius = 1.25;
   faceViewer = true;
};
                                    
//--------------------------------------------------------------------------
// BOMBER TARGETING LASER
//--------------------------------------------------------------------------

datablock AudioProfile(BomberTargetingSwitchSound)
{
   filename    = "fx/weapons/generic_switch.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(BomberTargetingPaintSound)
{
   filename    = "fx/weapons/targetinglaser_paint.wav";
   description = CloseLooping3d;
   preload = true;
};

//--------------------------------------
// BOMBER TARGETING PROJECTILE
//--------------------------------------
datablock TargetProjectileData(BomberTargeter)
{
   directDamage         = 0.0;
   hasDamageRadius      = false;
   indirectDamage       = 0.0;
   damageRadius         = 0.0;
   velInheritFactor     = 1.0;

   maxRifleRange        = 1000;
   beamColor            = "0.1 1.0 0.1";
                        
   startBeamWidth       = 0.20;
   pulseBeamWidth       = 0.15;
   beamFlareAngle       = 3.0;
   minFlareSize         = 0.0;
   maxFlareSize         = 400.0;
   pulseSpeed           = 6.0;
   pulseLength          = 0.150;

   textureName[0]       = "special/nonlingradient";
   textureName[1]       = "special/flare";
   textureName[2]       = "special/pulse";
   textureName[3]      	= "special/expFlare";
   beacon               = true;
   beaconType           = vehicle;
};

//-------------------------------------
// BOMBER TARGETING CHARACTERISTICS
//-------------------------------------
datablock ShapeBaseImageData(BomberTargetingImage)
{
   className = WeaponImage;

   shapeFile = "turret_muzzlepoint.dts";
   offset = "0 -0.04 -0.01";
   mountPoint = 2;

   projectile = BomberTargeter;
   projectileType = TargetProjectile;
   deleteLastProjectile = true;

   usesEnergy = true;
   minEnergy = 3;

   stateName[0]                = "Activate";
   stateSequence[0]            = "Activate";
   stateSound[0]               = BomberTargetingSwitchSound;
   stateTimeoutValue[0]        = 0.5;
   stateTransitionOnTimeout[0] = "ActivateReady";

   stateName[1]               = "ActivateReady";
   stateTransitionOnAmmo[1]   = "Ready";
   stateTransitionOnNoAmmo[1] = "NoAmmo";

   stateName[2]                    = "Ready";
   stateTransitionOnNoAmmo[2]      = "NoAmmo";
   stateTransitionOnTriggerDown[2] = "Fire";

   stateName[3]                  = "Fire";
   stateEnergyDrain[3]           = 3;
   stateFire[3]                  = true;
   stateAllowImageChange[3]      = false;
   stateScript[3]                = "onFire";
   stateTransitionOnTriggerUp[3] = "Deconstruction";
   stateTransitionOnNoAmmo[3]    = "Deconstruction";
   stateSound[3] = BomberTargetingPaintSound;

   stateName[4]             = "NoAmmo";
   stateTransitionOnAmmo[4] = "Ready";

   stateName[5]               = "Deconstruction";
// Deconstruct is now being called from weapTurretCode.cs
//   stateScript[5]             = "deconstruct";
   stateTransitionOnTimeout[5] = "ActivateReady";
   stateTimeoutValue[5]        = 0.05;
};

function BomberTargetingImage::onFire(%data,%obj,%slot)
{
   %bomber = %obj.getObjectMount();
   if(%bomber.beacon)
   {
      %bomber.beacon.delete();
      %bomber.beacon = "";
   }   
   %p = Parent::onFire(%data, %obj, %slot);
   %p.setTarget(%obj.team);
}

function BomberTargetingImage::deconstruct(%data, %obj, %slot)
{
   %pos = %obj.lastProjectile.getTargetPoint();
   %bomber = %obj.getObjectMount();
   
   if(%bomber.beacon)
   {
      %bomber.beacon.delete();
      %bomber.beacon = "";
   }
   %bomber.beacon = new ScopeAlwaysShape() {
      dataBlock = "BomberBeacon";
      position = %pos;
   };

   %bomber.beacon.playThread($AmbientThread, "ambient");
   %bomber.beacon.team = %bomber.team;
   %bomber.beacon.sourceObject = %bomber;

   // give it a team target
   %bomber.beacon.setTarget(%bomber.team);                  
   MissionCleanup.add(%bomber.beacon);
   
   Parent::deconstruct(%data, %obj, %slot);
}

//-------------------------------------
// BOMBER BEACON
//-------------------------------------
datablock StaticShapeData(BomberBeacon)
{
   shapeFile = "turret_muzzlepoint.dts";
   beacon = true;
   targetNameTag = 'beacon';
   beaconType           = vehicle;
   isInvincible = true;
   
   dynamicType = $TypeMasks::SensorObjectType;
};

