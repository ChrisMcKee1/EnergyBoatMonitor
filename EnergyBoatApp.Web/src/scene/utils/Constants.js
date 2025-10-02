/**
 * Scene-wide constants for the 3D maritime visualization
 */

// Geographic center point (London)
export const CENTER_LAT = 51.5074;
export const CENTER_LON = -0.1278;

// Map boundaries (North Sea area)
// Expanded to give waypoints buffer zone (boats navigate to edges of this area)
export const BOUNDS = {
  minLat: 51.48,
  maxLat: 51.53,
  minLon: -0.16,
  maxLon: -0.09,  // Expanded from -0.10 to give BOAT-001 waypoint buffer room
};

// Scale factors for lat/lon to 3D coordinates
// 1 degree difference = 2000 units in scene
export const SCALE_FACTOR_LAT = 2000;
export const SCALE_FACTOR_LON = 2000;

// Dock location and orientation
export const DOCK_LAT = 51.5100;
export const DOCK_LON = -0.1350;
export const DOCK_ROTATION = Math.PI / 4; // 45 degrees

// Camera initial position
export const CAMERA_INITIAL_POSITION = { x: 30, y: 25, z: 30 };
export const CAMERA_INITIAL_TARGET = { x: 0, y: 0, z: 0 };

// Boat status colors
export const BOAT_COLORS = {
  Active: 0xFF4500,      // Contoso-Sea orange-red
  Charging: 0xFFAA00,    // Yellow-orange
  Maintenance: 0x888888,  // Gray
  default: 0xFF4500,
};

// Status light colors
export const STATUS_LIGHT_COLORS = {
  Active: 0x00FF00,      // Green
  Charging: 0xFFFF00,    // Yellow
  Maintenance: 0xFF0000,  // Red
  default: 0x00FF00,
};

// Navigation light colors
export const NAV_LIGHT_COLORS = {
  port: 0xFF0000,        // Red (left)
  starboard: 0x00FF00,   // Green (right)
  masthead: 0xFFFFFF,    // White (top)
};

// Material properties for PBR rendering
export const MATERIALS = {
  metal: {
    metalness: 0.9,
    roughness: 0.1,
  },
  weatheredMetal: {
    metalness: 0.7,
    roughness: 0.4,
  },
  weatheredWood: {
    color: 0x8B7355,
    metalness: 0.1,
    roughness: 0.9,
  },
  agedWood: {
    color: 0x5C4033,
    metalness: 0.1,
    roughness: 0.95,
  },
  concrete: {
    color: 0x888888,
    metalness: 0.2,
    roughness: 0.8,
  },
  glass: {
    color: 0x87CEEB,
    transparent: true,
    opacity: 0.4,
    metalness: 0.5,
    roughness: 0.1,
  },
};

// Sky shader parameters
export const SKY_PARAMS = {
  daytime: {
    turbidity: 10,
    rayleigh: 3,
    mieCoefficient: 0.005,
    mieDirectionalG: 0.7,
    elevation: 15,      // degrees above horizon
    azimuth: 180,       // south-facing
    sunIntensity: 1.5,
    exposure: 0.5,
  },
  nighttime: {
    turbidity: 2,
    rayleigh: 0.5,
    mieCoefficient: 0.05,
    mieDirectionalG: 0.7,
    elevation: -10,     // degrees below horizon
    azimuth: 180,
    sunIntensity: 0.2,
    exposure: 0.2,
  },
};

// Water shader parameters
export const WATER_PARAMS = {
  textureWidth: 512,
  textureHeight: 512,
  waterColor: 0x2C7DA0,  // Ocean blue
  distortionScale: 3.7,
  alpha: 1.0,
};

// Animation speeds
export const ANIMATION = {
  waterSpeed: 1.0 / 60.0,
  boatBobbingSpeed: 2.0,
  boatBobbingAmplitude: 0.15,
  boatRockingSpeedZ: 1.5,
  boatRockingAmplitudeZ: 0.05,
  boatRockingSpeedX: 1.2,
  boatRockingAmplitudeX: 0.03,
  buoyBobbingSpeed: 1.5,
  buoyBobbingAmplitude: 0.2,
  buoyRotationAmplitude: 0.1,
  cameraLerpSpeed: 0.05,
};

// Keyboard movement
export const KEYBOARD = {
  moveSpeed: 0.5,  // units per frame
};
