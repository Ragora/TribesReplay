
//--------------------------------------
// Audio Descriptions
//

//datablock AudioDescription(TestLooping3d)
//{
//   volume   = 0.5;
//   isLooping= true;
//
//   is3D     = true;
//   minDistance= 10.0;
//   MaxDistance= 640.0;
//   type     = $EffectAudioType;
//};

datablock AudioDescription(ProjectileLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   minDistance= 5.0;
   MaxDistance= 20.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(ClosestLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   minDistance= 5.0;
   MaxDistance= 30.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(CloseLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   minDistance= 10.0;
   MaxDistance= 50.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioDefaultLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   minDistance= 20.0;
   MaxDistance= 100.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioClosest3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 5.0;
   MaxDistance= 30.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioClose3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 10.0;
   MaxDistance= 60.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioDefault3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 20.0;
   MaxDistance= 100.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioExplosion3d)
// Regular weapon explosions
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 20.0;
   MaxDistance= 150.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioBomb3d)
// Regular weapon explosions
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 80.0;
   MaxDistance= 250.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioBIGExplosion3d)
// Big explosions like mortars, vehicles, and satchel charges
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   minDistance= 50.0;
   MaxDistance= 250.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioLooping2D)
// Used for Looping Environmental Sounds
{
   volume = 1.0;
   isLooping = true;
   is3D = false;
   type = $EffectAudioType;
};

datablock AudioDescription(Audio2D)
// Used for non-looping environmental sounds (like power on, power off)
{
   volume = 1.0;
   isLooping = false;
   is3D = false;
   type = $EffectAudioType;
};


