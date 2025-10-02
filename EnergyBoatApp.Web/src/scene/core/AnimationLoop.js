/**
 * AnimationLoop Module
 * 
 * Main animation loop handling water animation, boat movement, buoy bobbing,
 * keyboard controls, and rendering. Synchronized with speed multiplier.
 * 
 * Phase 6 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { updateCameraFromKeyboard } from '../controls/KeyboardControls.js';

/**
 * Animates water shader time uniform
 * 
 * @param {THREE.Mesh} ocean - Ocean mesh with Water material
 * @param {number} speedMultiplier - Current speed multiplier
 */
export function animateWater(ocean, speedMultiplier) {
  ocean.material.uniforms['time'].value += (1.0 / 60.0) * speedMultiplier;
}

/**
 * Animates boat with bobbing and rocking motion
 * 
 * @param {THREE.Group} boatMesh - Boat mesh to animate
 * @param {number} time - Current time in seconds
 */
export function animateBoat(boatMesh, time) {
  if (!boatMesh) return;
  
  // Vertical bobbing motion
  const bobbingOffset = Math.sin(time * 2 + boatMesh.position.x) * 0.15;
  boatMesh.position.y = 1.5 + bobbingOffset;
  
  // Rocking motion synchronized with waves
  boatMesh.rotation.z = Math.sin(time * 1.5 + boatMesh.position.x) * 0.05;
  boatMesh.rotation.x = Math.cos(time * 1.2 + boatMesh.position.z) * 0.03;
}

/**
 * Animates all boats in the scene
 * 
 * @param {Object} boatMeshes - Object containing boat meshes by ID
 * @param {number} time - Current time in seconds
 */
export function animateAllBoats(boatMeshes, time) {
  Object.values(boatMeshes).forEach((boatMesh) => {
    animateBoat(boatMesh, time);
  });
}

/**
 * Animates buoy with bobbing and rotation
 * 
 * @param {THREE.Object3D} buoy - Buoy object to animate
 * @param {number} time - Current time in seconds
 */
export function animateBuoy(buoy, time) {
  if (buoy.userData.bobOffset === undefined) return;
  
  // Vertical bobbing motion
  const buoyBob = Math.sin(time * 1.5 + buoy.userData.bobOffset) * 0.2;
  buoy.position.y = 0.5 + buoyBob;
  
  // Slight rotation with waves
  buoy.rotation.z = Math.sin(time + buoy.userData.bobOffset) * 0.1;
}

/**
 * Animates all buoys in the scene
 * 
 * @param {THREE.Scene} scene - Scene containing buoys
 * @param {number} time - Current time in seconds
 */
export function animateAllBuoys(scene, time) {
  scene.traverse((object) => {
    if (object.userData.bobOffset !== undefined) {
      animateBuoy(object, time);
    }
  });
}

/**
 * Creates main animation loop
 * 
 * @param {Object} params - Animation parameters
 * @param {THREE.Scene} params.scene - Scene to render
 * @param {THREE.Camera} params.camera - Camera for rendering
 * @param {THREE.WebGLRenderer} params.renderer - Renderer
 * @param {Object} params.controls - OrbitControls instance
 * @param {THREE.Mesh} params.ocean - Ocean mesh
 * @param {Object} params.boatMeshes - Boat meshes by ID
 * @param {Object} params.keysPressed - Keyboard state ref
 * @param {Object} params.speedMultiplierRef - Speed multiplier ref
 * @returns {Function} Animation loop function
 */
export function createAnimationLoop({
  scene,
  camera,
  renderer,
  controls,
  ocean,
  boatMeshes,
  keysPressed,
  speedMultiplierRef,
}) {
  const animate = () => {
    requestAnimationFrame(animate);

    // Update water animation (read current speed from ref)
    animateWater(ocean, speedMultiplierRef.current);

    // Handle keyboard camera movement
    updateCameraFromKeyboard(camera, controls, keysPressed);

    // Animate boats with bobbing motion
    const time = performance.now() * 0.001; // Convert to seconds
    animateAllBoats(boatMeshes, time);

    // Animate buoys
    animateAllBuoys(scene, time);

    // Update controls and render
    controls.update();
    renderer.render(scene, camera);
  };

  return animate;
}

/**
 * Starts the animation loop
 * 
 * @param {Function} animateFunction - Animation function to start
 */
export function startAnimation(animateFunction) {
  animateFunction();
}

export default {
  animateWater,
  animateBoat,
  animateAllBoats,
  animateBuoy,
  animateAllBuoys,
  createAnimationLoop,
  startAnimation,
};
