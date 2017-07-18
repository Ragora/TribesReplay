//--------------------------------------------------------------------------
// Satchel Charge pack
// can be used by any armor type
// when activated, throws the pack -- when activated again (before
// picking up another pack), detonates with a BIG explosion.

//--------------------------------------------------------------------------
// Sounds

datablock AudioProfile(SatchelChargeActivateSound)
{
   filename    = "fx/packs/satchel_pack_activate.wav";
   description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(SatchelChargeExplosionSound)
{
   filename = "fx/packs/satchel_pack_detonate.wav";
   description = AudioBIGExplosion3d;
   preload = true;
};


//----------------------------------------------------------------------------
// Satchel Debris
//----------------------------------------------------------------------------
datablock ParticleData( SDebrisSmokeParticle )
{
   dragCoeffiecient     = 1.0;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;

   lifetimeMS           = 1000;  
   lifetimeVarianceMS   = 100;

   textureName          = "particleTest";

   useInvAlpha =     true;

   spinRandomMin = -60.0;
   spinRandomMax = 60.0;

   colors[0]     = "0.4 0.4 0.4 1.0";
   colors[1]     = "0.3 0.3 0.3 0.5";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 0.0;
   sizes[1]      = 2.0;
   sizes[2]      = 3.0;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleEmitterData( SDebrisSmokeEmitter )
{
   ejectionPeriodMS = 7;
   periodVarianceMS = 1;

   ejectionVelocity = 1.0;  // A little oomph at the back end
   velocityVariance = 0.2;

   thetaMin         = 0.0;
   thetaMax         = 40.0;

   particles = "SDebrisSmokeParticle";
};


datablock DebrisData( SatchelDebris )
{
   emitters[0] = SDebrisSmokeEmitter;

   explodeOnMaxBounce = true;

   elasticity = 0.4;
   friction = 0.2;

   lifetime = 0.3;
   lifetimeVariance = 0.02;
};             

//--------------------------------------------------------------------------
// Satchel Explosion Particle effects
//--------------------------------------
datablock ParticleData(SatchelExplosionSmoke)
{
   dragCoeffiecient     = 0.4;
   gravityCoefficient   = -0.0;   // rises slowly
   inheritedVelFactor   = 0.025;

   lifetimeMS           = 2000;
   lifetimeVarianceMS   = 0;

   textureName          = "particleTest";

   useInvAlpha =  true;
   spinRandomMin = -200.0;
   spinRandomMax =  200.0;

   textureName = "special/Smoke/smoke_001";

   colors[0]     = "1.0 0.7 0.0 1.0";
   colors[1]     = "0.2 0.2 0.2 0.5";
   colors[2]     = "0.0 0.0 0.0 0.0";
   sizes[0]      = 7.0;
   sizes[1]      = 17.0;
   sizes[2]      = 2.0;
   times[0]      = 0.0;
   times[1]      = 0.4;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(SatchelExplosionSmokeEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;

   ejectionVelocity = 14.25;
   velocityVariance = 2.25;

   thetaMin         = 0.0;
   thetaMax         = 180.0;

   lifetimeMS       = 200;

   particles = "SatchelExplosionSmoke";
};



datablock ParticleData(SatchelSparks)
{
   dragCoefficient      = 1;
   gravityCoefficient   = 0.0;
   inheritedVelFactor   = 0.2;
   constantAcceleration = 0.0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 150;
   textureName          = "special/bigSpark";
   colors[0]     = "0.56 0.36 0.26 1.0";
   colors[1]     = "0.56 0.36 0.26 1.0";
   colors[2]     = "1.0 0.36 0.26 0.0";
   sizes[0]      = 0.5;
   sizes[1]      = 0.5;
   sizes[2]      = 0.75;
   times[0]      = 0.0;
   times[1]      = 0.5;
   times[2]      = 1.0;

};

datablock ParticleEmitterData(SatchelSparksEmitter)
{
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionVelocity = 40;
   velocityVariance = 20.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 180;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvances = false;
   orientParticles  = true;
   lifetimeMS       = 200;
   particles = "SatchelSparks";
};



//---------------------------------------------------------------------------
// Explosion
//---------------------------------------------------------------------------

datablock ExplosionData(SatchelSubExplosion)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.5 0.5 0.5";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 80;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 1000;
   delayMS = 0;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 0.0;

   playSpeed = 1.5;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "3.0 3.0 3.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelSubExplosion2)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "0.7 0.7 0.7";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 170;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 1000;
   delayMS = 50;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 9.0;

   playSpeed = 1.5;

   sizes[0] = "1.5 1.5 1.5";
   sizes[1] = "1.5 1.5 1.5";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelSubExplosion3)
{
   explosionShape = "disc_explosion.dts";
   faceViewer           = true;
   explosionScale = "1.0 1.0 1.0";

   debris = SatchelDebris;
   debrisThetaMin = 10;
   debrisThetaMax = 170;
   debrisNum = 8;
   debrisVelocity = 60.0;
   debrisVelocityVariance = 15.0;

   lifetimeMS = 2000;
   delayMS = 100;

   emitter[0] = SatchelExplosionSmokeEmitter;
   emitter[1] = SatchelSparksEmitter;

   offset = 9.0;

   playSpeed = 2.5;

   sizes[0] = "1.0 1.0 1.0";
   sizes[1] = "1.0 1.0 1.0";
   times[0] = 0.0;
   times[1] = 1.0;
};

datablock ExplosionData(SatchelMainExplosion)
{
   subExplosion[0] = SatchelSubExplosion;
   subExplosion[1] = SatchelSubExplosion2;
   subExplosion[2] = SatchelSubExplosion3;
};



//--------------------------------------------------------------------------
// Projectile

//-------------------------------------------------------------------------
// shapebase datablocks
datablock ShapeBaseImageData(SatchelChargeImage)
{
   shapeFile = "pack_upgrade_satchel.dts";
   item = SatchelCharge;
   mountPoint = 1;
   offset = "0 0 0";
   emap = true;
};

datablock ItemData(SatchelCharge)
{
   className = Pack;
   catagory = "Packs";
   image = SatchelChargeImage;
   shapeFile = "pack_upgrade_satchel.dts";
   mass = 1;
   elasticity = 0.2;
   friction = 0.6;
   pickupRadius = 2;
   rotate = true;
	pickUpName = "a satchel charge pack";

   computeCRC = true;
};

datablock ItemData(SatchelChargeThrown)
{
   shapeFile = "pack_upgrade_satchel.dts";
   explosion = SatchelMainExplosion;
   mass = 1.2;
   elasticity = 0.1;
   friction = 0.9;
   rotate = false;
   pickupRadius = 0;
	noTimeout = true;
	armDelay = 3000;
   maxDamage = 0.6;

   kickBackStrength    = 4000;

   computeCRC = true;
};

//--------------------------------------------------------------------------

function SatchelCharge::onUse(%this, %obj)
{
   %item = new Item() {
      dataBlock = SatchelChargeThrown;
      rotation = "0 0 1 " @ (getRandom() * 360);
   };
   MissionCleanup.add(%item);
	// take pack out of inventory and unmount image
	%obj.decInventory(SatchelCharge, 1);
   %obj.throwObject(%item);
	//error("throwing satchel charge #" @ %item);
   %obj.thrownChargeId = %item;
   %item.sourceObject = %obj;
   %item.armed = false;
	%item.damaged = 0.0;
	%item.thwart = false;
	// arm itself 3 seconds after being thrown
   schedule(%item.getDatablock().armDelay, %item, "armSatchelCharge", %item);
   messageClient(%obj.client, 'MsgSatchelChargePlaced', "\c2Satchel charge deployed.");
}

function armSatchelCharge(%satchel)
{
	//error("satchel charge #" @ %satchel @ " armed!");
	%satchel.armed = true;
	// "deet deet deet" sound
	%satchel.playAudio(1, SatchelChargeActivateSound);
	// also need to play "antenna extending" animation
	%satchel.playThread(0, "deploy");
	%satchel.playThread(1, "activate");
}

function detonateSatchelCharge(%player)
{
	%satchelCharge = %player.thrownChargeId;
	// can't detonate the satchel charge if it isn't armed
	if(!%satchelCharge.armed)
		return;

	//error("Detonating satchel charge #" @ %satchelCharge @ " for player #" @ %player);

   if(%satchelCharge.getDamageState() !$= Destroyed)
   	%satchelCharge.setDamageState(Destroyed);

   if(isObject(%player))	
   	%player.thrownChargeId = 0;
}

function SatchelChargeThrown::onEnterLiquid(%data, %obj, %coverage, %type)
{
   // lava types
   if(%type >=4 && %type <= 6)
   {
      if(%obj.getDamageState() !$= "Destroyed")
      {
         %obj.armed = true;
         detonateSatchelCharge(%obj.sourceObject);
         return;
      }
   }
   
   // quickSand   
   if(%type == 7)
      if(isObject(%obj.sourceObject))
         %obj.sourceObject.thrownChargeId = 0;
      
  Parent::onEnterLiquid(%data, %obj, %coverage, %type);
}

function SatchelChargeImage::onMount(%data, %obj, %node)
{
	%obj.thrownChargeId = 0;
}

function SatchelChargeImage::onUnmount(%data, %obj, %node)
{
}

function SatchelChargeThrown::onDestroyed(%this, %object, %lastState)
{
	if(%object.kaboom)
		return;
	else
	{
		%object.kaboom = true;

		// the "thwart" flag is set if the charge is destroyed with weapons rather
		// than detonated. A less damaging explosion, but visually the same scale.
		if(%object.thwart)
		{
			%dmgRadius = 10;
			%dmgMod = 0.3;
			%expImpulse = 1000;
		}
		else
		{
			messageClient(%object.sourceObject.client, 'msgSatchelChargeDetonate', "\c2Satchel Charge Detonated!");
			%dmgRadius = 20;
			%dmgMod = 1.0;
			%expImpulse = 2500;
		}

	   RadiusExplosion(%object, %object.getPosition(), %dmgRadius, %dmgMod, %expImpulse, %object.sourceObject, $DamageType::SatchelCharge);

		%object.schedule(500, "delete");
	}
}

function SatchelChargeThrown::onCollision(%data,%obj,%col)
{
}

function SatchelChargeThrown::damageObject(%data, %targetObject, %sourceObject, %position, %amount, %damageType)
{
	%targetObject.damaged += %amount;

   if(%targetObject.damaged >= %targetObject.getDataBlock().maxDamage && 
                  %satchelCharge.getDamageState() !$= Destroyed)
   {
		%targetObject.thwart = true;
		%targetObject.setDamageState(Destroyed);
	}
}

function SatchelCharge::onPickup(%this, %obj, %shape, %amount)
{
	// created to prevent console errors
}