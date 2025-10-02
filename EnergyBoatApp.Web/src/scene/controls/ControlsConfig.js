/**
 * ControlsConfig Module
 * 
 * Configuration constants for controls system.
 * Centralizes all control-related settings.
 * 
 * Phase 5 of refactoring - extracted from BoatScene.jsx
 */

/**
 * Camera control configuration
 */
export const CAMERA_CONFIG = {
  // Initial camera position
  initialPosition: { x: 30, y: 20, z: 30 },
  
  // Initial camera target
  initialTarget: { x: 0, y: 0, z: 0 },
  
  // OrbitControls settings
  orbitControls: {
    enableDamping: true,
    dampingFactor: 0.08,
    minDistance: 15,
    maxDistance: 150,
    maxPolarAngle: Math.PI / 2.1, // Prevent going underground
  },
};

/**
 * Keyboard control configuration
 */
export const KEYBOARD_CONFIG = {
  // Movement speed (units per frame)
  moveSpeed: 0.5,
  
  // Supported keys
  supportedKeys: ['w', 'a', 's', 'd', 'arrowup', 'arrowdown', 'arrowleft', 'arrowright'],
};

/**
 * Mouse button mappings for OrbitControls
 */
export const MOUSE_CONFIG = {
  LEFT: 'ROTATE',    // Left mouse button rotates camera
  MIDDLE: 'DOLLY',   // Middle mouse button zooms
  RIGHT: 'PAN',      // Right mouse button pans
};

/**
 * Touch gesture mappings for OrbitControls
 */
export const TOUCH_CONFIG = {
  ONE: 'ROTATE',        // One finger rotates camera
  TWO: 'DOLLY_PAN',     // Two fingers zoom and pan
};

export default {
  CAMERA_CONFIG,
  KEYBOARD_CONFIG,
  MOUSE_CONFIG,
  TOUCH_CONFIG,
};
