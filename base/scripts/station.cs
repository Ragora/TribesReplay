//******************************************************************************
//*   Station  -  Data Blocks                                                  *
//******************************************************************************
datablock EffectProfile(StationInventoryActivateEffect)
{
   effectname = "powered/inv_pad_on";
   minDistance = 5.0;
};

datablock EffectProfile(StationVehicleAcitvateEffect)
{
   effectname = "powered/vehicle_screen_on2";
   minDistance = 3.0;
};

datablock EffectProfile(StationVehicleDeactivateEffect)
{
   effectname = "powered/vehicle_screen_off";
   minDistance = 3.0;
};

datablock AudioProfile(StationInventoryActivateSound)
{
   filename = 	"fx/powered/inv_pad_on.wav";
   description = AudioClose3d;
   preload = 	 true;
   effect = StationInventoryActivateEffect;
};

datablock AudioProfile(MobileBaseInventoryActivateSound)
{
   filename    = "fx/vehicles/mpb_inv_station.wav";
   description = AudioClose3d;
   preload = true;
};

datablock AudioProfile(DepInvActivateSound)
{
   filename = 	"fx/powered/dep_inv_station.wav";
   description = AudioClose3d;
   preload = 	 true;
};

datablock AudioProfile(StationVehicleAcitvateSound)
{
   filename    = "fx/powered/vehicle_screen_on2.wav";
   description = AudioClosest3d;
   preload = true;
   effect = StationVehicleAcitvateEffect;
};

datablock AudioProfile(StationVehicleDeactivateSound)
{
   filename    = "fx/powered/vehicle_screen_off.wav";
   description = AudioClose3d;
   preload = true;
   effect = StationVehicleDeactivateEffect;
};

datablock AudioProfile(StationAccessDeniedSound)
{
   filename    = "fx/powered/station_denied.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(StationVehicleHumSound)
{
   filename    = "fx/powered/station_hum.wav";
   description = CloseLooping3d;
   preload = true;
};

datablock AudioProfile(StationInventoryHumSound)
{
   filename    = "fx/powered/station_hum.wav";
   description = CloseLooping3d;
   preload = true;
};

datablock StationFXPersonalData( PersonalInvFX )
{
   delay = 0;
   fadeDelay = 0.5;
   lifetime = 1.2;
   height = 2.5;
   numArcSegments = 10.0;
   numDegrees = 180.0;
   trailFadeTime = 0.5;
   leftRadius = 1.85;
   rightRadius = 1.85;
   leftNodeName = "FX1";
   rightNodeName = "FX2";

   texture[0] = "special/stationLight";
};


datablock DebrisData( StationDebris )
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
   baseRadius = 0.4;

   velocity = 9.0;
   velocityVariance = 4.5;
   
};             

datablock StaticShapeData(StationInventory) : StaticShapeDamageProfile
{  
   className = Station;
   catagory = "Stations";
   shapeFile = "station_inv_human.dts";
   maxDamage = 1.00;
   destroyedLevel = 1.00;
   disabledLevel = 0.70;
   explosion      = ShapeExplosion;
	expDmgRadius = 8.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
	// don't allow this object to be damaged in non-team-based
	// mission types (DM, Rabbit, Bounty, Hunters)
	noIndividualDamage = true;

   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 75;
	maxEnergy = 50;
	rechargeRate = 0.35;
   doesRepair = true;
   humSound = StationInventoryHumSound;

   cmdCategory = "Support";
   cmdIcon = CMDStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_inventory_grey";
   targetNameTag = 'Inventory';
   targetTypeTag = 'Station';

   debrisShapeName = "debris_generic.dts";
   debris = StationDebris;
};
   
datablock StaticShapeData(StationVehicle) : StaticShapeDamageProfile
{   
   className = Station;
   catagory = "Stations";
   shapeFile = "vehicle_pad_station.dts";
   maxDamage = 1.20;
   destroyedLevel = 1.20;
   disabledLevel = 0.84;
   explosion      = ShapeExplosion;
	expDmgRadius = 10.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 33;
	maxEnergy = 250;
	rechargeRate = 0.31;
   humSound = StationVehicleHumSound;
	// don't let these be damaged in Siege missions
	noDamageInSiege = true;

   cmdCategory = "Support";
   cmdIcon = CMDVehicleStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_vehicle_pad_inventory";
   targetTypeTag = 'Vehicle Station';

   debrisShapeName = "debris_generic.dts";
   debris = StationDebris;
};

datablock StaticShapeData(StationVehiclePad)
{   
   className = Station;
   catagory = "Stations";
   shapeFile = "vehicle_pad.dts";
   isInvincible = true;
   dynamicType = $TypeMasks::StaticObjectType;
   rechargeRate = 0.05;
};

//datablock StaticShapeData(StationAmmo)
//{   
//   className = Station;
//   catagory = "Stations";
//   shapeFile = "station_ammo.dts";
//   maxDamage = 1.0;
//   disabledLevel = 0.6;
//   destroyedLevel = 0.8;
//   icon = "CMDStationIcon";
//   dynamicType = $TypeMasks::StationObjectType;
//};

datablock StaticShapeData(MobileInvStation)
{  
   className = Station;
   catagory = "Stations";
   shapeFile = "station_inv_mpb.dts";
   icon = "CMDStationIcon";
	// don't allow this object to be damaged in non-team-based
	// mission types (DM, Rabbit, Bounty, Hunters)
	noIndividualDamage = true;

   dynamicType = $TypeMasks::StationObjectType;
	rechargeRate = 0.256;
   doesRepair = true;
};
 

//******************************************************************************
//*   Station  -  Functions                                                    *
//******************************************************************************

////////////////////////////////////////////////////////////////////////////////
/// -Inventory- ////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block 
//                %obj = Object being added
//Decription -- Called when the object is added to the mission 
////////////////////////////////////////////////////////////////////////////////
function StationInventory::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

   %obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };             
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());

   %trigger.station = %obj;
   %trigger.mainObj = %obj;
   %trigger.disableObj = %obj;
   %obj.trigger = %trigger;
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when station has been triggered and animation is 
//              completed
////////////////////////////////////////////////////////////////////////////////
function StationInventory::stationReady(%data, %obj)
{
   //Display the Inventory Station GUI here
   %obj.notReady = 1;
   %obj.inUse = "Down";
   %obj.schedule(500,"playThread",$ActivateThread,"activate1");
   %player = %obj.triggeredBy;
   %energy = %player.getEnergyLevel();
   %player.setCloaked(true);
   %player.schedule(500, "setCloaked", false);              
	if (!%player.client.isAIControlled())
	   buyFavorites(%player.client);
   
   %player.setEnergyLevel(%energy);

   %data.schedule( 500, "beginPersonalInvEffect", %obj );
}

function StationInventory::beginPersonalInvEffect( %data, %obj )
{
   if (!%obj.isDisabled())
   {
      %fx = new StationFXPersonal()
      {
         dataBlock = PersonalInvFX;
         stationObject    = %obj;
      };
   }
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function StationInventory::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block 
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate 
//              animation. 
////////////////////////////////////////////////////////////////////////////////
function StationInventory::getSound(%data, %forward)
{
   if(%forward)
      return "StationInventoryActivateSound";
   else
      return false;
}

/// -Inventory- ////////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block 
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station 
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function StationInventory::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
      %colObj.setTransform(getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2) + 0.8 @ " " @ %rot);//center player on object
      %colObj.setMoveState(true);
      %colObj.schedule(1600,"setMoveState", false);
      %colObj.setvelocity("0 0 0");
      return true;
   }
   return false;
}

////////////////////////////////////////////////////////////////////////////////
/// -Vehicle- //////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block 
//                %obj = Object being added
//Decription -- Called when the object is added to the mission 
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

	%obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.0 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.0";
   };             
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());
   %trigger.station = %obj;
   %obj.trigger = %trigger;
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when station has been triggered and animation is 
//              completed
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::stationReady(%data, %obj)
{
   // Make sure none of the other popup huds are active:
   messageClient( %obj.triggeredBy.client, 'CloseHud', "", 'scoreScreen' );
   messageClient( %obj.triggeredBy.client, 'CloseHud', "", 'inventoryScreen' );

   //Display the Vehicle Station GUI
   commandToClient(%obj.triggeredBy.client, 'StationVehicleShowHud');
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::stationFinished(%data, %obj)
{
   //Hide the Vehicle Station GUI
	commandToClient(%obj.triggeredBy.client, 'StationVehicleHideHud');
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block 
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate 
//              animation. 
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::getSound(%data, %forward)
{
   if(%forward)
      return "StationVehicleAcitvateSound";
   else
      return "StationVehicleDeactivateSound";
}

/// -Vehicle- //////////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block 
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station 
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function StationVehicle::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %posXY = getWords(%trigger.getTransform(),0 ,1);
      %posZ = getWord(%trigger.getTransform(), 2);
      %rotZ =  getWord(%obj.getTransform(), 5);
      %angle =  getWord(%obj.getTransform(), 6);
	   %angle += 3.141592654;
      if(%angle > 6.283185308)
         %angle = %angle - 6.283185308;
      %colObj.setvelocity("0 0 0");
      %colObj.setTransform(%posXY @ " " @ %posZ + 0.2 @ " " @ "0 0 "  @ %rotZ @ " " @ %angle );//center player on object
      return true;
   }
   return false;
}

function StationVehiclePad::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);

	%obj.ready = true;
   %obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %xform = %obj.getSlotTransform(0);
   %pos = getWords(%xform, 0, 2);
   %rot = getWords(%xform, 3, 5);
   %angle = (getWord(%xform, 6) * 180) / 3.14159;

   %sv = new StaticShape() {
	   scale = "1 1 1";
      dataBlock = StationVehicle;
	   lockCount = "0";
	   homingCount = "0";
	   team = %obj.team;
      position = %pos;
      rotation = %rot @ " " @ %angle;
	};
   
   MissionCleanup.add(%sv);
   %sv.getDataBlock().gainPower(%sv);
   %sv.pad = %obj;
   %obj.station = %sv;
   %sv.trigger.mainObj = %obj;
   %sv.trigger.disableObj = %sv;

   //Remove unwanted vehicles
   if(%obj.scoutVehicle !$= "Removed")
	   %sv.vehicle[scoutvehicle] = true;
   if(%obj.assaultVehicle !$= "Removed")
	   %sv.vehicle[assaultVehicle] = true;
   if(%obj.mobileBaseVehicle !$= "Removed")
	{
      %sv.vehicle[mobileBasevehicle] = true;
//      createTeleporter(%sv.getDataBlock(), %sv);
   }
   if(%obj.scoutFlyer !$= "Removed")
	   %sv.vehicle[scoutFlyer] = true;
   if(%obj.bomberFlyer !$= "Removed")
	   %sv.vehicle[bomberFlyer] = true;
   if(%obj.hapcFlyer !$= "Removed")
   	%sv.vehicle[hapcFlyer] = true;
}

function StationVehiclePad::onEndSequence(%data, %obj, %thread)
{
   if(%thread == $ActivateThread)
   {
      %obj.ready = true;
      %obj.stopThread($ActivateThread);
   }
   Parent::onEndSequence(%data, %obj, %thread);
}

////////////////////////////////////////////////////////////////////////////////
/// -Mobile Base Inventory- ////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block 
//                %obj = Object being added
//Decription -- Called when the object is added to the mission 
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::onAdd(%this, %obj)
{
}

function MobileInvStation::createTrigger(%this, %obj)
{
   Parent::onAdd(%this, %obj);

	%obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };             
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.vehicle.getSlotTransform(2));

   %trigger.station = %obj;
   %trigger.mainObj = %obj;
   %trigger.disableObj = %obj;
   
   %obj.trigger = %trigger;
//   createTarget(%obj, 'Inventory Station', "", "", 'Station', 0, 0);
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when station has been triggered and animation is 
//              completed
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::stationReady(%data, %obj)
{
   //Display the Inventory Station GUI here
   %obj.notReady = 1;
   %obj.inUse = "Down";
   %obj.schedule(200,"playThread",$ActivateThread,"activate1");
   %obj.getObjectMount().playThread($ActivateThread,"Activate");
   %player = %obj.triggeredBy;
   %energy = %player.getEnergyLevel();
   %player.setCloaked(true);
   %player.schedule(900, "setCloaked", false);
	if (!%player.client.isAIControlled())
	   buyFavorites(%player.client);
   
   %player.setEnergyLevel(%energy);
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block 
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate 
//              animation. 
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::getSound(%data, %forward)
{
   if(%forward)
      return "MobileBaseInventoryActivateSound";
   else
      return false;
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block 
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station 
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function MobileInvStation::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
//      %colObj.setTransform(getWord(%pos,0) @ " " @ getWord(%pos,1) - 0.75 @ " " @ getWord(%pos,2)+0.7 @ " " @ %rot);//center player on object
      %colObj.setTransform(getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2)+0.8 @ " " @ %rot);//center player on object
      %colObj.setMoveState(true);
      %colObj.schedule(1600,"setMoveState", false);
      %colObj.setvelocity("0 0 0");
      return true;
   }
   return false;
}

function MobileInvStation::onDamage()
{
}

function MobileInvStation::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
   //If vehicle station is hit then apply damage to the vehicle
   %targetObject.getObjectMount().damage(%sourceObject, %position, %amount, %damageType);
}

////////////////////////////////////////////////////////////////////////////////
/// -Station Trigger- //////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onEnterTrigger (%data, %obj, %colObj)
//                %data = Trigger Data Block 
//                %obj = Trigger Object 
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been triggered 
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onEnterTrigger(%data, %obj, %colObj)  
{
	//make sure it's a player object, and that that object is still alive
   if(%colObj.getDataBlock().className !$= "Armor" || %colObj.getState() $= "Dead")
      return;

   %colObj.inStation = true;
   commandToClient(%colObj.client,'setStationKeys', true);
   if(Game.stationOnEnterTrigger(%data, %obj, %colObj)) {
      //verify station.team is team associated and isn't on player's team
      if((%obj.mainObj.team != %colObj.client.team) && (%obj.mainObj.team != 0))
      {
         %obj.station.playAudio(2, StationAccessDeniedSound);
         messageClient(%colObj.client, 'msgStationDenied', '\c2Access Denied -- Wrong team.');
      }
      else if(%obj.disableObj.isDisabled())
      {
         messageClient(%colObj.client, 'msgStationDisabled', '\c2Station is disabled.');
      }
      else if(!%obj.mainObj.isPowered())
      {
         messageClient(%colObj.client, 'msgStationNoPower', '\c2Station is not powered.');
      }
      else if(%obj.station.notDeployed)
      {
         messageClient(%colObj.client, 'msgStationNotDeployed', '\c2Station is not deployed.');
      }
      else if(%obj.station.triggeredBy $= "")
      {
         if(%obj.station.getDataBlock().setPlayersPosition(%obj.station, %obj, %colObj))
         {
            messageClient(%colObj.client, 'CloseHud', "", 'inventoryScreen');
            commandToClient(%colObj.client, 'TogglePlayHuds', true);
            %obj.station.triggeredBy = %colObj;
            %obj.station.getDataBlock().stationTriggered(%obj.station, 1);
            %colObj.station = %obj.station;
            %colObj.lastWeapon = ( %colObj.getMountedImage($WeaponSlot) == 0 ) ? "" : %colObj.getMountedImage($WeaponSlot).getName().item;
            %colObj.unmountImage($WeaponSlot);
         }
      }
   }
}


////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onLeaveTrigger (%data, %obj, %colObj)
//                %data = Trigger Data Block 
//                %obj = Trigger Object 
//                %colObj = Object that collided with the trigger
//Decription -- Called when trigger has been untriggered
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onLeaveTrigger(%data, %obj, %colObj)
{
   if(%colObj.getDataBlock().className !$= "Armor")
      return;

   %colObj.inStation = false;
   commandToClient(%colObj.client,'setStationKeys', false);
   if(%obj.station)
   {
      if(%obj.station.triggeredBy == %colObj)
      {
         %obj.station.getDataBlock().stationFinished(%obj.station);
         %obj.station.getDataBlock().endRepairing(%obj.station);
         %obj.station.triggeredBy = "";
         %obj.station.getDataBlock().stationTriggered(%obj.station, 0);
               
         if(!%colObj.teleporting)
            %colObj.station = "";
			if(%colObj.getMountedImage($WeaponSlot) == 0 && !%colObj.teleporting)
	      {
	         if(%colObj.inv[%colObj.lastWeapon])
	            %colObj.use(%colObj.lastWeapon);
	         
            if(%colObj.getMountedImage($WeaponSlot) == 0) 
               %colObj.selectWeaponSlot( 0 );
	      }
      }
   }
}

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- stationTriggered(%data, %obj, %isTriggered)
//                %data = Station Data Block 
//                %obj = Station Object 
//                %isTriggered = 1 if triggered; 0 if status changed to 
//                               untriggered
//Decription -- Called when a "station trigger" has been triggered or 
//              untriggered
////////////////////////////////////////////////////////////////////////////////
function Station::stationTriggered(%data, %obj, %isTriggered)
{
   if(%data.teleporter $= "")
   {   
      if(%isTriggered)
      {
         %obj.setThreadDir($ActivateThread, TRUE);
         %obj.playThread($ActivateThread,"activate");	
         %obj.playAudio($ActivateSound, %data.getSound(true));
         %obj.inUse = "Up";
      }
      else
      {
         if(%obj.getDataBlock().getName() !$= StationVehicle)
         {
            %obj.stopThread($ActivateThread);
            if(%obj.getObjectMount())
               %obj.getObjectMount().stopThread($ActivateThread);
            %obj.inUse = "Down";
         }
         else
         {
            %obj.setThreadDir($ActivateThread, FALSE);
            %obj.playThread($ActivateThread,"activate");
            %obj.playAudio($ActivateSound, %data.getSound(false));
            %obj.inUse = "Down";
         }                            
      }
   }
   else
      %data.tryTeleport(%obj);
}
                                
////-Station-///////////////////////////////////////////////////////////////////
//Function -- onEndSequence(%data, %obj, %thread)
//                %data = Station Data Block 
//                %obj = Station Object
//                %thread = Thread number that the animation is associated 
//                          with / running on. 
//Decription -- Called when an animation sequence is finished playing
////////////////////////////////////////////////////////////////////////////////
function Station::onEndSequence(%data, %obj, %thread)
{
   if(%thread == $ActivateThread)
   {
      if(%obj.inUse $= "Up")
      {
         %data.stationReady(%obj);
         %player = %obj.triggeredBy;
	      if(%data.doesRepair && !%player.stationRepairing && %player.getDamageLevel() != 0) {
	         %oldRate = %player.getRepairRate();
	         %player.setRepairRate(%oldRate + 0.00625);
	         %player.stationRepairing = 1;
	      }
      }
      else
      {
         if(%obj.getDataBlock().getName() !$= MobileInvStation)
         {
            %obj.stopThread($ActivateThread);
            %obj.inUse = "Down";
         }
      }
   }
   Parent::onEndSequence(%data, %obj, %thread);
}

////-Station-///////////////////////////////////////////////////////////////////
//Function -- onCollision(%data, %obj, %colObj)
//                %data = Station Data Block 
//                %obj = Station Object 
//                %colObj = Object that collided with the station
//Decription -- Called when an object collides with a station
////////////////////////////////////////////////////////////////////////////////
function Station::onCollision(%data, %obj, %colObj)
{
	// Currently Not Needed
}

////-Station-///////////////////////////////////////////////////////////////////
//Function -- endRepairing(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called to stop repairing the object
////////////////////////////////////////////////////////////////////////////////
function Station::endRepairing(%data, %obj)
{
   if(%obj.triggeredBy.stationRepairing)
   {
      %oldRate = %obj.triggeredBy.getRepairRate();
      %obj.triggeredBy.setRepairRate(%oldRate - 0.00625);
      %obj.triggeredBy.stationRepairing = 0;
   }
}

////-Station Trigger-///////////////////////////////////////////////////////////
//Function -- onTickTrigger(%data, %obj)
//                %data = Trigger Data Block 
//                %obj = Trigger Object 
//Decription -- Called every tick if triggered
////////////////////////////////////////////////////////////////////////////////
function stationTrigger::onTickTrigger(%data, %obj)
{
}

//******************************************************************************
//*   Station General  -  Functions                                            *
//******************************************************************************

//function Station::onGainPowerEnabled(%data, %obj)

function Station::onLosePowerDisabled(%data, %obj)
{
   Parent::onLosePowerDisabled(%data, %obj);

	// check to see if a player was using this station
	%occupied = %obj.triggeredBy;
	if(%occupied > 0)
	{
		if(%data.doesRepair)
         %data.endRepairing(%obj);
		// if it's a deployed station, stop "flashing panels" thread
		if(%data.getName() $= DeployedStationInventory)
		   %obj.stopThread($ActivateThread);
		// reset some attributes
      %occupied.setCloaked(false);
		%occupied.station = "";
		%occupied.inStation = false;
		%obj.triggeredBy = "";
		// restore "toggle inventory hud" key
	   commandToClient(%occupied.client,'setStationKeys', false);
		// re-mount last weapon or weapon slot 0
		if(%occupied.getMountedImage($WeaponSlot) == 0)
      {
         if(%occupied.inv[%occupied.lastWeapon])
            %occupied.use(%occupied.lastWeapon);
         if(%occupied.getMountedImage($WeaponSlot) == 0) 
            %occupied.selectWeaponSlot( 0 );
      }
	}
}

function StationVehiclePad::gainPower(%data, %obj)
{
   %obj.station.setSelfPowered();
   Parent::gainPower(%data, %obj);
}

function StationVehiclePad::losePower(%data, %obj)
{
   %obj.station.clearSelfPowered();
   Parent::losePower(%data, %obj);
}

//---------------------------------------------------------------------------
// DeployedStationInventory:
//---------------------------------------------------------------------------

function DeployedStationInventory::stationReady(%data, %obj)
{
   %obj.notReady = 1;
   %player = %obj.triggeredBy;
   %obj.playThread($ActivateThread,"activate1");
   // function below found in inventoryHud.cs
   if (!%player.client.isAIControlled())
      buyDeployableFavorites(%player.client);
}

function DeployedStationInventory::stationFinished(%data, %obj)
{
}

function DeployedStationInventory::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      // build offset for player position
      %rot = getWords(%obj.getTransform(), 3, 6);
      %colObj.setTransform( getWords(%colObj.getTransform(),0,2) @ " " @ %rot );

      %colObj.setvelocity("0 0 0");
      %colObj.setMoveState(true);
      %colObj.schedule(1600,"setMoveState", false);
      return true;
   }
   return false;
}

function DeployedStationInventory::onDestroyed(%data, %obj, %prevState)
{
   %obj.trigger.delete();
   // decrement team deployed count for this item
   $TeamDeployedCount[%obj.team, InventoryDeployable]--;
   
   // when a station is destroyed, we don't need its trigger any more
   %obj.schedule(700, "delete");
   Parent::onDestroyed(%data, %obj, %prevState);
}

/// -Deployable Inventory- //////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block 
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate 
//              animation. 
////////////////////////////////////////////////////////////////////////////////
function DeployedStationInventory::getSound(%data, %forward)
{
   if(%forward)
      return "DepInvActivateSound";
   else
      return false;
}


////////////////////////////////////////////////////////////////////////////////
/// -Mobile Base Teleporter DATA- //////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

datablock AudioProfile(StationTeleportAcitvateSound)
{
   filename    = "fx/powered/vehicle_screen_on2.wav";
   description = AudioClosest3d;
   preload = true;
   effect = StationVehicleAcitvateEffect;
};

datablock AudioProfile(StationTeleportHumSound)
{
   filename    = "fx/powered/station_hum.wav";
   description = CloseLooping3d;
   preload = true;
};

datablock AudioProfile(StationTeleportDeactivateSound)
{
   filename    = "fx/powered/vehicle_screen_off.wav";
   description = AudioClose3d;
   preload = true;
   effect = StationVehicleDeactivateEffect;
};

datablock AudioProfile(TeleportSound)
{
   filename    = "fx/powered/vehicle_screen_on2.wav";
   description = AudioClosest3d;
   preload = true;
   effect = StationVehicleAcitvateEffect;
};

datablock AudioProfile(UnTeleportSound)
{
   filename    = "fx/powered/vehicle_screen_off.wav";
   description = AudioClose3d;
   preload = true;
   effect = StationVehicleDeactivateEffect;
};
                          
datablock StaticShapeData(MPBTeleporter) : StaticShapeDamageProfile
{  
   className = Station;
   catagory = "Stations";
   shapeFile = "station_teleport.dts";
   maxDamage = 1.20;
   destroyedLevel = 1.20;
   disabledLevel = 0.84;
   explosion      = ShapeExplosion;
	expDmgRadius = 10.0;
	expDamage = 0.4;
	expImpulse = 1500.0;
   dynamicType = $TypeMasks::StationObjectType;
	isShielded = true;
	energyPerDamagePoint = 33;
	maxEnergy = 250;
	rechargeRate = 0.31;
   humSound = StationTeleportHumSound;
	// don't let these be damaged in Siege missions
	noDamageInSiege = true;

   cmdCategory = "Support";
   cmdIcon = CMDVehicleStationIcon;
   cmdMiniIconName = "commander/MiniIcons/com_vehicle_pad_inventory";
   targetTypeTag = 'Teleport Station';
   teleporter = 1;
};
                                
////////////////////////////////////////////////////////////////////////////////
/// -Mobile Base Teleport- /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- onAdd (%this, %obj)
//                %this = Object data block 
//                %obj = Object being added
//Decription -- Called when the object is added to the mission 
////////////////////////////////////////////////////////////////////////////////
function MPBTeleporter::onAdd(%this, %obj)
{
   Parent::onAdd(%this, %obj);
}

function MPBTeleporter::createTrigger(%this, %obj)
{
//   createTarget(%obj, 'Inventory Station', "", "", 'Station', 0, 0);

	%obj.setRechargeRate(%obj.getDatablock().rechargeRate);
   %trigger = new Trigger()
   {
      dataBlock = stationTrigger;
      polyhedron = "-0.75 0.75 0.1 1.5 0.0 0.0 0.0 -1.5 0.0 0.0 0.0 2.3";
   };             
   MissionCleanup.add(%trigger);
   %trigger.setTransform(%obj.getTransform());

   %trigger.station = %obj;
   %trigger.mainObj = %obj.vStation;
   %trigger.disableObj = %obj;
   
   %obj.trigger = %trigger;
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationReady(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when station has been triggered and animation is 
//              completed
////////////////////////////////////////////////////////////////////////////////
function MPBTeleporter::stationReady(%data, %obj)
{
   //Display the Inventory Station GUI here
   %obj.notReady = 1;
   %obj.inUse = "Down";
   %obj.schedule(200,"playThread",$ActivateThread,"activate1");
   %obj.getObjectMount().playThread($ActivateThread,"Activate");
   %player = %obj.triggeredBy;
   %energy = %player.getEnergyLevel();
   %player.setCloaked(true);
   %player.schedule(900, "setCloaked", false);
	if (!%player.client.isAIControlled())
	   buyFavorites(%player.client);
   
   %player.setEnergyLevel(%energy);
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- stationFinished(%data, %obj)
//                %data = Station Data Block 
//                %obj = Station Object 
//Decription -- Called when player has left the station
////////////////////////////////////////////////////////////////////////////////
function MPBTeleporter::stationFinished(%data, %obj)
{
   //Hide the Inventory Station GUI
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- getSound(%data, %forward)
//                %data = Station Data Block 
//                %forward = direction the animation is playing
//Decription -- This sound will be played at the same time as the activate 
//              animation. 
////////////////////////////////////////////////////////////////////////////////
function MPBTeleporter::getSound(%data, %forward)
{
   if(%forward)
      return "MobileBaseInventoryActivateSound";
   else
      return false;
}

/// -Mobile Base- //////////////////////////////////////////////////////////////
//Function -- setPlayerPosition(%data, %obj, %trigger, %colObj)
//                %data = Station Data Block 
//                %obj = Station Object
//                %trigger = Stations trigger
//                %colObj = Object that is at the station 
//Decription -- Called when player enters the trigger.  Used to set the player
//              in the center of the station.
////////////////////////////////////////////////////////////////////////////////
function MPBTeleporter::setPlayersPosition(%data, %obj, %trigger, %colObj)
{
   %vel = getWords(%colObj.getVelocity(), 0, 1) @ " 0";
   if((VectorLen(%vel) < 22) && (%obj.triggeredBy != %colObj))
   {
      %pos = %trigger.position;
      %colObj.setvelocity("0 0 0");
	   %rot = getWords(%colObj.getTransform(),3, 6);
      %colObj.setTransform(getWord(%pos,0) @ " " @ getWord(%pos,1) @ " " @ getWord(%pos,2)+0.8 @ " " @ %rot);//center player on object
      %colObj.setvelocity("0 0 0");
      return true;
   }
   return false;
}

function MPBTeleporter::tryTeleport(%data, %obj)
{
   if(isObject(%obj.MPB) && %obj.MPB.fullyDeployed && %obj.triggeredBy !$= "")
   {
      %trans = %obj.MPB.getTransform();
      %vX = getWord(%trans, 0);
      %vY = getWord(%trans, 1);
      %vZ = getWord(%trans, 2);
      %rot= getWords(%trans, 3,6);

      %obj.triggeredBy.teleporting = 1;
      %obj.triggeredBy.startFade( 1000, 0, true );
      %obj.triggeredBy.playAudio($PlaySound, TeleportSound);
      %obj.triggeredBy.setMoveState(true);
      %data.schedule(4500,"teleportingDone", %obj.triggeredBy);

      %data.schedule(2000, "teleportout", %obj, %obj.triggeredBy, %vX @ " " @ %vY @ " " @ %vZ + 3 @ " " @ %rot);
      
   }
   else if(%obj.triggeredBy !$= "")
      MessageClient(%obj.triggeredBy.client, "", 'MPB is not deployed.'); 
}


function MPBTeleporter::onEndSequence(%data, %obj, %thread)
{
}

function MPBTeleporter::teleportOut(%data, %obj, %player, %trans)
{
   if(isObject(%obj.MPB))
   {
      %index = -1;
      for(%x=0; %x < %obj.MPB.spawnPosCount; %x++)
      {
         %index = mFloor(getRandom() * %obj.MPB.spawnPosCount);

         InitContainerRadiusSearch(%MPB.spawnPos[%index], 2, $TypeMasks::MoveableObjectType);
         if(ContainerSearchNext() == 0)
            break;
         else
            %index = -1;
      }
      
      if(%index >= 0)
         %player.setTransform(%obj.MPB.spawnPos[%index] @ " " @ getWords(%obj.MPB.getTransform(), 3, 6));   
      else
      {
         messageClient(%player.client, "", 'No Valid teleporting positions.');
         %player.teleporting = 0;
      }
   }
   else
   {
      messageClient(%player.client, "", 'No Valid teleporting positions because MPB was destroyed');
      %player.teleporting = 0;
   }
   %data.schedule(1000, "teleportIn", %player);
}

function MPBTeleporter::teleportIn(%data, %player, %trans)
{
   %player.startFade(1000, 0, false );
   %player.playAudio($PlaySound, UnTeleportSound);
}

function MPBTeleporter::teleportingDone(%data, %player)
{
   %player.setMoveState(false);
   %player.teleporting = 0;
   %player.station = "";
	if(%player.getMountedImage($WeaponSlot) == 0)
	{
	   if(%player.inv[%player.lastWeapon])
	      %player.use(%player.lastWeapon);
	   
      if(%player.getMountedImage($WeaponSlot) == 0) 
         %player.selectWeaponSlot( 0 );
	}
}

for(%y = -1; %y < 1; %y += 0.25)
{
   %xCount=0;
   for(%x = -1; %x < 1; %x += 0.25)
   {
      $MPBSpawnPos[(%yCount * 8) + %xCount] = %x @ " " @ %y; 
      %xCount++;
   }
   %yCount++;
}      

function checkSpawnPos(%MPB, %radius)
{
   %count = -1;
   for(%x = 0; %x < 64; %x++)
   {
      %pPos = getWords(%MPB.getTransform(), 0, 2);
      %pPosX = getWord(%pPos, 0);
      %pPosY = getWord(%pPos, 1);
      %pPosZ = getWord(%pPos, 2);
      
      %posX = %pPosX + ( getWord($MPBSpawnPos[%x],0) * %radius);
      %posY = %pPosY + (getWord($MPBSpawnPos[%x],1) * %radius);
      
      %terrHeight = getTerrainHeight(%posX @ " " @ %posY);

      if(abs(%terrHeight - %pPosZ) < %radius )
      {
         %mask = $TypeMasks::VehicleObjectType     | $TypeMasks::MoveableObjectType   |
                 $TypeMasks::StaticShapeObjectType | $TypeMasks::StaticTSObjectType   | 
                 $TypeMasks::ForceFieldObjectType  | $TypeMasks::ItemObjectType       | 
                 $TypeMasks::PlayerObjectType      | $TypeMasks::TurretObjectType     |
                 $TypeMasks::InteriorObjectType;

         InitContainerRadiusSearch(%posX @ " " @ %posY @ " " @ %terrHeight, 2, %mask);
         if(ContainerSearchNext() == 0)
            %MPB.spawnPos[%count++] = %posX @ " " @ %posY @ " " @ %terrHeight;                  
      }
   }   
   %MPB.spawnPosCount = %count;
}

function createTeleporter(%data, %obj)
{
   %Teleporter = new StaticShape() {
      scale = "1 1 1";
      dataBlock = "MPBTeleporter";
      lockCount = "0";
      homingCount = "0";
      team = %obj.team;
   };                
   %obj.teleporter = %Teleporter;
   %Teleporter.vStation = %obj.pad;
   MissionCleanup.add(%Teleporter);

   %trans = %obj.getTransform();
   %vSPos = getWords(%trans,0,2);
   %vRot =  getWords(%trans,3,5);
   %vAngle = getWord(%trans,6);
   %matrix = VectorOrthoBasis(%vRot @ " " @ %vAngle + 0.36);
   %yRot = getWords(%matrix, 3, 5);
   %pos = vectorAdd(%vSPos, vectorScale(%yRot, -31.5));
   
   %Teleporter.setTransform(%pos @ " " @ %vRot @ " " @ %vAngle);
   %Teleporter.getDataBlock().createTrigger(%Teleporter);
}

