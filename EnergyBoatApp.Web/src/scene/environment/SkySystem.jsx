/**
 * SkySystem Component
 * 
 * Manages the procedural sky using Preetham atmospheric scattering.
 * Includes day/night toggle functionality with smooth transitions.
 * 
 * Phase 2 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { Sky } from 'three/examples/jsm/objects/Sky.js';

/**
 * Sky configuration presets
 */
export const SKY_PRESETS = {
  DAYTIME: {
    turbidity: 10,        // Atmospheric haziness
    rayleigh: 3,          // Blue sky scattering
    mieCoefficient: 0.005, // Atmospheric particles
    mieDirectionalG: 0.7,  // Sun glow
    sunElevation: 15,     // 15 degrees above horizon - late afternoon
    sunAzimuth: 180,      // South-facing sun
    sunLightIntensity: 1.5,
    toneMappingExposure: 0.5,
  },
  NIGHTTIME: {
    turbidity: 2,
    rayleigh: 0.5,
    mieCoefficient: 0.05,
    mieDirectionalG: 0.7,
    sunElevation: -10,    // Below horizon
    sunAzimuth: 180,
    sunLightIntensity: 0.2,
    toneMappingExposure: 0.45, // Increased from 0.2 for better nighttime visibility
  }
};

/**
 * Creates and configures the sky system
 * 
 * @param {THREE.Scene} scene - The Three.js scene
 * @param {boolean} isDaytime - Initial daytime state
 * @returns {Object} Object containing sky, sunLight, sun position, and moon
 */
export function createSkySystem(scene, isDaytime = true) {
  // Create procedural sky using Preetham atmospheric scattering
  const sky = new Sky();
  sky.scale.setScalar(10000);
  
  // Make sky render FIRST and don't write to depth buffer
  // This allows other objects to render in front of it
  sky.material.depthWrite = false;
  sky.renderOrder = -1;
  
  scene.add(sky);

  const sun = new THREE.Vector3();

  // Apply initial preset
  const preset = isDaytime ? SKY_PRESETS.DAYTIME : SKY_PRESETS.NIGHTTIME;
  applySkyPreset(sky, sun, preset);

  // Create main sun light - positioned to match sky sun
  const sunLight = new THREE.DirectionalLight(0xFFFAF0, preset.sunLightIntensity);
  sunLight.position.copy(sun).multiplyScalar(5000); // Increased from 100 to 5000 to match moon distance
  sunLight.castShadow = true;
  sunLight.shadow.mapSize.width = 2048;
  sunLight.shadow.mapSize.height = 2048;
  sunLight.shadow.camera.near = 0.5;
  sunLight.shadow.camera.far = 500;
  sunLight.shadow.camera.left = -100;
  sunLight.shadow.camera.right = 100;
  sunLight.shadow.camera.top = 100;
  sunLight.shadow.camera.bottom = -100;
  scene.add(sunLight);

  // Create moon - pass sun vector so moon can be positioned relative to it
  const moon = createMoon(scene, sun);
  moon.visible = !isDaytime; // Only visible at night
  
  console.log('🌙 Initial moon state - visible:', moon.visible, 'isDaytime:', isDaytime);
  console.log('🌙 Moon world position:', moon.position);
  console.log('⚠️  Camera typically looks at origin (0,0,0) from position (30,25,30)');
  console.log('⚠️  Moon is high in sky - you need to LOOK UP or move camera to see it!');

  return { sky, sunLight, sun, moon };
}

/**
 * Applies a sky preset to the sky mesh
 * 
 * @param {Sky} sky - The Sky mesh
 * @param {THREE.Vector3} sun - The sun position vector (will be updated)
 * @param {Object} preset - Sky preset configuration
 */
function applySkyPreset(sky, sun, preset) {
  const skyUniforms = sky.material.uniforms;
  skyUniforms['turbidity'].value = preset.turbidity;
  skyUniforms['rayleigh'].value = preset.rayleigh;
  skyUniforms['mieCoefficient'].value = preset.mieCoefficient;
  skyUniforms['mieDirectionalG'].value = preset.mieDirectionalG;

  // Position sun using spherical coordinates
  const phi = THREE.MathUtils.degToRad(90 - preset.sunElevation);
  const theta = THREE.MathUtils.degToRad(preset.sunAzimuth);
  sun.setFromSphericalCoords(1, phi, theta);
  skyUniforms['sunPosition'].value.copy(sun);
}

/**
 * Creates a realistic moon mesh
 * Position it opposite the sun during nighttime
 * 
 * @param {THREE.Scene} scene - The Three.js scene
 * @param {THREE.Vector3} sun - The sun position vector from sky system
 * @returns {THREE.Mesh} Moon mesh
 */
function createMoon(scene, sun) {
  // Create moon geometry - realistic size for moon at distance
  const moonGeometry = new THREE.SphereGeometry(50, 32, 32); // Scaled back from 200 to 50 for realistic size
  
  // Use MeshStandardMaterial with strong emissive to make it glow
  const moonMaterial = new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,      // Pure white
    emissive: 0xFFFFFF,   // Emits white light
    emissiveIntensity: 5, // Bright glow
    roughness: 1,
    metalness: 0,
    depthWrite: true,     // Ensure it writes to depth buffer
  });
  
  const moon = new THREE.Mesh(moonGeometry, moonMaterial);
  
  // Position moon on SAME side as sun (south) so visual position matches light direction
  // At night: sun is at elevation -10° (below horizon), moon at same azimuth but above horizon
  // CRITICAL: Camera at (30,25,30) looks at origin (0,0,0) - that's looking at horizon level
  // Moon must be at LOW elevation to be visible in camera view!
  const moonElevation = 15; // Low elevation - near horizon where camera can see it
  const moonAzimuth = 180; // South - SAME side as sun (was 0/North - opposite)
  
  const phi = THREE.MathUtils.degToRad(90 - moonElevation);
  const theta = THREE.MathUtils.degToRad(moonAzimuth);
  
  // Position farther back - increased from 1000 to 5000 for better perspective
  const moonDistance = 5000;
  moon.position.setFromSphericalCoords(moonDistance, phi, theta);
  
  // Render after sky (which is at -1), so moon appears in front
  moon.renderOrder = 1000; // Very high renderOrder to ensure it's drawn last (on top)
  
  console.log('🌙 Moon created - material:', moonMaterial.type);
  console.log('🌙 Moon emissive intensity:', moonMaterial.emissiveIntensity);
  console.log('🌙 Moon position:', {x: moon.position.x.toFixed(2), y: moon.position.y.toFixed(2), z: moon.position.z.toFixed(2)});
  console.log('🌙 Moon distance from origin:', moon.position.length().toFixed(2));
  console.log('🌙 Moon size (radius):', 50, 'renderOrder:', moon.renderOrder);
  console.log('🌙 Sky renderOrder:', -1, 'Sky depthWrite:', false);
  
  scene.add(moon);
  return moon;
}

/**
 * Toggles between day and night
 * 
 * @param {Sky} sky - The Sky mesh
 * @param {THREE.DirectionalLight} sunLight - The main sun light
 * @param {THREE.WebGLRenderer} renderer - The renderer for tone mapping
 * @param {THREE.Vector3} sun - The sun position vector
 * @param {THREE.Mesh} moon - The moon mesh
 * @param {boolean} currentIsDaytime - Current daytime state
 * @returns {boolean} New daytime state (toggled)
 */
export function toggleDayNight(sky, sunLight, renderer, sun, moon, currentIsDaytime) {
  const newIsDaytime = !currentIsDaytime;
  const preset = newIsDaytime ? SKY_PRESETS.DAYTIME : SKY_PRESETS.NIGHTTIME;

  // Apply sky preset
  applySkyPreset(sky, sun, preset);

  // Update sun light
  sunLight.intensity = preset.sunLightIntensity;
  sunLight.position.copy(sun).multiplyScalar(5000); // Match moon distance (was 100)

  // Update tone mapping
  renderer.toneMappingExposure = preset.toneMappingExposure;

  // Toggle moon visibility and update position
  if (moon) {
    moon.visible = !newIsDaytime; // Visible at night only
    
    console.log('🌙🌙🌙 TOGGLING - newIsDaytime:', newIsDaytime, 'moon.visible:', moon.visible);
    
    // Update moon position opposite to current sun position
    if (!newIsDaytime) {
      // At night, position moon on SAME side as sun (south) so light direction matches visual
      const moonElevation = 15; // Low elevation - camera looks at horizon, not up!
      const moonAzimuth = 180; // South - SAME side as sun (was 0/North)
      const moonDistance = 5000; // Match initial moon distance (was 1000)
      
      const phi = THREE.MathUtils.degToRad(90 - moonElevation);
      const theta = THREE.MathUtils.degToRad(moonAzimuth);
      moon.position.setFromSphericalCoords(moonDistance, phi, theta);
      
      console.log('🌙 Moon repositioned to:', {x: moon.position.x.toFixed(2), y: moon.position.y.toFixed(2), z: moon.position.z.toFixed(2)});
    }
    
    // Force material to update
    moon.material.needsUpdate = true;
    
    console.log('🌙 Moon visibility after toggle:', moon.visible, 'isDaytime:', newIsDaytime);
    console.log('🌙 Moon position:', {x: moon.position.x.toFixed(2), y: moon.position.y.toFixed(2), z: moon.position.z.toFixed(2)});
    console.log('🌙 Moon in scene:', moon.parent !== null);
    console.log('🌙 Moon material emissiveIntensity:', moon.material.emissiveIntensity);
  }

  return newIsDaytime;
}

/**
 * Creates ambient and fill lights for the scene
 * 
 * @param {THREE.Scene} scene - The Three.js scene
 */
export function createSceneLights(scene) {
  // Ambient light for overall brightness
  const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
  scene.add(ambientLight);

  // Additional fill light for better visibility
  const fillLight = new THREE.DirectionalLight(0xFFFFFF, 0.3);
  fillLight.position.set(-50, 40, -30);
  scene.add(fillLight);

  return { ambientLight, fillLight };
}

export default {
  createSkySystem,
  toggleDayNight,
  createSceneLights,
  SKY_PRESETS,
};
