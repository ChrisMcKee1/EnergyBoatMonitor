/**
 * CameraControls Module
 * 
 * Creates and configures Three.js OrbitControls for camera manipulation.
 * Handles mouse-based camera rotation, panning, and zooming.
 * 
 * Phase 5 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';

/**
 * Creates and configures OrbitControls
 * 
 * @param {THREE.Camera} camera - The camera to control
 * @param {HTMLElement} domElement - The renderer's DOM element
 * @returns {OrbitControls} Configured OrbitControls instance
 */
export function createOrbitControls(camera, domElement) {
  const controls = new OrbitControls(camera, domElement);
  
  // Enable all control features
  controls.enabled = true;
  controls.enableDamping = true;
  controls.dampingFactor = 0.08;
  controls.enableZoom = true;
  controls.enableRotate = true;
  controls.enablePan = true;
  
  // Set distance limits
  controls.minDistance = 15;
  controls.maxDistance = 150;
  
  // Prevent camera from going underground
  controls.maxPolarAngle = Math.PI / 2.1;
  
  // Set initial target (center of scene)
  controls.target.set(0, 0, 0);
  
  // Configure mouse button mappings
  controls.mouseButtons = {
    LEFT: THREE.MOUSE.ROTATE,
    MIDDLE: THREE.MOUSE.DOLLY,
    RIGHT: THREE.MOUSE.PAN
  };
  
  // Configure touch gesture mappings
  controls.touches = {
    ONE: THREE.TOUCH.ROTATE,
    TWO: THREE.TOUCH.DOLLY_PAN
  };
  
  // Apply initial update
  controls.update();
  
  return controls;
}

/**
 * Attaches control event listeners for debugging (optional)
 * 
 * @param {OrbitControls} controls - OrbitControls instance
 */
export function attachControlsDebugListeners(controls) {
  // Debug listeners removed to reduce console noise
  // Uncomment if needed for debugging:
  // controls.addEventListener('start', () => console.log('OrbitControls START'));
  // controls.addEventListener('change', () => console.log('OrbitControls CHANGE'));
  // controls.addEventListener('end', () => console.log('OrbitControls END'));
}

/**
 * Resets camera to initial position and target
 * 
 * @param {THREE.Camera} camera - The camera to reset
 * @param {OrbitControls} controls - OrbitControls instance
 * @param {Object} initialPosition - Initial camera position {x, y, z}
 * @param {Object} initialTarget - Initial target position {x, y, z}
 */
export function resetCamera(camera, controls, initialPosition = { x: 30, y: 20, z: 30 }, initialTarget = { x: 0, y: 0, z: 0 }) {
  camera.position.set(initialPosition.x, initialPosition.y, initialPosition.z);
  controls.target.set(initialTarget.x, initialTarget.y, initialTarget.z);
  controls.update();
}

/**
 * Disposes OrbitControls and cleans up resources
 * 
 * @param {OrbitControls} controls - OrbitControls instance to dispose
 */
export function disposeControls(controls) {
  if (controls) {
    controls.dispose();
  }
}

export default {
  createOrbitControls,
  attachControlsDebugListeners,
  resetCamera,
  disposeControls,
};
