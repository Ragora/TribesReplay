//**************************************************************
// WILDCAT GRAV CYCLE
//**************************************************************
//**************************************************************
// SOUNDS
//**************************************************************

datablock AudioProfile(ScoutSqueelSound)
{
   filename    = "fx/vehicles/outrider_skid.wav";
   description = ClosestLooping3d;
   preload = true;
};

// Scout
datablock AudioProfile(ScoutEngineSound)
{
   filename    = "fx/vehicles/outrider_engine.wav";
   description = AudioDefaultLooping3d;
   preload = true;
};

datablock AudioProfile(ScoutThrustSound)
{
   filename    = "fx/vehicles/outrider_boost.wav";
   description = AudioDefaultLooping3d;
   preload = true;
};

//**************************************************************
// VEHICLE CHARACTERISTICS
//**************************************************************

datablock HoverVehicleData(ScoutVehicle) : GravCycleDamageProfile
{
   spawnOffset = "0 0 1";

   floatingGravMag = 2.5;

   catagory = "Vehicles";
   shapeFile = "vehicle_grav_scout.dts";
   computeCRC = true;

   debrisShapeName = "vehicle_grav_scout_debris.dts";
   debris = ShapeDebris;

   drag = 0.0;
   density = 0.9;

   mountPose[0] = scoutRoot;
   cameraMaxDist = 5.0;
   cameraOffset = 0.7;
   cameraLag = 0.5;
   numMountPoints = 1;
   isProtectedMountPoint[0] = true;
   explosion = VehicleExplosion;

   lightOnly = 1;

   maxDamage = 0.60;
   destroyedLevel = 0.60;

   isShielded = true;
   rechargeRate = 0.7;
   energyPerDamagePoint = 75;
   maxEnergy = 150;
   minJetEnergy = 15;
   jetEnergyDrain = 1.3;

   collDamageThresholdVel = 20.0;
   collDamageMultiplier   = 0.01;

   // Rigid Body
   mass = 400;
   bodyFriction = 0.1;
   bodyRestitution = 0.5;  
   minImpactSpeed = 10;
   softImpactSpeed = 10;       // Play SoftImpact Sound
   hardImpactSpeed = 25;      // Play HardImpact Sound
   speedDamageScale = 0.008;

   dragForce            = 25 / 45.0;
   vertFactor           = 0.0;
   floatingThrustFactor = 0.35;

   mainThrustForce    = 25;
   reverseThrustForce = 10;
   strafeThrustForce  = 8;
   turboFactor        = 2.0;

   brakingForce = 25;
   brakingActivationSpeed = 4;

   stabLenMin = 2.25;
   stabLenMax = 3.25;
   stabSpringConstant  = 30;
   stabDampingConstant = 12;

   gyroDrag = 16;
   normalForce = 30;
   restorativeForce = 20;
   steeringForce = 25;
   rollForce  = 15;
   pitchForce = 7;

   dustEmitter = VehicleLiftoffDustEmitter;
   triggerDustHeight = 2.5;
   dustHeight = 1.0;
   dustTrailEmitter = TireEmitter;
   dustTrailOffset = "0.0 -1.0 0.5";
   triggerTrailHeight = 3.6;
   dustTrailFreqMod = 15.0;

   jetSound         = ScoutSqueelSound;
   engineSound      = ScoutEngineSound;
   floatSound       = ScoutThrustSound;
   softImpactSound  = GravSoftImpactSound;
   hardImpactSound  = HardImpactSound;
   wheelImpactSound = WheelImpactSound;

   //
   softSplashSoundVelocity = 20.0; 
   mediumSplashSoundVelocity = 50.0;   
   hardSplashSoundVelocity = 100.0;   
   exitSplashSoundVelocity = 20.0;
   
   exitingWater      = VehicleExitWaterSoftSound;
   impactWaterEasy   = VehicleImpactWaterSoftSound;
   impactWaterMedium = VehicleImpactWaterSoftSound;
   impactWaterHard   = VehicleImpactWaterMediumSound;
   waterWakeSound    = VehicleWakeSoftSplashSound; 

   minMountDist = 4;

   damageEmitter[0] = SmallLightDamageSmoke;
   damageEmitter[1] = SmallHeavyDamageSmoke;
   damageEmitter[2] = DamageBubbles;
   damageEmitterOffset[0] = "0.0 -1.5 0.5 ";
   damageLevelTolerance[0] = 0.3;
   damageLevelTolerance[1] = 0.7;
   numDmgEmitterAreas = 1;

   splashEmitter[0] = VehicleFoamDropletsEmitter;
   splashEmitter[1] = VehicleFoamEmitter;

   shieldImpact = VehicleShieldImpact;
   
   forwardJetEmitter = WildcatJetEmitter;

   cmdCategory = Tactical;
   cmdIcon = CMDHoverScoutIcon;
   cmdMiniIconName = "commander/MiniIcons/com_landscout_grey";
   targetNameTag = 'WildCat';
   targetTypeTag = 'Hover Vehicle';
   sensorData = VehiclePulseSensor;

   checkRadius = 1.7785;
   observeParameters = "1 10 10";
};

//**************************************************************
// WEAPONS
//**************************************************************

