$HandInvThrowTimeout = 0.8 * 1000; // 1/2 second between throwing grenades or mines

$WeaponsHudData[0, bitmapName] = "gui/hud_blaster.png";
$WeaponsHudData[0, itemDataName] = "Blaster";
//$WeaponsHudData[0, ammoDataName] = "";
$WeaponsHudData[1, bitmapName] = "gui/hud_plasma.png";
$WeaponsHudData[1, itemDataName] = "Plasma";
$WeaponsHudData[1, ammoDataName] = "PlasmaAmmo";
$WeaponsHudData[2, bitmapName] = "gui/hud_chaingun.png";
$WeaponsHudData[2, itemDataName] = "Chaingun";
$WeaponsHudData[2, ammoDataName] = "ChaingunAmmo";
$WeaponsHudData[3, bitmapName] = "gui/hud_disc.png";
$WeaponsHudData[3, itemDataName] = "Disc";
$WeaponsHudData[3, ammoDataName] = "DiscAmmo";
$WeaponsHudData[4, bitmapName] = "gui/hud_grenlaunch.png";
$WeaponsHudData[4, itemDataName] = "GrenadeLauncher";
$WeaponsHudData[4, ammoDataName] = "GrenadeLauncherAmmo";
$WeaponsHudData[5, bitmapName] = "gui/hud_sniper.png";
$WeaponsHudData[5, itemDataName] = "SniperRifle";
//$WeaponsHudData[5, ammoDataName] = "";
$WeaponsHudData[6, bitmapName] = "gui/hud_elfgun.png";
$WeaponsHudData[6, itemDataName] = "ELFGun";
//$WeaponsHudData[6, ammoDataName] = "";
$WeaponsHudData[7, bitmapName] = "gui/hud_mortor.png";
$WeaponsHudData[7, itemDataName] = "Mortar";
$WeaponsHudData[7, ammoDataName] = "MortarAmmo";
$WeaponsHudData[8, bitmapName] = "gui/hud_missiles.png";
$WeaponsHudData[8, itemDataName] = "MissileLauncher";
$WeaponsHudData[8, ammoDataName] = "MissileLauncherAmmo";
// WARNING!!! If you change the weapon index of the targeting laser,
// you must change the HudWeaponInvBase::addWeapon function to test
// for the new value!
$WeaponsHudData[9, bitmapName]   = "gui/hud_targetlaser.png";
$WeaponsHudData[9, itemDataName] = "TargetingLaser";
//$WeaponsHudData[9, ammoDataName] = "";
//
$WeaponsHudData[10, bitmapName]   = "gui/hud_shocklance.png";
$WeaponsHudData[10, itemDataName] = "ShockLance";
//$WeaponsHudData[10, ammoDataName] = "";


$WeaponsHudCount = 11;


$AmmoIncrement[PlasmaAmmo]          = 10;
$AmmoIncrement[ChaingunAmmo]        = 25;
$AmmoIncrement[DiscAmmo]            = 5;
$AmmoIncrement[GrenadeLauncherAmmo] = 5;
$AmmoIncrement[MortarAmmo]          = 5;
$AmmoIncrement[MissileLauncherAmmo] = 2;
$AmmoIncrement[Mine]                = 3;
$AmmoIncrement[Grenade]             = 5;
$AmmoIncrement[FlashGrenade]        = 5;
$AmmoIncrement[FlareGrenade]        = 5;
$AmmoIncrement[ConcussionGrenade]   = 5;
$AmmoIncrement[RepairKit]           = 1;

//----------------------------------------------------------------------------
// Weapons scripts
//--------------------------------------

// --- Mounting weapons
exec("scripts/weapons/blaster.cs");
exec("scripts/weapons/plasma.cs");
exec("scripts/weapons/chaingun.cs");
exec("scripts/weapons/disc.cs");
exec("scripts/weapons/grenadeLauncher.cs");
exec("scripts/weapons/sniperRifle.cs");
exec("scripts/weapons/ELFGun.cs");
exec("scripts/weapons/mortar.cs");
exec("scripts/weapons/missileLauncher.cs");
exec("scripts/weapons/targetingLaser.cs");
exec("scripts/weapons/shockLance.cs");

// --- Throwing weapons
exec("scripts/weapons/mine.cs");
exec("scripts/weapons/grenade.cs");
exec("scripts/weapons/flashGrenade.cs");
exec("scripts/weapons/flareGrenade.cs");
exec("scripts/weapons/concussionGrenade.cs");
exec("scripts/weapons/cameraGrenade.cs");

//----------------------------------------------------------------------------

function Weapon::onUse(%data, %obj)
{
   if(Game.weaponOnUse(%data, %obj))
      if (%obj.getDataBlock().className $= Armor)
         %obj.mountImage(%data.image, $WeaponSlot);
}

function WeaponImage::onMount(%this,%obj,%slot)
{
   //MES -- is call below useful at all?
   //Parent::onMount(%this, %obj, %slot);
   if(%obj.getClassName() !$= "Player")
      return;

   //messageClient(%obj.client, 'MsgWeaponMount', "", %this, %obj, %slot);
   // Looks arm position
   if (%this.armthread $= "")
   {
      %obj.setArmThread(look);
   }
   else
   {
      %obj.setArmThread(%this.armThread);
   }
   
   // Initial ammo state
   if(%obj.getMountedImage($WeaponSlot).ammo !$= "")
      if (%obj.getInventory(%this.ammo))
         %obj.setImageAmmo(%slot,true);

   %obj.client.setWeaponsHudActive(%this.item);
   if(%obj.getMountedImage($WeaponSlot).ammo !$= "")
      %obj.client.setAmmoHudCount(%obj.getInventory(%this.ammo));
   else
      %obj.client.setAmmoHudCount(-1);
}

function WeaponImage::onUnmount(%this,%obj,%slot)
{
   %obj.client.setWeaponsHudActive(%this.item, 1);
   %obj.client.setAmmoHudCount(-1);
   // try to avoid running around with sniper/missile arm thread and no weapon
   %obj.setArmThread(look);
   Parent::onUnmount(%this, %obj, %slot);
}

function Ammo::onInventory(%this,%obj,%amount)
{
   // Loop through and make sure the images using this ammo have
   // their ammo states set.
   for (%i = 0; %i < 8; %i++) {
      %image = %obj.getMountedImage(%i);
      if (%image > 0)
      {
         if (isObject(%image.ammo) && %image.ammo.getId() == %this.getId())
            %obj.setImageAmmo(%i,%amount != 0);
      }
   }
   ItemData::onInventory(%this,%obj,%amount);
   if(%obj.getClassname() $= "Player")
   {
      %obj.client.setWeaponsHudAmmo(%this.getName(), %amount);
      if(%obj.getMountedImage($WeaponSlot).ammo $= %this.getName())
         %obj.client.setAmmoHudCount(%amount);
   }
}

function Weapon::onInventory(%this,%obj,%amount)
{
   if(Game.weaponOnInventory(%this, %obj, %amount))
   {
      %obj.client.setWeaponsHudItem(%this.getName(), 0, 1);   
      ItemData::onInventory(%this,%obj,%amount);
      // if a player threw a weapon (which means that player isn't currently
      // holding a weapon), set armthread to "no weapon"
		// MES - taken out to avoid v-menu animation problems (bug #4749)
      //if((%amount == 0) && (%obj.getClassName() $= "Player"))
      //   %obj.setArmThread(looknw);
   }
}

function Weapon::onPickup(%this, %obj, %shape, %amount)
{
   // If player doesn't have a weapon in hand, use this one...
   if ( %shape.getClassName() $= "Player" 
     && %shape.getMountedImage( $WeaponSlot ) == 0 )
      %shape.use( %this.getName() );
}

function HandInventory::onInventory(%this,%obj,%amount)
{
   // prevent console errors when throwing ammo pack
   if(%obj.getClassName() $= "Player")
      %obj.client.setInventoryHudAmount(%this.getName(), %amount);
   ItemData::onInventory(%this,%obj,%amount);
}

function HandInventory::onUse(%data, %obj)
{
   // %obj = player  %data = datablock of what's being thrown
   if(Game.handInvOnUse(%data, %obj))
   {
      //AI HOOK - If you change the %throwStren, tell Tinman!!!
      //Or edit aiInventory.cs and search for: use(%grenadeType);

      %tossTimeout = getSimTime() - %obj.lastThrowTime[%data];
      if(%tossTimeout < $HandInvThrowTimeout)
         return;

      %throwStren = %obj.throwStrength;

      %obj.decInventory(%data, 1);
      %thrownItem = new Item()
      {
         dataBlock = %data.thrownItem;
         sourceObject = %obj;
      };
      MissionCleanup.add(%thrownItem);
      
      // throw it
      %eye = %obj.getEyeVector();
      %vec = vectorScale(%eye, (%throwStren * 20.0));
      
      // add a vertical component to give it a better arc
      %dot = vectorDot("0 0 1", %eye);
      if(%dot < 0)
         %dot = -%dot;
      %vec = vectorAdd(%vec, vectorScale("0 0 4", 1 - %dot));
      
      // add player's velocity
      %vec = vectorAdd(%vec, vectorScale(%obj.getVelocity(), 0.4));
      %pos = getBoxCenter(%obj.getWorldBox());
      

      %thrownItem.sourceObject = %obj;
      %thrownItem.setTransform(%pos);
      
      %thrownItem.applyImpulse(%pos, %vec);
      %thrownItem.setCollisionTimeout(%obj);
      serverPlay3D(GrenadeThrowSound, %pos);
      %obj.lastThrowTime[%data] = getSimTime();

      %thrownItem.getDataBlock().onThrow(%thrownItem);
      %obj.throwStrength = 0;
   }
}

function HandInventoryImage::onMount(%this,%obj,%slot)
{
   messageClient(%col.client, 'MsgHandInventoryMount', "", %this, %obj, %slot);
   // Looks arm position
   if (%this.armthread $= "")
      %obj.setArmThread(look);
   else
      %obj.setArmThread(%this.armThread);
   
   // Initial ammo state
   if (%obj.getInventory(%this.ammo))
      %obj.setImageAmmo(%slot,true);

   %obj.client.setWeaponsHudActive(%this.item);
}

function Weapon::incCatagory(%data, %obj)
{
   // Don't count the targeting laser as a weapon slot:
   if ( %data.getName() !$= "TargetingLaser" )
      %obj.weaponCount++;   
}

function Weapon::decCatagory(%data, %obj)
{
   // Don't count the targeting laser as a weapon slot:
   if ( %data.getName() !$= "TargetingLaser" )
      %obj.weaponCount--;   
}

function SimObject::damageObject(%data)
{
   //function was added to reduce console err msg spam
}

