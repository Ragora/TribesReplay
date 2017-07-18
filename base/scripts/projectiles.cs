//--------------------------------------------------------------------------
// Projectiles.cs: Note that the actual projectile blocks are stored with
//                  with weapon that uses them in base/scripts/weapons/*.cs,
//                  the blocks below are only to illustrate the default values
//                  for each block type.  Also, ProjectileData cannot be used
//                  as a concrete datablock type.
//  Inheritance:
//   ProjectileData            : GameBaseData
//   LinearProjectileData      : ProjectileData
//   LinearFlareProjectileData : LinearProjectileData
//   GrenadeProjectileData     : ProjectileData
//   SeekerProjectileData      : ProjectileData
//   SniperProjectileData      : ProjectileData
// 
//--------------------------------------------------------------------------
//-------------------------------------- Default functions
//
function ProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
   %targetObject.damage(%projectile.sourceObject, %position, %data.directDamage * %modifier, %data.directDamageType);
}

function SniperProjectileData::onCollision(%data, %projectile, %targetObject, %modifier, %position, %normal)
{
   %damageAmount = %data.directDamage * %projectile.damageFactor;
   
   if(%targetObject.getDataBlock().getClassName() $= "PlayerData")
   {
      %damLoc = firstWord(%targetObject.getDamageLocation(%position));
      if(%damLoc $= "head")
      {   
         %targetObject.getOwnerClient().headShot = 1;
         %modifier = %data.rifleHeadMultiplier;
      }
      else
      {   
         %modifier = 1;
         %targetObject.getOwnerClient().headShot = 0;
      }
   }
   %targetObject.damage(%projectile.sourceObject, %position, %modifier * %damageAmount, %data.directDamageType);
}

function TurretImageData::onFire( %data, %obj, %slot )
{
   if( %data.muzzleFlash !$= "" )
   {
      %flash = new Shockwave()
      {
         dataBlock = %data.muzzleFlash;
         pos = %obj.getMuzzlePoint(%slot);
         normal = %obj.getMuzzleVector(%slot);
      };
   }
   
   Parent::onFire( %data, %obj, %slot );
}

function ShapeBaseImageData::onFire(%data, %obj, %slot)
{
   %data.lightStart = getSimTime();

   if( %obj.station $= "" && %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
         %obj.respawnCloakThread = "";
      }
      else
      {
         if( %obj.getEnergyLevel() > 20 )
         {   
            %obj.setCloaked( false );
            %obj.reCloak = %obj.schedule( 500, "setCloaked", true ); 
         }
      }   
   }
   
   %vehicle = 0;
   if(%data.usesEnergy)
   {
      if(%data.useMountEnergy)
      {
         %useEnergyObj = %obj.getObjectMount();
         if(!%useEnergyObj)
            %useEnergyObj = %obj;
         %energy = %useEnergyObj.getEnergyLevel();
         %vehicle = %useEnergyObj;
      }
      else
         %energy = %obj.getEnergyLevel();
      if(%energy < %data.minEnergy)
         return;
   }
   if(%data.projectileSpread)
   {
      %vector = %obj.getMuzzleVector(%slot);
      %x = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %y = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %z = (getRandom() - 0.5) * 2 * 3.1415926 * %data.projectileSpread;
      %mat = MatrixCreateFromEuler(%x @ " " @ %y @ " " @ %z);
      %vector = MatrixMulVector(%mat, %vector);

      %p = new (%data.projectileType)() {
         dataBlock        = %data.projectile;
         initialDirection = %vector;
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         sourceSlot       = %slot;
         vehicleObject    = %vehicle;
      };
   }
   else
   {
      %p = new (%data.projectileType)() {
         dataBlock        = %data.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         sourceSlot       = %slot;
         vehicleObject    = %vehicle;
      };
   }
	if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
	   %obj.lastProjectile.delete();
   %obj.lastProjectile = %p;
   MissionCleanup.add(%p);
   
   // AI hook
   if(%obj.client)
      %obj.client.projectile = %p;

   if(%data.usesEnergy)
   {
      if(%data.useMountEnergy)
         %useEnergyObj.setEnergyLevel(%energy - %data.fireEnergy);
      else
         %obj.setEnergyLevel(%energy - %data.fireEnergy);
   }
   else
      %obj.decInventory(%data.ammo,1);
   return %p;
}

function ShapeBaseImageData::onUnmount(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function TurretImageData::deconstruct(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function ShapeBaseImageData::deconstruct(%data, %obj, %slot)
{
   if (%data.deleteLastProjectile && isObject(%obj.lastProjectile))
   {
      %obj.lastProjectile.delete();
      %obj.lastProjectile = "";
   }
}

function MissileLauncherImage::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   MissileSet.add(%p);
   
   %target = %obj.getLockedTarget();
   if(%target)
      %p.setObjectTarget(%target);
   else if(%obj.isLocked())
      %p.setPositionTarget(%obj.getLockedPosition());
   else
      %p.setNoTarget();
}

function MissileLauncherImage::onWetFire(%data, %obj, %slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   MissileSet.add(%p);
   
   %p.setObjectTarget(0);
}

//--------------------------------------------------------------------------

function MissileBarrelLarge::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data,%obj,%slot);

   if (%obj.getControllingClient())
   {
      // a player is controlling the turret
      %target = %obj.getLockedTarget();
   }
   else
   {
      // The ai is controlling the turret
      %target = %obj.getTargetObject();
   }
   
   if(%target)
      %p.setObjectTarget(%target);
   else if(%obj.isLocked())
      %p.setPositionTarget(%obj.getLockedPosition());
   else
      %p.setNoTarget(); // set as unguided. Only happens when itchy trigger can't wait for lock tone.    
}

//add mortars to the "grenade set" so the AI's can avoid them better...
function MortarImage::onFire(%data,%obj,%slot)
{
	%p = Parent::onFire(%data, %obj, %slot);
	AIGrenadeThrown(%p);
}

function SniperRifleImage::onFire(%data,%obj,%slot)
{
   if(!%obj.hasEnergyPack)
   {
      // siddown Junior, you can't use it
      serverPlay3D(SniperRifleDryFireSound, %obj.getTransform());
      return;
   }
   
   %pct = %obj.getEnergyLevel() / %obj.getDataBlock().maxEnergy;
   %p = new (%data.projectileType)() {
      dataBlock        = %data.projectile;
      initialDirection = %obj.getMuzzleVector(%slot);
      initialPosition  = %obj.getMuzzlePoint(%slot);
      sourceObject     = %obj;
      damageFactor     = %pct * %pct;
      sourceSlot       = %slot;
   };
   %p.setEnergyPercentage(%pct);

   %obj.lastProjectile = %p;
   MissionCleanup.add(%p);
   serverPlay3D(SniperRifleFireSound, %obj.getTransform());
   
   // AI hook
   if(%obj.client)
      %obj.client.projectile = %p;

   %obj.setEnergyLevel(0);
}

function ElfGunImage::onFire(%data, %obj, %slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   
   if(!%p.hasTarget())
      %obj.playAudio(0, ELFFireWetSound);
}

function TargetingLaserImage::onFire(%data,%obj,%slot)
{
   %p = Parent::onFire(%data, %obj, %slot);
   %p.setTarget(%obj.team);
}

function ShockLanceImage::onFire(%this, %obj, %slot)
{
   if( %obj.isCloaked() )
   {
      if( %obj.respawnCloakThread !$= "" )
      {
         Cancel(%obj.respawnCloakThread);
         %obj.setCloaked( false );
      }
      else
      {
         if( %obj.getEnergyLevel() > 20 )
         {   
            %obj.setCloaked( false );
            %obj.reCloak = %obj.schedule( 500, "setCloaked", true ); 
         }
      }   
   }
   
   %muzzlePos = %obj.getMuzzlePoint(%slot);
   %muzzleVec = %obj.getMuzzleVector(%slot);

   %endPos    = VectorAdd(%muzzlePos, VectorScale(%muzzleVec, %this.projectile.extension));

   %damageMasks = $TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType |
      $TypeMasks::StationObjectType | $TypeMasks::GeneratorObjectType |
      $TypeMasks::SensorObjectType | $TypeMasks::TurretObjectType;

   %everythingElseMask = $TypeMasks::TerrainObjectType |
                         $TypeMasks::InteriorObjectType |
                         $TypeMasks::ForceFieldObjectType |
                         $TypeMasks::StaticObjectType |
                         $TypeMasks::MoveableObjectType |
                         $TypeMasks::PathedObjectType |
                         $TypeMasks::DamagableItemObjectType;

   // did I miss anything? players, vehicles, stations, gens, sensors, turrets
   %hit = ContainerRayCast(%muzzlePos, %endPos, %damageMasks | %everythingElseMask, %obj);

   %noDisplay = true;
   
   if (%hit !$= "0")
   {
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.hitEnergy);

      %hitobj = getWord(%hit, 0);
      %hitpos = getWord(%hit, 1) @ " " @ getWord(%hit, 2) @ " " @ getWord(%hit, 3);

      if ( %hitObj.getType() & %damageMasks )
      {
         %hitobj.applyImpulse(%hitpos, VectorScale(%muzzleVec, %this.projectile.impulse));
         %obj.playAudio(0, ShockLanceHitSound);

         // This is truly lame, but we need the sourceobject property present...
         %p = new ShockLanceProjectile() {
            dataBlock        = %this.projectile;
            initialDirection = %obj.getMuzzleVector(%slot);
            initialPosition  = %obj.getMuzzlePoint(%slot);
            sourceObject     = %obj;
            sourceSlot       = %slot;
            targetId         = %hit;
         };
         MissionCleanup.add(%p);

         %damageMultiplier = 1.0;
         
         if(%hitObj.getDataBlock().getClassName() $= "PlayerData")
         {
            // Now we see if we hit from behind...
            %eyeVector = %hitobj.getEyeVector();

            %eyeXForm  = %hitobj.getEyeTransform();
            %eyePos    = getWord(%eyeXForm, 0) @ " " @ getWord(%eyeXForm, 1) @ " " @ getWord(%eyeXForm, 2);
            %dif       = VectorSub(%eyePos, %muzzlePos);
            %dif       = getWord(%dif, 0) @ " " @ getWord(%dif, 1) @ " 0";
            %dif       = VectorNormalize(%dif);
            %eyeVector = getWord(%eyeVector, 0) @ " " @ getWord(%eyeVector, 1) @ " 0";
            %dot       = VectorDot(%dif, %eyeVector);
                       
            // 120 Deg angle test...
            if (%dot > mCos(1.05)) {
               // Rear hit
               %damageMultiplier = 3.0;
            }
         }
         
         %totalDamage = %this.Projectile.DirectDamage * %damageMultiplier;
         %hitObj.getDataBlock().damageObject(%hitobj, %p.sourceObject, %hitpos, %totalDamage, $DamageType::ShockLance);

         %noDisplay = false;
      }
   } 
   
   if( %noDisplay )
   {
      // Miss
      %obj.setEnergyLevel(%obj.getEnergyLevel() - %this.missEnergy);
      %obj.playAudio(0, ShockLanceMissSound);

      %p = new ShockLanceProjectile() {
         dataBlock        = %this.projectile;
         initialDirection = %obj.getMuzzleVector(%slot);
         initialPosition  = %obj.getMuzzlePoint(%slot);
         sourceObject     = %obj;
         sourceSlot       = %slot;
      };
      MissionCleanup.add(%p);

   }
}

$ELFZapSound = 2;
$ELFFireSound = 3;

function ELFProjectileData::zapTarget(%data, %projectile, %target, %targeter)
{
   %oldERate = %target.getRechargeRate();
   %target.setRechargeRate(%oldERate - %data.drainEnergy);
   %projectile.checkELFStatus(%data, %target, %targeter);
}

function ELFProjectileData::unzapTarget(%data, %projectile, %target, %targeter)
{
   cancel(%projectile.ELFrecur);
	%target.stopAudio($ELFZapSound);
   %targeter.stopAudio($ELFFireSound);
	%target.zapSound = false;
	%targeter.zappingSound = false;
   if(!%target.isDisabled())
   {
      %oldERate = %target.getRechargeRate();
      %target.setRechargeRate(%oldERate + %data.drainEnergy);
   }
}

function ELFProjectile::checkELFStatus(%this, %data, %target, %targeter)
{
   if(isObject(%target))
   {
      %enLevel = %target.getEnergyLevel();
      if(%enLevel < 1.0)
      {
         %dataBlock = %target.getDataBlock();
         %dataBlock.damageObject(%target, %this.sourceObject, %target.getPosition(), %data.drainHealth, %data.directDamageType); 
         
      }
      else
      {
         %normal = "0.0 0.0 1.0";
         %target.playShieldEffect( %normal );
      }
      %this.ELFrecur = %this.schedule(32, checkELFStatus, %data, %target, %targeter);
      
      %targeter.playAudio($ELFFireSound, ELFGunFireSound);
      if(!%target.zapSound)
		{
         %target.playAudio($ELFZapSound, ELFHitTargetSound);
			%target.zapSound = true;
			%targeter.zappingSound = true;
		}
   }
	else if(%targeter.zappingSound)
	{
		%targeter.stopAudio($ELFFireSound);
		%targeter.zappingSound = false;
	}
}


function RadiusExplosion(%explosionSource, %position, %radius, %damage, %impulse, %sourceObject, %damageType)
{
   InitContainerRadiusSearch(%position, %radius, $TypeMasks::PlayerObjectType      |
                                                 $TypeMasks::VehicleObjectType     |
                                                 $TypeMasks::MoveableObjectType    |
                                                 $TypeMasks::StaticShapeObjectType |
                                                 $TypeMasks::ForceFieldObjectType  |
                                                 $TypeMasks::TurretObjectType      |
                                                 $TypeMasks::ItemObjectType);

   %numTargets = 0;
   while ((%targetObject = containerSearchNext()) != 0)
   {
      %dist = containerSearchCurrRadDamageDist();

      if (%dist > %radius)
         continue;

      if (%targetObject.isMounted())
      {
         %mount = %targetObject.getObjectMount();
         %found = -1;
         for (%i = 0; %i < %mount.getDataBlock().numMountPoints; %i++)
         {
            if (%mount.getMountNodeObject(%i) == %targetObject)
            {
               %found = %i;
               break;
            }
         }
      
         if (%found != -1)
         {
            if (%mount.getDataBlock().isProtectedMountPoint[%found])
            {
               continue;
            }
         }
      }

      %targets[%numTargets]     = %targetObject;
      %targetDists[%numTargets] = %dist;
      %numTargets++;
   }

   for (%i = 0; %i < %numTargets; %i++)
   {
      %targetObject = %targets[%i];
      %dist = %targetDists[%i];

      %coverage = calcExplosionCoverage(%position, %targetObject,
                                        ($TypeMasks::InteriorObjectType |
                                         $TypeMasks::TerrainObjectType |
                                         $TypeMasks::ForceFieldObjectType |
                                         $TypeMasks::VehicleObjectType));
      if (%coverage == 0)
         continue;

      %amount = (1.0 - (%dist / %radius)) * %coverage * %damage;
      %data = %targetObject.getDataBlock();
      %className = %data.className;

      if (%impulse && %data.shouldApplyImpulse(%targetObject))
      {
         %p = %targetObject.getWorldBoxCenter();
         %momVec = VectorSub(%p, %position);
         %momVec = VectorNormalize(%momVec);
         %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
         %doImpulse = true;
      }
      else if( %className $= WheeledVehicleData || %className $= FlyingVehicleData || %className $= HoverVehicleData )
      {
         %p = %targetObject.getWorldBoxCenter();
         %momVec = VectorSub(%p, %position);
         %momVec = VectorNormalize(%momVec);
         
         %impulseVec = VectorScale(%momVec, %impulse * (1.0 - (%dist / %radius)));
         
         if( getWord( %momVec, 2 ) < -0.5 )
            %momVec = "0 0 1";
            
         // Add obj's velocity into the momentum vector
         %velocity = %targetObject.getVelocity();
         //%momVec = VectorNormalize( vectorAdd( %momVec, %velocity) );
         %doImpulse = true;
      }
      else
      {   
         %momVec = "0 0 1";
         %doImpulse = false;
      }
      
      if(%amount > 0)
         %data.damageObject(%targetObject, %sourceObject, %position, %amount, %damageType, %momVec);
      else if( %explosionSource.getDataBlock().getName() $= "ConcussionGrenadeThrown" && %data.getClassName() $= "PlayerData" )
         %data.applyConcussion( %dist, %radius, %sourceObject, %targetObject ); 
      
      if( %doImpulse )
         %targetObject.applyImpulse(%position, %impulseVec);
   }
}

function ProjectileData::onExplode(%data, %proj, %pos, %mod)
{
   if (%data.hasDamageRadius)
      RadiusExplosion(%proj, %pos, %data.damageRadius, %data.indirectDamage, %data.kickBackStrength, %proj.sourceObject, %data.radiusDamageType);
}

function Flag::shouldApplyImpulse(%data, %obj)
{
   if(%obj.isHome)
      return false;
   else
      return true;
}
