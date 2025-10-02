/**
 * LightingSystem Module
 * 
 * Creates and manages scene lighting including ambient light, sun light, and fill light.
 * Synchronized with sky system for realistic day/night lighting.
 * 
 * Phase 6 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates ambient light for base illumination
 * 
 * @param {number} intensity - Light intensity (default: 0.6)
 * @returns {THREE.AmbientLight} Ambient light
 */
export function createAmbientLight(intensity = 0.6) {
  const ambientLight = new THREE.AmbientLight(0xffffff, intensity);
  return ambientLight;
}

/**
 * Creates main sun light with shadows
 * Positioned to match sky sun position
 * 
 * @param {THREE.Vector3} sunPosition - Sun position from sky system
 * @param {number} intensity - Light intensity (default: 1.5)
 * @returns {THREE.DirectionalLight} Sun light with shadow configuration
 */
export function createSunLight(sunPosition, intensity = 1.5) {
  const sunLight = new THREE.DirectionalLight(0xFFFAF0, intensity);
  
  // Position light to match sky sun
  sunLight.position.copy(sunPosition).multiplyScalar(100);
  
  // Configure shadows
  sunLight.castShadow = true;
  sunLight.shadow.mapSize.width = 2048;
  sunLight.shadow.mapSize.height = 2048;
  sunLight.shadow.camera.near = 0.5;
  sunLight.shadow.camera.far = 500;
  sunLight.shadow.camera.left = -100;
  sunLight.shadow.camera.right = 100;
  sunLight.shadow.camera.top = 100;
  sunLight.shadow.camera.bottom = -100;
  
  return sunLight;
}

/**
 * Creates fill light for softer shadows and better visibility
 * 
 * @param {number} intensity - Light intensity (default: 0.3)
 * @returns {THREE.DirectionalLight} Fill light
 */
export function createFillLight(intensity = 0.3) {
  const fillLight = new THREE.DirectionalLight(0xFFFFFF, intensity);
  fillLight.position.set(-50, 40, -30);
  return fillLight;
}

/**
 * Creates complete lighting system
 * 
 * @param {THREE.Scene} scene - Scene to add lights to
 * @param {THREE.Vector3} sunPosition - Sun position from sky system
 * @returns {Object} Object containing all light references
 */
export function createLightingSystem(scene, sunPosition) {
  const ambientLight = createAmbientLight();
  scene.add(ambientLight);
  
  const sunLight = createSunLight(sunPosition);
  scene.add(sunLight);
  
  const fillLight = createFillLight();
  scene.add(fillLight);
  
  return {
    ambient: ambientLight,
    sun: sunLight,
    fill: fillLight,
  };
}

/**
 * Updates sun light position based on sky sun position
 * 
 * @param {THREE.DirectionalLight} sunLight - Sun light to update
 * @param {THREE.Vector3} sunPosition - New sun position
 */
export function updateSunPosition(sunLight, sunPosition) {
  sunLight.position.copy(sunPosition).multiplyScalar(100);
}

/**
 * Updates ambient light intensity (for day/night transitions)
 * 
 * @param {THREE.AmbientLight} ambientLight - Ambient light to update
 * @param {number} intensity - New intensity value
 */
export function updateAmbientIntensity(ambientLight, intensity) {
  ambientLight.intensity = intensity;
}

/**
 * Updates sun light intensity (for day/night transitions)
 * 
 * @param {THREE.DirectionalLight} sunLight - Sun light to update
 * @param {number} intensity - New intensity value
 */
export function updateSunIntensity(sunLight, intensity) {
  sunLight.intensity = intensity;
}

export default {
  createAmbientLight,
  createSunLight,
  createFillLight,
  createLightingSystem,
  updateSunPosition,
  updateAmbientIntensity,
  updateSunIntensity,
};
