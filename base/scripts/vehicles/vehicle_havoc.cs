//**************************************************************
// HAVOC HEAVY TRANSPORT FLIER
//**************************************************************
//**************************************************************
// SOUNDS
//**************************************************************

datablock AudioProfile(HAPCFlyerEngineSound)
{
   filename    = "fx/vehicles/htransport_thrust.wav";
   description = AudioDefaultLooping3d;
};

datablock AudioProfile(HAPCFlyerThrustSound)
{
   filename    = "fx/vehicles/htransport_boost.wav";
   description = AudioDefaultLooping3d;
};

//**************************************************************
// VEHICLE CHARACTERISTICS
//**************************************************************

datablock FlyingVehicleData(HAPCFlyer) : AirVehicleDamageProfile
{
   spawnOffset = "0 0 6";

   catagory = "Vehicles";
   shapeFile = "vehicle_air_hapc.dts";
   multipassenger = true;
   computeCRC = true;


   debrisShapeName = "vehicle_air_hapc_debris.dts";
   debris = ShapeDebris;

   drag = 0.2;
   density = 1.0;

   mountPose[0] = sitting;
//   mountPose[1] = sitting;
   numMountPoints = 6;
   isProtectedMountPoint[0] = true;
   isProtectedMountPoint[1] = true;
   isProtectedMountPoint[2] = true;
   isProtectedMountPoint[3] = true;
   isProtectedMountPoint[4] = true;
   isProtectedMountPoint[5] = true;

   cameraMaxDist = 17;
   cameraOffset = 2;
   cameraLag = 8.5;
   explosion = LargeAirVehicleExplosion;

   maxDamage = 3.50;
   destroyedLevel = 3.50;

   isShielded = true;
   rechargeRate = 0.8;
   energyPerDamagePoint = 200;
   maxEnergy = 550;
   minDrag = 100;                // Linear Drag
   rotationalDrag = 2700;        // Anguler Drag

   // Auto stabilize speed
   maxAutoSpeed = 10;
   autoAngularForce = 3000;      // Angular stabilizer force
   autoLinearForce = 450;        // Linear stabilzer force
   autoInputDamping = 0.95;      // 
                                                        
   // Maneuvering
   maxSteeringAngle = 8;
   horizontalSurfaceForce = 10;  // Horizontal center "wing"
   verticalSurfaceForce = 10;    // Vertical center "wing"
   maneuveringForce = 6000;      // Horizontal jets
   steeringForce = 1000;          // Steering jets
   steeringRollForce = 400;      // Steering jets
   rollForce = 12;               // Auto-roll
   hoverHeight = 8;         // Height off the ground at rest
   createHoverHeight = 6;   // Height off the ground when created

   // Turbo Jet
   jetForce = 5000;
   minJetEnergy = 55;
   jetEnergyDrain = 2.8;
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
   mass = 550;
   bodyFriction = 0;
   bodyRestitution = 0.3;
   minRollSpeed = 0;
   minImpactSpeed = 8;      // If hit ground at speed above this then it's an impact. Meters/second
   softImpactSpeed = 10;       // Sound hooks. This is the soft hit.
   hardImpactSpeed = 25;    // Sound hooks. This is the hard hit.
   speedDamageScale = 0.03;

   collDamageThresholdVel = 20.0;
   collDamageMultiplier   = 0.03;

   //
   minTrailSpeed = 15;
   trailEmitter = ContrailEmitter;
   forwardJetEmitter = FlyerJetEmitter;
   downJetEmitter = FlyerJetEmitter;

   //
   jetSound = HAPCFlyerThrustSound;
   engineSound = HAPCFlyerEngineSound;
   softImpactSound = SoftImpactSound;
   hardImpactSound = HardImpactSound;
   //wheelImpactSound = WheelImpactSound;

   //
   softSplashSoundVelocity = 15.0; 
   mediumSplashSoundVelocity = 30.0;   
   hardSplashSoundVelocity = 60.0;   
   exitSplashSoundVelocity = 20.0;
   
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
   cmdIcon = CMDFlyingHAPCIcon;
   cmdMiniIconName = "commander/MiniIcons/com_hapc_grey";
   targetNameTag = 'Havoc';
   targetTypeTag = 'Flying Vehicle';
   sensorData = VehiclePulseSensor;

   checkRadius = 7.8115;
   observeParameters = "1 15 15";
};

function HAPCFlyer::hasDismountOverrides(%data, %obj)
{
   return true;
}

function HAPCFlyer::getDismountOverride(%data, %obj, %mounted)
{
   %node = -1;
   for (%i = 0; %i < %data.numMountPoints; %i++)
   {
      if (%obj.getMountNodeObject(%i) == %mounted)
      {
         %node = %i;
         break;
      }
   }
   if (%node == -1)
   {
      warning("Couldn't find object mount point");
      return "0 0 1";
   }

   if (%node == 3 || %node == 2)
   {
      return "-1 0 1";
   }
   else if (%node == 5 || %node == 4)
   {
      return "1 0 1";
   }
   else
   {
      return "0 0 1";
   }
}

//**************************************************************
// WEAPONS
//**************************************************************



