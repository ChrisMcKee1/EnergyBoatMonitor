/**
 * OceanEnvironment Component
 * 
 * Manages the ocean water surface with realistic Water shader.
 * Includes wave animations synchronized with speed multiplier.
 * 
 * Phase 2 of refactoring - extracted from BoatScene.jsx
 */

import { useEffect, useRef } from 'react';
import * as THREE from 'three';
import { Water } from 'three/examples/jsm/objects/Water.js';

/**
 * Creates and manages the ocean water surface
 * 
 * @param {THREE.Scene} scene - The Three.js scene to add ocean to
 * @param {THREE.Vector3} sunPosition - Sun position for water reflections
 * @param {number} speedMultiplier - Speed multiplier for wave animations
 * @returns {THREE.Mesh} The ocean water mesh
 */
export function createOcean(scene, sunPosition, speedMultiplier = 1.0) {
  const waterGeometry = new THREE.PlaneGeometry(10000, 10000);
  
  // Load water normal map for realistic wave details
  const textureLoader = new THREE.TextureLoader();
  const waterNormals = textureLoader.load(
    'https://threejs.org/examples/textures/waternormals.jpg',
    (texture) => {
      texture.wrapS = texture.wrapT = THREE.RepeatWrapping;
    }
  );

  const water = new Water(waterGeometry, {
    textureWidth: 512,
    textureHeight: 512,
    waterNormals: waterNormals,
    sunDirection: sunPosition.clone().normalize(),
    sunColor: 0xffffff,
    waterColor: 0x2C7DA0,  // Ocean blue
    distortionScale: 3.7,
    fog: false,
    alpha: 1.0,
  });

  water.rotation.x = -Math.PI / 2;
  water.receiveShadow = true;
  water.userData.isOcean = true;
  
  return water;
}

/**
 * Updates ocean wave animation
 * Should be called in animation loop
 * 
 * @param {THREE.Mesh} ocean - The ocean mesh
 * @param {number} speedMultiplier - Speed multiplier for wave animations
 */
export function updateOceanAnimation(ocean, speedMultiplier = 1.0) {
  if (!ocean || !ocean.material || !ocean.material.uniforms) return;
  
  // Update Water shader time uniform for animated waves
  // Multiply by speedMultiplier so water moves faster/slower with simulation speed
  ocean.material.uniforms['time'].value += (1.0 / 60.0) * speedMultiplier;
}

/**
 * Hook-based wrapper for React integration
 * Use this in BoatScene.jsx instead of direct function calls
 */
export function useOceanEnvironment(sceneRef, sunPosition, speedMultiplier) {
  const oceanRef = useRef(null);

  useEffect(() => {
    if (!sceneRef.current) return;

    // Create ocean and add to scene
    const ocean = createOcean(sceneRef.current, sunPosition, speedMultiplier);
    oceanRef.current = ocean;
    sceneRef.current.add(ocean);

    // Cleanup
    return () => {
      if (sceneRef.current && oceanRef.current) {
        sceneRef.current.remove(oceanRef.current);
        oceanRef.current.geometry.dispose();
        oceanRef.current.material.dispose();
      }
    };
  }, [sceneRef]); // Only create once

  return oceanRef;
}

export default useOceanEnvironment;
