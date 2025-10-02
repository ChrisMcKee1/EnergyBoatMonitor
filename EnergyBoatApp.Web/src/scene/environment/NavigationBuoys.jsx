/**
 * NavigationBuoys Component
 * 
 * Creates navigation buoys at boundary markers with animated bobbing.
 * Buoys mark the operational area boundaries in the maritime scene.
 * 
 * Phase 2 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { CoordinateConverter } from '../utils/CoordinateConverter.js';

/**
 * Creates a single navigation buoy
 * 
 * @returns {THREE.Group} Buoy group with body, stripe, and light
 */
export function createBuoy() {
  const buoyGroup = new THREE.Group();
  
  // Buoy body (floating)
  const bodyGeometry = new THREE.CylinderGeometry(0.4, 0.5, 1.5, 16);
  const bodyMaterial = new THREE.MeshStandardMaterial({
    color: 0xFF6600,  // Navigation orange
    metalness: 0.4,
    roughness: 0.6,
  });
  const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
  body.castShadow = true;
  body.receiveShadow = true;
  buoyGroup.add(body);

  // White stripe
  const stripeGeometry = new THREE.CylinderGeometry(0.41, 0.51, 0.3, 16);
  const stripeMaterial = new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.3,
    roughness: 0.7,
  });
  const stripe = new THREE.Mesh(stripeGeometry, stripeMaterial);
  stripe.position.y = 0.3;
  buoyGroup.add(stripe);

  // Navigation light on top
  const lightGeometry = new THREE.SphereGeometry(0.2, 16, 16);
  const lightMaterial = new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    emissive: 0xFFFFFF,
    emissiveIntensity: 0.8,
  });
  const light = new THREE.Mesh(lightGeometry, lightMaterial);
  light.position.y = 1;
  buoyGroup.add(light);

  // Point light for atmospheric effect
  const pointLight = new THREE.PointLight(0xFFFFFF, 0.5, 10);
  pointLight.position.y = 1;
  buoyGroup.add(pointLight);

  return buoyGroup;
}

/**
 * Default boundary points for North Sea area around London
 * Updated to match expanded BOUNDS in Constants.js
 */
export const DEFAULT_BOUNDARY_POINTS = [
  { lat: 51.48, lon: -0.16 },   // Southwest
  { lat: 51.48, lon: -0.09 },   // Southeast (expanded from -0.10)
  { lat: 51.53, lon: -0.16 },   // Northwest
  { lat: 51.53, lon: -0.09 },   // Northeast (expanded from -0.10)
];

/**
 * Creates and places buoys at boundary corners
 * 
 * @param {THREE.Scene} scene - The Three.js scene
 * @param {Array} boundaryPoints - Array of {lat, lon} boundary points
 * @param {Object} coordinateConfig - Coordinate conversion configuration
 * @returns {Array<THREE.Group>} Array of buoy groups
 */
export function createBoundaryBuoys(
  scene,
  boundaryPoints = DEFAULT_BOUNDARY_POINTS,
  coordinateConfig = {}
) {
  const buoys = [];

  boundaryPoints.forEach((point, index) => {
    const buoy = createBuoy();
    
    // Convert lat/lon to scene coordinates
    const sceneCoords = CoordinateConverter.latLonToScene({
      latitude: point.lat,
      longitude: point.lon
    });
    
    buoy.position.x = sceneCoords.x;
    buoy.position.y = 0.5;  // Floating on water
    buoy.position.z = sceneCoords.z;
    
    // Add slight bobbing animation offset per buoy
    // This creates a wave-like pattern across all buoys
    buoy.userData.bobOffset = index * Math.PI / 2;
    buoy.userData.isBuoy = true; // Mark for animation
    
    scene.add(buoy);
    buoys.push(buoy);
  });

  return buoys;
}

/**
 * Animates buoy bobbing motion
 * Should be called in animation loop
 * 
 * @param {THREE.Group} buoy - The buoy to animate
 * @param {number} time - Current animation time
 * @param {number} amplitude - Bobbing amplitude (default: 0.2)
 * @param {number} frequency - Bobbing frequency (default: 1.5)
 */
export function animateBuoy(buoy, time, amplitude = 0.2, frequency = 1.5) {
  if (!buoy.userData.isBuoy) return;
  
  const bobOffset = buoy.userData.bobOffset || 0;
  const buoyBob = Math.sin(time * frequency + bobOffset) * amplitude;
  buoy.position.y = 0.5 + buoyBob;
}

/**
 * Animates all buoys in the scene
 * 
 * @param {THREE.Scene} scene - The scene containing buoys
 * @param {number} time - Current animation time
 */
export function animateAllBuoys(scene, time) {
  scene.traverse((object) => {
    if (object.userData.isBuoy) {
      animateBuoy(object, time);
    }
  });
}

export default {
  createBuoy,
  createBoundaryBuoys,
  animateBuoy,
  animateAllBuoys,
  DEFAULT_BOUNDARY_POINTS,
};
