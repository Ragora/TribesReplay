// deployable objects script
//
// remote pulse sensor, remote motion sensor, remote turrets (indoor
// and outdoor), remote inventory station, remote ammo station
// Note: cameras are treated as grenades, not "regular" deployables

$TurretIndoorSpaceRadius  = 20;  // deployed turrets must be this many meters apart
$TurretIndoorSphereRadius = 50;  // radius for turret frequency check
$TurretIndoorMaxPerSphere = 4;   // # of turrets allowed in above radius

$TurretOutdoorSpaceRadius  = 25;  // deployed turrets must be this many meters apart
$TurretOutdoorSphereRadius = 60;  // radius for turret frequency check
$TurretOutdoorMaxPerSphere = 4;   // # of turrets allowed in above radius

$TeamDeployableMax[InventoryDeployable]      = 5;
$TeamDeployableMax[TurretIndoorDeployable]   = 10;
$TeamDeployableMax[TurretOutdoorDeployable]  = 10;
$TeamDeployableMax[PulseSensorDeployable]    = 15;
$TeamDeployableMax[MotionSensorDeployable]   = 15;

$TeamDeployableMin[TurretIndoorDeployable] = 4;
$TeamDeployableMin[TurretOutdoorDeployable] = 4;

$NotDeployableReason::None                      =  0;
$NotDeployableReason::MaxDeployed               =  1;
$NotDeployableReason::NoSurfaceFound            =  2;
$NotDeployableReason::SlopeTooGreat             =  3;
$NotDeployableReason::SelfTooClose              =  4;
$NotDeployableReason::ObjectTooClose            =  5;
$NotDeployableReason::NoTerrainFound            =  6;
$NotDeployableReason::NoInteriorFound           =  7;
$NotDeployableReason::TurretTooClose            =  8;
$NotDeployableReason::TurretSaturation          =  9;

$MinDeployableDistance                       =   0.5;
$MaxDeployableDistance                       =  4.0;  //meters from body


// --------------------------------------------
// sound datablocks
// --------------------------------------------

datablock AudioProfile(TurretDeploySound)
{
   fileName = "fx/packs/turret_place.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(SensorDeploySound)
{
   fileName = "fx/powered/sensor_activate.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(MotionSensorDeploySound)
{
   fileName = "fx/powered/motion_sensor_activate.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(StationDeploySound)
{
   fileName = "fx/packs/inventory_deploy.wav";
   description = AudioClose3d;
   preload = true;
};

// --------------------------------------------
// deployable debris definition

datablock DebrisData( DeployableDebris )
{
   explodeOnMaxBounce = false;

   elasticity = 0.40;
   friction = 0.5;

   lifetime = 17.0;
   lifetimeVariance = 0.0;

   minSpinSpeed = 60;
   maxSpinSpeed = 600;

   numBounces = 10;
   bounceVariance = 0;

   staticOnMaxBounce = true;

   useRadiusMass = true;
   baseRadius = 0.2;

   velocity = 5.0;
   velocityVariance = 2.5;
   
};             


// --------------------------------------------
// deployable inventory station

datablock StaticShapeData(DeployedStationInventory) : StaticShapeDamageProfile
{
   className = Station;
   shapeFile = "deploy_inventory.dts";
   maxDamage = 0.70;
   destroyedLevel = 0.70;
   disabledLevel = 0.42;
   explosion = DeployablesExplosion;

   dynamicType = $TypeMasks::StationObjectType;
   isShielded = true;
   energyPerDamagePoint = 110;
   maxEnergy = 50;
   rechargeRate = 0.20;
   renderWhenDestroyed = false;
   doesRepair = true;

   cmdCategory = "DSupport";
   cmdIcon = CMDStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_inventory_grey";
   targetNameTag = 'Deployable';
   targetTypeTag = 'Station';

   debrisShapeName = "debris_generic_small.dts";
   debris = DeployableDebris;
};

datablock ShapeBaseImageData(InventoryDeployableImage)
{
   mass = 15;
   emap = true;

   shapeFile = "pack_deploy_inventory.dts";
   item = InventoryDeployable;
   mountPoint = 1;
   offset = "0 0 0";
   deployed = DeployedStationInventory;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "Idle";

   isLarge = true;
   maxDepSlope = 30;
   deploySound = StationDeploySound;
};

datablock ItemData(InventoryDeployable)
{
   className = Pack;
   catagory = "Deployables";
   shapeFile = "pack_deploy_inventory.dts";
   mass = 3.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   rotate = false;
   image = "InventoryDeployableImage";
   pickUpName = "an inventory pack";

   computeCRC = true;
   emap = true;

};

// --------------------------------------------
// deployable motion sensor

datablock SensorData(DeployMotionSensorObj)
{
   detects = true;
   detectsUsingLOS = true;
   detectsActiveJammed = false;
   detectsPassiveJammed = true;
   detectsCloaked = true;
   detectionPings = false;
   detectMinVelocity = 2;
   detectRadius = 60;
};

datablock StaticShapeData(DeployedMotionSensor) : StaticShapeDamageProfile
{
   className = Sensor;
   shapeFile = "deploy_sensor_motion.dts";
   maxDamage = 0.6;
   destroyedLevel = 0.6;
   disabledLevel = 0.4;
   explosion = DeployablesExplosion;
   dynamicType = $TypeMasks::SensorObjectType;

   cmdCategory = "DSupport";
   cmdIcon = CMDSensorIcon;
   cmdMiniIconName = "commander/MiniIcons/com_deploymotionsensor";
   targetNameTag = 'Deployable Motion';
   targetTypeTag = 'Sensor';
   sensorData = DeployMotionSensorObj;
   sensorRadius = DeployMotionSensorObj.detectRadius;
   sensorColor = "9 136 255";
   deployAmbientThread = true;

   debrisShapeName = "debris_generic_small.dts";
   debris = DeployableDebris;
};

datablock ShapeBaseImageData(MotionSensorDeployableImage)
{
   shapeFile = "pack_deploy_sensor_motion.dts";
   item = MotionSensorDeployable;
   mountPoint = 1;
   offset = "0 0 0";
   deployed = DeployedMotionSensor;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "Idle";

   maxDepSlope = 360;
   deploySound = MotionSensorDeploySound;
   emap = true;
};

datablock ItemData(MotionSensorDeployable)
{
   className = Pack;
   catagory = "Deployables";
   shapeFile = "pack_deploy_sensor_motion.dts";
   mass = 2.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   rotate = false;
   image = "MotionSensorDeployableImage";
   pickUpName = "a motion sensor pack";

   computeCRC = true;
   emap = true;

   //maxSensors = 3;
   maxSensors = 2;
};

// --------------------------------------------
// deployable pulse sensor

datablock SensorData(DeployPulseSensorObj)
{
   detects = true;
   detectsUsingLOS = true;
   detectsPassiveJammed = false;
   detectsCloaked = false;
   detectionPings = true;
   detectRadius = 120;
};

datablock StaticShapeData(DeployedPulseSensor) : StaticShapeDamageProfile
{
   className = Sensor;
   shapeFile = "deploy_sensor_pulse.dts";
   maxDamage = 0.6;
   destroyedLevel = 0.6;
   disabledLevel = 0.4;
   explosion = DeployablesExplosion;
   dynamicType = $TypeMasks::SensorObjectType;

   cmdCategory = "DSupport";
   cmdIcon = CMDSensorIcon;
   cmdMiniIconName = "commander/MiniIcons/com_deploypulsesensor";
   targetNameTag = 'Deployable';
   targetTypeTag = 'Pulse Sensor';
   sensorData = DeployPulseSensorObj;
   sensorRadius = DeployPulseSensorObj.detectRadius;
   sensorColor = "255 194 9";
   deployAmbientThread = true;

   debrisShapeName = "debris_generic_small.dts";
   debris = DeployableDebris;
};

datablock ShapeBaseImageData(PulseSensorDeployableImage)
{
   shapeFile = "pack_deploy_sensor_pulse.dts";
   item = PulseSensorDeployable;
   mountPoint = 1;
   offset = "0 0 0";
   deployed = DeployedPulseSensor;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "Idle";
   deploySound = SensorDeploySound;

   maxDepSlope = 40;
   emap = true;
};

datablock ItemData(PulseSensorDeployable)
{
   className = Pack;
   catagory = "Deployables";
   shapeFile = "pack_deploy_sensor_pulse.dts";
   mass = 2.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   rotate = false;
   image = "PulseSensorDeployableImage";
   pickUpName = "a pulse sensor pack";

   computeCRC = true;
   emap = true;

   maxSensors = 2;
};

// --------------------------------------------
// deployable outdoor turret

datablock ShapeBaseImageData(TurretOutdoorDeployableImage)
{
   mass = 15;

   shapeFile = "pack_deploy_turreto.dts";
   item = TurretOutdoorDeployable;
   mountPoint = 1;
   offset = "0 0 0";
   deployed = TurretDeployedOutdoor;

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "Idle";

   maxDamage = 4.5;
   destroyedLevel = 4.5;
   disabledLevel = 4.0;

   isLarge = true;
   emap = true;

   maxDepSlope = 40;
   deploySound = TurretDeploySound;
};

datablock ItemData(TurretOutdoorDeployable)
{
   className = Pack;
   catagory = "Deployables";
   shapeFile = "pack_deploy_turreto.dts";
   mass = 3.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   rotate = false;
   image = "TurretOutdoorDeployableImage";
   pickUpName = "an outdoor turret pack";

   computeCRC = true;
   emap = true;

};

// --------------------------------------------
// deployable indoor turret (3 varieties -- floor, wall and ceiling)

datablock ShapeBaseImageData(TurretIndoorDeployableImage)
{
   mass = 15;

   shapeFile = "pack_deploy_turreti.dts";
   item = TurretIndoorDeployable;
   mountPoint = 1;
   offset = "0 0 0";

   stateName[0] = "Idle";
   stateTransitionOnTriggerDown[0] = "Activate";

   stateName[1] = "Activate";
   stateScript[1] = "onActivate";
   stateTransitionOnTriggerUp[1] = "Idle";

   isLarge = true;
   emap = true;

   maxDepSlope = 360;
   deploySound = TurretDeploySound;
};

datablock ItemData(TurretIndoorDeployable)
{
   className = Pack;
   catagory = "Deployables";
   shapeFile = "pack_deploy_turreti.dts";
   mass = 3.0;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 1;
   rotate = false;
   image = "TurretIndoorDeployableImage";
   pickUpName = "an indoor turret pack";

   computeCRC = true;
   emap = true;

};

// --------------------------------------------
// miscellaneous yet handy functions

function posFromTransform(%transform)
{
   // the first three words of an object's transform are the object's position
   %position = getWord(%transform, 0) @ " " @ getWord(%transform, 1) @ " " @ getWord(%transform, 2);
   return %position;
}

function rotFromTransform(%transform)
{
   // the last four words of an object's transform are the object's rotation
   %rotation = getWord(%transform, 3) @ " " @ getWord(%transform, 4) @ " " @ getWord(%transform, 5) @ " " @ getWord(%transform, 6);
   return %rotation;
}

function posFromRaycast(%transform)
{
   // the 2nd, 3rd, and 4th words returned from a successful raycast call are the position of the point
   %position = getWord(%transform, 1) @ " " @ getWord(%transform, 2) @ " " @ getWord(%transform, 3);
   return %position;
}

function normalFromRaycast(%transform)
{
   // the 5th, 6th and 7th words returned from a successful raycast call are the normal of the surface
   %norm = getWord(%transform, 4) @ " " @ getWord(%transform, 5) @ " " @ getWord(%transform, 6);
   return %norm;
}

function addToDeployGroup(%object)
{
   // all deployables should go into a special group for AI purposes
   %depGroup = nameToID("MissionCleanup/Deployables");
   if(%depGroup <= 0) {
      %depGroup = new SimGroup("Deployables");
      MissionCleanup.add(%depGroup);
   }
   %depGroup.add(%object);
}

function Deployables::searchView(%obj, %searchRange, %mask)
{
   // get the eye vector and eye transform of the player
   %eyeVec   = %obj.getEyeVector();
   %eyeTrans = %obj.getEyeTransform();

   // extract the position of the player's camera from the eye transform (first 3 words)
   %eyePos = posFromTransform(%eyeTrans);

   // normalize the eye vector
   %nEyeVec = VectorNormalize(%eyeVec);

   // scale (lengthen) the normalized eye vector according to the search range
   %scEyeVec = VectorScale(%nEyeVec, %searchRange);

   // add the scaled & normalized eye vector to the position of the camera
   %eyeEnd = VectorAdd(%eyePos, %scEyeVec);

   // see if anything gets hit
   %searchResult = containerRayCast(%eyePos, %eyeEnd, %mask, 0);

   return %searchResult;
}

//-----------------------//
// Deployable Procedures //
//-----------------------//

//-------------------------------------------------
function ShapeBaseImageData::testMaxDeployed(%item, %plyr)
{
	if(%item.item $= TurretOutdoorDeployable || %item.item $= TurretIndoorDeployable)
		%itemCount = countTurretsAllowed(%item.item);
	else
		%itemCount = $TeamDeployableMax[%item.item];

   return $TeamDeployedCount[%plyr.team, %item.item] >= %itemCount;
}

//-------------------------------------------------
function ShapeBaseImageData::testNoSurfaceInRange(%item, %plyr)
{
   return ! Deployables::searchView(%plyr, $MaxDeployableDistance, $TypeMasks::TerrainObjectType | $TypeMasks::InteriorObjectType);
}

//-------------------------------------------------
function ShapeBaseImageData::testSlopeTooGreat(%item)
{
   if (%item.surface)
   {          
      return getTerrainAngle(%item.surfaceNrm) > %item.maxDepSlope;
   }   
}

//-------------------------------------------------
function ShapeBaseImageData::testSelfTooClose(%item, %plyr)
{
   InitContainerRadiusSearch(%item.surfacePt, $MinDeployableDistance, $TypeMasks::PlayerObjectType);

   return containerSearchNext() == %plyr; 
}

//-------------------------------------------------
function ShapeBaseImageData::testObjectTooClose(%item)
{
   return !ContainerBoxEmpty (
               $TypeMasks::VehicleObjectType     | $TypeMasks::MoveableObjectType   |
               $TypeMasks::StaticShapeObjectType | $TypeMasks::StaticTSObjectType   | 
               $TypeMasks::ForceFieldObjectType  | $TypeMasks::ItemObjectType       | 
               $TypeMasks::PlayerObjectType      | $TypeMasks::TurretObjectType, 
               %item.surfacePt, $MinDeployableDistance);
}


//-------------------------------------------------
function TurretOutdoorDeployableImage::testNoTerrainFound(%item)
{
      return %item.surface.getClassName() !$= TerrainBlock;
}

function ShapeBaseImageData::testNoTerrainFound(%item, %surface)
{
   //don't check this for non-Landspike turret deployables
}

//-------------------------------------------------
function TurretIndoorDeployableImage::testNoInteriorFound(%item)
{
   return %item.surface.getClassName() !$= InteriorInstance;
}

function ShapeBaseImageData::testNoInteriorFound(%item, %surface)
{
   //don't check this for non-Clasping turret deployables
}

//-------------------------------------------------
function TurretIndoorDeployableImage::testTurretTooClose(%item, %plyr)
{
   InitContainerRadiusSearch(%item.surfacePt, $TurretIndoorSpaceRadius, $TypeMasks::StaticShapeObjectType);

   // old function was only checking whether the first object found was a turret -- also wasn't checking
   // which team the object was on
   %turretInRange = false;
   while((%found = containerSearchNext()) != 0)
   {
      %foundName = %found.getDataBlock().getName();
      if((%foundname $= TurretDeployedFloorIndoor) || (%foundName $= TurretDeployedWallIndoor) || (%foundName $= TurretDeployedCeilingIndoor) || (%foundName $= TurretDeployedOutdoor))
         if (%found.team == %plyr.team)
         {
            %turretInRange = true;
            break;
         }
   }
   return %turretInRange;
}

function TurretOutdoorDeployableImage::testTurretTooClose(%item, %plyr)
{
   InitContainerRadiusSearch(%item.surfacePt, $TurretOutdoorSpaceRadius, $TypeMasks::StaticShapeObjectType);

   // old function was only checking whether the first object found was a turret -- also wasn't checking
   // which team the object was on
   %turretInRange = false;
   while((%found = containerSearchNext()) != 0)
   {
      %foundName = %found.getDataBlock().getName();
      if((%foundname $= TurretDeployedFloorIndoor) || (%foundName $= TurretDeployedWallIndoor) || (%foundName $= TurretDeployedCeilingIndoor) || (%foundName $= TurretDeployedOutdoor))
         if (%found.team == %plyr.team)
         {
            %turretInRange = true;
            break;
         }
   }
   return %turretInRange;
}

function ShapeBaseImageData::testTurretTooClose(%item, %plyr)
{
   //don't check this for non-turret deployables
}

//-------------------------------------------------
function TurretIndoorDeployableImage::testTurretSaturation(%item)
{
   %highestDensity = 0;
   InitContainerRadiusSearch(%item.surfacePt, $TurretIndoorSphereRadius, $TypeMasks::StaticShapeObjectType);
   %found = containerSearchNext();
   while(%found)
   {
      %foundName = %found.getDataBlock().getName();
      if ((%foundName $= TurretDeployedFloorIndoor) || (%foundName $= TurretDeployedWallIndoor) || (%foundName $= TurretDeployedCeilingIndoor) || (%foundName $= TurretDeployedOutdoor))
      {
           //found one
           %numTurretsNearby++;

       %nearbyDensity = testNearbyDensity(%found, $TurretIndoorSphereRadius);
       if (%nearbyDensity > %highestDensity)
          %highestDensity = %nearbyDensity;     
      }
     %found = containerSearchNext();
   }

   if (%numTurretsNearby > %highestDensity)
      %highestDensity = %numTurretsNearby;
   return %highestDensity > $TurretIndoorMaxPerSphere;
}

function TurretOutdoorDeployableImage::testTurretSaturation(%item)
{
   %highestDensity = 0;
   InitContainerRadiusSearch(%item.surfacePt, $TurretOutdoorSphereRadius, $TypeMasks::StaticShapeObjectType);
   %found = containerSearchNext();
   while(%found)
   {
      %foundName = %found.getDataBlock().getName();
      if ((%foundName $= TurretDeployedFloorIndoor) || (%foundName $= TurretDeployedWallIndoor) || (%foundName $= TurretDeployedCeilingIndoor) || (%foundName $= TurretDeployedOutdoor))
      {
           //found one
           %numTurretsNearby++;

       %nearbyDensity = testNearbyDensity(%found, $TurretOutdoorSphereRadius);
       if (%nearbyDensity > %highestDensity)
          %highestDensity = %nearbyDensity;     
      }
     %found = containerSearchNext();
   }

   if (%numTurretsNearby > %highestDensity)
      %highestDensity = %numTurretsNearby;
   return %highestDensity > $TurretOutdoorMaxPerSphere;
}

function ShapeBaseImageData::testTurretSaturation(%item, %surfacePt)
{
   //don't check this for non-turret deployables
}

function testNearbyDensity(%item, %radius)
{
   //this checks how many turrets are in adjacent spheres in case placing a new one overloads them.
   %surfacePt = posFromTransform(%item.getTransform());
   %turretCount = 0;

   InitContainerRadiusSearch(%surfacePt, %radius, $TypeMasks::StaticShapeObjectType);
   %found = containerSearchNext();
   while(%found)
   {
      %foundName = %found.getDataBlock().getName();
      if ((%foundName $= TurretDeployedFloorIndoor) || (%foundName $= TurretDeployedWallIndoor) || (%foundName $= TurretDeployedCeilingIndoor) || (%foundName $= TurretDeployedOutdoor))
         %turretCount++;       
     %found = containerSearchNext();      
   }
   return %turretCount;
}

//-------------------------------------------------
//if this function, or any of the included tests are changed, those changes need to be reflected in function:
//AIODeployEquipment::weight(%this, %client, %level), found in aiObjectives.cs  --tinman
function ShapeBaseImageData::testInvalidDeployConditions(%item, %plyr, %slot)
{
   cancel(%plyr.deployCheckThread);
   %disqualified = $NotDeployableReason::None;  //default
   
   %surface = Deployables::searchView(%plyr,
                                      $MaxDeployableDistance,
                                      ($TypeMasks::TerrainObjectType |
                                       $TypeMasks::InteriorObjectType));
   if (%surface)  
   {  
      %surfacePt  = posFromRaycast(%surface);
      %surfaceNrm = normalFromRaycast(%surface);

      // Check that point to see if anything is objstructing it...
      %eyeTrans = %plyr.getEyeTransform();
      %eyePos   = posFromTransform(%eyeTrans);

      %searchResult = containerRayCast(%eyePos, %surfacePt, -1, %plyr);
      if (!%searchResult)
      {
         %item.surface = %surface;
         %item.surfacePt = %surfacePt;
         %item.surfaceNrm = %surfaceNrm;
      }
      else
      {
         if (%surfacePt $= posFromRaycast(%searchResult))
         {
            %item.surface = %surface;
            %item.surfacePt = %surfacePt;
            %item.surfaceNrm = %surfaceNrm;
         }
         else
         {
            // Don't set the item
            %disqualified = $NotDeployableReason::ObjectTooClose;
         }
      }
   }


   if (%item.testMaxDeployed(%plyr))
      %disqualified = $NotDeployableReason::MaxDeployed;

   else if (%item.testNoSurfaceInRange(%plyr))
      %disqualified = $NotDeployableReason::NoSurfaceFound;

   else if (%item.testNoTerrainFound(%surface))
      %disqualified = $NotDeployableReason::NoTerrainFound;

   else if (%item.testNoInteriorFound())
      %disqualified = $NotDeployableReason::NoInteriorFound;
   
   else if (%item.testSlopeTooGreat(%surface, %surfaceNrm))
      %disqualified = $NotDeployableReason::SlopeTooGreat;

   else if (%item.testSelfTooClose(%plyr, %surfacePt))
      %disqualified = $NotDeployableReason::SelfTooClose;

   else if (%item.testObjectTooClose(%surfacePt))
      %disqualified = $NotDeployableReason::ObjectTooClose;   

   else if (%item.testTurretTooClose(%plyr))
      %disqualified = $NotDeployableReason::TurretTooClose;

   else if (%item.testTurretSaturation())
      %disqualified = $NotDeployableReason::TurretSaturation;

      
   if (%plyr.getMountedImage($BackpackSlot) == %item)  //player still have the item?
   {
      if (%disqualified)
           activateDeploySensorRed(%plyr);
      else
         activateDeploySensorGrn(%plyr);      

      if (%plyr.client.deployPack == true)
         %item.attemptDeploy(%plyr, %slot, %disqualified);       
      else
         %plyr.deployCheckThread = %item.schedule(50, "testInvalidDeployConditions", %plyr, %slot); //update checks every 50 milliseconds
   }
   else
       deactivateDeploySensor(%plyr);
}

function ShapeBaseImageData::attemptDeploy(%item, %plyr, %slot, %disqualified)
{
   deactivateDeploySensor(%plyr);
   Deployables::displayErrorMsg(%item, %plyr, %slot, %disqualified);
}  

function activateDeploySensorRed(%pl)
{
   if(%pl.deploySensor !$= "red")
   {
      messageClient(%pl.client, 'msgDeploySensorRed', "");
      %pl.deploySensor = "red";
   }
}

function activateDeploySensorGrn(%pl)
{
   if(%pl.deploySensor !$= "green")
   {
      messageClient(%pl.client, 'msgDeploySensorGrn', "");
      %pl.deploySensor = "green";
   }
}  

function deactivateDeploySensor(%pl)
{
   if (%pl.deploySensor !$= "")
   {
      messageClient(%pl.client, 'msgDeploySensorOff', "");
      %pl.deploySensor = "";
   }
}
   
function Deployables::displayErrorMsg(%item, %plyr, %slot, %error)
{
   deactivateDeploySensor(%plyr);
   
   %errorSnd = '~wfx/misc/misc.error.wav';
   switch (%error)
   {
      case $NotDeployableReason::None:
         %item.onDeploy(%plyr, %slot);
         messageClient(%plyr.client, 'MsgTeamDeploySuccess', "");
         return;

      case $NotDeployableReason::NoSurfaceFound:
         %msg = '\c2Item must be placed within reach.%1';

      case $NotDeployableReason::MaxDeployed:
         %msg = '\c2Your team\'s control network has reached its capacity for this item.%1';

      case $NotDeployableReason::SlopeTooGreat:
         %msg = '\c2Surface is too steep to place this item on.%1';

      case $NotDeployableReason::SelfTooClose:
         %msg = '\c2You are too close to the surface you are trying to place the item on.%1';

      case $NotDeployableReason::ObjectTooClose:
         %msg = '\c2You cannot place this item so close to another object.%1';

      case $NotDeployableReason::NoTerrainFound:
         %msg = '\c2You must place this on outdoor terrain.%1';

      case $NotDeployableReason::NoInteriorFound:
         %msg = '\c2You must place this on a solid surface.%1';

      case $NotDeployableReason::TurretTooClose:
         %msg = '\c2Interference from a nearby turret prevents placement here.%1';

      case $NotDeployableReason::TurretSaturation:
         %msg = '\c2There are too many turrets nearby.%1';

      default:
         %msg = '\c2Deploy failed.';
   }
   messageClient(%plyr.client, 'MsgDeployFailed', %msg, %errorSnd);
}  

function ShapeBaseImageData::onActivate(%data, %obj, %slot)
{
   //Tinman - apparently, anything that uses the generic onActivate() method is a deployable.
   //repair packs, cloak packs, shield, etc...  all overload this method...
   %data.testInvalidDeployConditions(%obj, %slot);

   //whether the test passed or not, reset the image trigger (deployables don't have an on/off toggleable state)
   %obj.setImageTrigger(%slot, false);
}

function ShapeBaseImageData::onDeploy(%item, %plyr, %slot)
{
   if(%item.item $= "MotionSensorDeployable" || %item.item $= "PulseSensorDeployable")
   {
      %plyr.deploySensors--;
      %plyr.client.updateSensorPackText(%plyr.deploySensors);
      if(%plyr.deploySensors <= 0)
      {
         // take the deployable off the player's back and out of inventory
         %plyr.unmountImage(%slot);
         %plyr.decInventory(%item.item, 1);  
      }
   }
   else
   {
      // take the deployable off the player's back and out of inventory
      %plyr.unmountImage(%slot);
      %plyr.decInventory(%item.item, 1);  
   }
   
   // create the actual deployable
   %rot = %item.getInitialRotation(%plyr);
   if(%item.deployed.className $= "DeployedTurret")
      %className = "Turret";
   else
      %className = "StaticShape";

   %deplObj = new (%className)() {
      dataBlock = %item.deployed;
   };
   
   
   // set orientation
   if(%className $= "Turret")
      %deplObj.setDeployRotation(%item.surfacePt, %item.surfaceNrm);
   else
      %deplObj.setTransform(%item.surfacePt SPC %rot);
   
   // set team, owner, and handle
   %deplObj.team = %plyr.client.Team;
   %deplObj.owner = %plyr.client;

   // set the sensor group if it needs one
   if(%deplObj.getTarget() != -1)
      setTargetSensorGroup(%deplObj.getTarget(), %plyr.client.team);

   // place the deployable in the MissionCleanup/Deployables group (AI reasons)
   addToDeployGroup(%deplObj);

   //let the AI know as well...
   AIDeployObject(%plyr.client, %deplObj);

   // play the deploy sound
   serverPlay3D(%item.deploySound, %deplObj.getTransform());

   // increment the team count for this deployed object

   $TeamDeployedCount[%plyr.team, %item.item]++;
   %deplObj.deploy();
   return %deplObj;  
}

function ShapeBaseImageData::getInitialRotation(%item, %plyr)
{
   return rotFromTransform(%plyr.getTransform());
}

function MotionSensorDeployableImage::getInitialRotation(%item, %plyr)
{
   %rotAxis = vectorNormalize(vectorCross(%item.surfaceNrm, "0 0 1"));
   if (getWord(%item.surfaceNrm, 2) == 1 || getWord(%item.surfaceNrm, 2) == -1)
      %rotAxis = vectorNormalize(vectorCross(%item.surfaceNrm, "0 1 0"));     
   return %rotAxis SPC mACos(vectorDot(%item.surfaceNrm, "0 0 1"));
}

function MotionSensorDeployable::onPickup(%this, %pack, %player, %amount)
{
   // %this = Sensor pack datablock
   // %pack = Sensor pack object number
   // %player = player
   // %amount = amount picked up (1)

   if(%pack.sensors $= "")
   {
      // assume that this is a pack that has been placed in a mission
      // this case was handled in ::onInventory below (max sensors);
   }
   else
   {
      // find out how many sensor were in the pack
      %player.deploySensors = %pack.sensors;
      %player.client.updateSensorPackText(%player.deploySensors);
   }
}

function MotionSensorDeployable::onThrow(%this,%pack,%player)
{
   // %this = Sensor pack datablock
   // %pack = Sensor pack object number
   // %player = player

   %player.throwSensorPack = 1;
   %pack.sensors = %player.deploySensors;
   %player.deploySensors = 0;
   %player.client.updateSensorPackText(%player.deploySensors);
   // do the normal ItemData::onThrow stuff -- sound and schedule deletion
   serverPlay3D(ItemThrowSound, %player.getTransform());
   %pack.schedulePop();
}

function MotionSensorDeployable::onInventory(%this,%player,%value)
{
   // %this = Sensor pack datablock
   // %player = player
   // %value = 1 if gaining a pack, 0 if losing a pack

   if(%player.getClassName() $= "Player")
   {
      if(%value)
      {
         // player picked up or bought a motion sensor pack
         %player.deploySensors = %this.maxSensors;
         %player.client.updateSensorPackText(%player.deploySensors);
      }
      else
      {
         // player dropped or sold a motion sensor pack
         if(%player.throwSensorPack)
         {
            // player threw the pack
            %player.throwSensorPack = 0;
            // everything handled in ::onThrow above
         }
         else
         {
            //the pack was sold at an inventory station, or unmounted because the player
            // used all the sensors
            %player.deploySensors = 0;
            %player.client.updateSensorPackText(%player.deploySensors);
         }
      }
   }
   Pack::onInventory(%this,%player,%value);
}

function PulseSensorDeployable::onPickup(%this, %pack, %player, %amount)
{
   // %this = Sensor pack datablock
   // %pack = Sensor pack object number
   // %player = player
   // %amount = amount picked up (1)

   if(%pack.sensors $= "")
   {
      // assume that this is a pack that has been placed in a mission
      // this case was handled in ::onInventory below (max sensors);
   }
   else
   {
      // find out how many sensor were in the pack
      %player.deploySensors = %pack.sensors;
      %player.client.updateSensorPackText(%player.deploySensors);
   }
}

function PulseSensorDeployable::onThrow(%this,%pack,%player)
{
   // %this = Sensor pack datablock
   // %pack = Sensor pack object number
   // %player = player

   %player.throwSensorPack = 1;
   %pack.sensors = %player.deploySensors;
   %player.deploySensors = 0;
   %player.client.updateSensorPackText(%player.deploySensors);
   // do the normal ItemData::onThrow stuff -- sound and schedule deletion
   serverPlay3D(ItemThrowSound, %player.getTransform());
   %pack.schedulePop();
}

function PulseSensorDeployable::onInventory(%this,%player,%value)
{
   // %this = Sensor pack datablock
   // %player = player
   // %value = 1 if gaining a pack, 0 if losing a pack

   if(%player.getClassName() $= "Player")
   {
      if(%value)
      {
         // player picked up or bought a motion sensor pack
         %player.deploySensors = %this.maxSensors;
         %player.client.updateSensorPackText(%player.deploySensors);
      }
      else
      {
         // player dropped or sold a motion sensor pack
         if(%player.throwSensorPack)
         {
            // player threw the pack
            %player.throwSensorPack = 0;
            // everything handled in ::onThrow above
         }
         else
         {
            //the pack was sold at an inventory station, or unmounted because the player
            // used all the sensors
            %player.deploySensors = 0;
            %player.client.updateSensorPackText(%player.deploySensors);
         }
      }
   }
   Pack::onInventory(%this,%player,%value);
}

function TurretIndoorDeployableImage::getInitialRotation(%item, %plyr)
{
   %surfaceAngle = getTerrainAngle(%item.surfaceNrm);
   if(%surfaceAngle > 155)
      %item.deployed = TurretDeployedCeilingIndoor;
   else if(%surfaceAngle > 45)
      %item.deployed = TurretDeployedWallIndoor;
   else
      %item.deployed = TurretDeployedFloorIndoor;
}  

function TurretIndoorDeployable::onPickup(%this, %obj, %shape, %amount)
{
   // created to prevent console errors
}

function TurretOutdoorDeployable::onPickup(%this, %obj, %shape, %amount)
{
   // created to prevent console errors
}

function InventoryDeployable::onPickup(%this, %obj, %shape, %amount)
{
   // created to prevent console errors
}

// ---------------------------------------------------------------------------------------
// deployed station functions
function DeployedStationInventory::onEndSequence(%data, %obj, %thread)
{
   Parent::onEndSequence(%data, %obj, %thread);
   if(%thread == $DeployThread)
   {
      %trigger = new Trigger()
      {
         dataBlock = stationTrigger;
      polyhedron = "-0.75 0.0 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
      };             
      MissionCleanup.add(%trigger);
      %trigger.setTransform(%obj.getTransform());

      // associate the trigger with the station
      %trigger.station = %obj;
      %trigger.mainObj = %obj;
      %trigger.disableObj = %obj;
      %obj.trigger = %trigger;
   }
}

//--------------------------------------------------------------------------
//DeployedMotionSensor:
//--------------------------------------------------------------------------

function DeployedMotionSensor::onDestroyed(%this, %obj, %prevState)
{
   //%obj.hide(true);
   Parent::onDestroyed(%this, %obj, %prevState);
   $TeamDeployedCount[%obj.team, MotionSensorDeployable]--;
   %obj.schedule(500, "delete");
}

//--------------------------------------------------------------------------
//DeployedPulseSensor:
//--------------------------------------------------------------------------
function PulseSensorDeployableImage::onActivate(%data, %obj, %slot)
{
   %data.testInvalidDeployConditions(%obj, %slot);
}

function DeployedPulseSensor::onDestroyed(%this, %obj, %prevState)
{
   Parent::onDestroyed(%this, %obj, %prevState);
   $TeamDeployedCount[%obj.team, PulseSensorDeployable]--;
   %obj.schedule(300, "delete");
}

// ---------------------------------------------------------------------------------------
// deployed turret functions

function DeployedTurret::onAdd(%data, %obj)
{
   Parent::onAdd(%data, %obj);
   // auto-mount the barrel
   %obj.mountImage(%data.barrel, 0, false);
}

function DeployedTurret::onDestroyed(%this, %obj, %prevState)
{
   Parent::onDestroyed(%this, %obj, %prevState);
   %turType = %this.getName();
   // either it'll be an outdoor turret, or one of the three types of indoor turret
   // (floor, ceiling, wall)
   if(%turType $= "TurretDeployedOutdoor")
      %turType = "TurretOutdoorDeployable";
   else
      %turType = "TurretIndoorDeployable";

   // decrement team count
   $TeamDeployedCount[%obj.team, %turType]--;

   %obj.schedule(700, "delete");
}

function countTurretsAllowed(%type)
{
	for(%j = 1; %j < Game.numTeams; %j++)
		%teamPlayerCount[%j] = 0;
   %numClients = ClientGroup.getCount();
   for(%i = 0; %i < %numClients; %i++)
   {
      %cl = ClientGroup.getObject(%i);
		if(%cl.team > 0)
			%teamPlayerCount[%cl.team]++;
   }
	// the bigger team determines the number of turrets allowed
	%maxPlayers = %teamPlayerCount[1] > %teamPlayerCount[2] ? %teamPlayerCount[1] : %teamPlayerCount[2];
	// each team can have 1 turret of each type (indoor/outdoor) for every 2 players
	// minimum and maximums are defined in deployables.cs
	%teamTurretMax = mFloor(%maxPlayers / 2);
	if(%teamTurretMax < $TeamDeployableMin[%type])
		%teamTurretMax = $TeamDeployableMin[%type];
	else if(%teamTurretMax > $TeamDeployableMax[%type])
		%teamTurretMax = $TeamDeployableMax[%type];
	
	return %teamTurretMax;
}

