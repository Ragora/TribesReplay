//-------------------------------------- Desert interior texture property mapping

addMaterialMapping("ice/sw_ichute01", "environment: special/chuteTexture 0.25");
addMaterialMapping("ice/sw_ichute02", "environment: special/chuteTexture 0.25");

//"Color: red green blue startAlpha endAlpha"
addMaterialMapping("terrain/IceWorld.Ice",      "color: 0.9 0.9 0.9 0.4 0.0");
addMaterialMapping("terrain/IceWorld.RockBlue", "color: 0.9 0.9 0.9 0.4 0.0");
addMaterialMapping("terrain/IceWorld.Snow",     "color: 0.9 0.9 0.9 0.4 0.0");
addMaterialMapping("terrain/IceWorld.SnowIce",  "color: 0.9 0.9 0.9 0.4 0.0");
addMaterialMapping("terrain/IceWorld.SnowRock", "color: 0.9 0.9 0.9 0.4 0.0");

//Soft  sound = 0
//Hard  sound = 1
//Metal sound = 2
//Snow  sound = 3
addMaterialMapping("terrain/IceWorld.Ice",      "sound: 3");
addMaterialMapping("terrain/IceWorld.RockBlue", "sound: 3");
addMaterialMapping("terrain/IceWorld.Snow",     "sound: 3");
addMaterialMapping("terrain/IceWorld.SnowIce",  "sound: 3");
addMaterialMapping("terrain/IceWorld.SnowRock", "sound: 3");
