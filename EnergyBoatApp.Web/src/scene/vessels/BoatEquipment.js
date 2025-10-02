/**
 * BoatEquipment Module
 * 
 * Provides equipment and detail components for vessels.
 * Includes navigation/communication equipment, deck equipment, and navigation lights.
 * 
 * Phase 3 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates the main radar mast
 * 
 * @param {THREE.Material} material - Mast material
 * @returns {THREE.Mesh} Positioned mast mesh
 */
export function createMast(material) {
  const geometry = new THREE.CylinderGeometry(0.12, 0.15, 4.0, 16);
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0, 7.9, 0.8);  // Centered on superstructure
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates the radar dome
 * 
 * @param {THREE.Material} material - Radar material
 * @returns {THREE.Mesh} Positioned radar mesh
 */
export function createRadar(material) {
  const geometry = new THREE.SphereGeometry(0.4, 24, 24);
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0, 10.0, 0.8);  // Top of mast
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates communication antennas array
 * 
 * @param {THREE.Material} material - Antenna material
 * @returns {THREE.Group} Group containing 3 antennas
 */
export function createAntennas(material) {
  const antennaGroup = new THREE.Group();
  
  for (let i = 0; i < 3; i++) {
    const antennaGeometry = new THREE.CylinderGeometry(0.03, 0.03, 1.2, 8);
    const antenna = new THREE.Mesh(antennaGeometry, material);
    antenna.position.set((i - 1) * 0.3, 9.5, 1.2);  // Spread across width, behind mast
    antennaGroup.add(antenna);
  }
  
  return antennaGroup;
}

/**
 * Creates deck crane for ROV deployment
 * 
 * @param {THREE.Material} material - Crane material
 * @returns {THREE.Group} Group containing crane base and arm
 */
export function createCrane(material) {
  const craneGroup = new THREE.Group();
  
  // Crane base
  const craneBaseGeometry = new THREE.CylinderGeometry(0.3, 0.35, 1.2, 16);
  const craneBase = new THREE.Mesh(craneBaseGeometry, material);
  craneBase.position.set(-1.5, 2.8, 1.0);  // On port side, aft of midships
  craneBase.castShadow = true;
  craneGroup.add(craneBase);
  
  // Crane arm
  const craneArmGeometry = new THREE.BoxGeometry(0.2, 2.5, 0.2);
  const craneArm = new THREE.Mesh(craneArmGeometry, material);
  craneArm.rotation.z = -Math.PI / 6;
  craneArm.position.set(-0.5, 4.0, 1.0);
  craneArm.castShadow = true;
  craneGroup.add(craneArm);
  
  return craneGroup;
}

/**
 * Creates a single railing section
 * 
 * @param {number} x - X position
 * @param {number} y - Y position
 * @param {number} z - Z position
 * @param {number} length - Length of railing
 * @param {number} rotY - Y rotation in radians (default: 0)
 * @param {THREE.Material} material - Railing material
 * @returns {THREE.Group} Railing group with top rail and posts
 */
export function createRailing(x, y, z, length, rotY = 0, material) {
  const railingGroup = new THREE.Group();
  
  // Top rail
  const topRailGeometry = new THREE.BoxGeometry(0.05, 0.05, length);
  const topRail = new THREE.Mesh(topRailGeometry, material);
  topRail.position.y = 0.6;
  railingGroup.add(topRail);
  
  // Support posts
  const numPosts = Math.max(3, Math.floor(length / 0.8) + 1);
  for (let i = 0; i < numPosts; i++) {
    const postGeometry = new THREE.CylinderGeometry(0.02, 0.02, 0.6, 8);
    const post = new THREE.Mesh(postGeometry, material);
    const postSpacing = length / (numPosts - 1);
    post.position.set(0, 0.3, -length/2 + i * postSpacing);
    railingGroup.add(post);
  }
  
  railingGroup.position.set(x, y, z);
  railingGroup.rotation.y = rotY;
  return railingGroup;
}

/**
 * Creates port and starboard railings around deck perimeter
 * 
 * @param {THREE.Material} material - Railing material
 * @returns {THREE.Group} Group containing all deck railings
 */
export function createDeckRailings(material) {
  const railingsGroup = new THREE.Group();
  
  const deckWidth = 3.0;
  const deckLength = 7.5;
  const deckY = 2.25;
  
  // Port side (left, -Z)
  const portRailing = createRailing(0, deckY, -deckWidth / 2, deckLength, 0, material);
  railingsGroup.add(portRailing);
  
  // Starboard side (right, +Z)
  const starboardRailing = createRailing(0, deckY, deckWidth / 2, deckLength, 0, material);
  railingsGroup.add(starboardRailing);
  
  // Bow (front, +X)
  const bowRailing = createRailing(deckLength / 2, deckY, 0, deckWidth, Math.PI / 2, material);
  railingsGroup.add(bowRailing);
  
  // Stern (back, -X) - partial railings for helipad access
  const sternRailingLeft = createRailing(-deckLength / 2, deckY, -deckWidth / 4, deckWidth / 2, Math.PI / 2, material);
  railingsGroup.add(sternRailingLeft);
  
  const sternRailingRight = createRailing(-deckLength / 2, deckY, deckWidth / 4, deckWidth / 2, Math.PI / 2, material);
  railingsGroup.add(sternRailingRight);
  
  return railingsGroup;
}

/**
 * Creates navigation lights (port, starboard, masthead)
 * 
 * @param {THREE.Material} portMaterial - Red port light material
 * @param {THREE.Material} starboardMaterial - Green starboard light material
 * @param {THREE.Material} mastheadMaterial - White masthead light material
 * @returns {THREE.Group} Group containing all navigation lights
 */
export function createNavigationLights(portMaterial, starboardMaterial, mastheadMaterial) {
  const lightsGroup = new THREE.Group();
  const lightGeometry = new THREE.SphereGeometry(0.15, 16, 16);
  
  // Port light (red - left side)
  const portLight = new THREE.Mesh(lightGeometry, portMaterial);
  portLight.position.set(2.0, 3.8, -1.2);  // On port side of lower accommodation, forward
  portLight.userData.isNavLight = true;
  portLight.userData.type = 'port';
  lightsGroup.add(portLight);
  
  // Starboard light (green - right side)
  const starboardLight = new THREE.Mesh(lightGeometry, starboardMaterial);
  starboardLight.position.set(2.0, 3.8, 1.2);  // On starboard side of lower accommodation, forward
  starboardLight.userData.isNavLight = true;
  starboardLight.userData.type = 'starboard';
  lightsGroup.add(starboardLight);
  
  // Masthead light (white - top)
  const mastheadLight = new THREE.Mesh(lightGeometry, mastheadMaterial);
  mastheadLight.position.set(0.8, 9.5, 0);  // On mast, forward of radar
  mastheadLight.userData.isNavLight = true;
  mastheadLight.userData.type = 'masthead';
  lightsGroup.add(mastheadLight);
  
  return lightsGroup;
}

/**
 * Creates point lights for navigation lights (atmospheric effect)
 * 
 * @returns {THREE.Group} Group containing point lights
 */
export function createNavigationPointLights() {
  const pointLightsGroup = new THREE.Group();
  
  // Port point light (red)
  const portPointLight = new THREE.PointLight(0xff0000, 5, 5);
  portPointLight.position.set(2.0, 3.8, -1.2);
  portPointLight.userData.isNavLight = true;
  portPointLight.userData.type = 'port';
  pointLightsGroup.add(portPointLight);
  
  // Starboard point light (green)
  const starboardPointLight = new THREE.PointLight(0x00ff00, 5, 5);
  starboardPointLight.position.set(2.0, 3.8, 1.2);
  starboardPointLight.userData.isNavLight = true;
  starboardPointLight.userData.type = 'starboard';
  pointLightsGroup.add(starboardPointLight);
  
  // Masthead point light (white)
  const mastheadPointLight = new THREE.PointLight(0xffffff, 5, 8);
  mastheadPointLight.position.set(0.8, 9.5, 0);
  mastheadPointLight.userData.isNavLight = true;
  mastheadPointLight.userData.type = 'masthead';
  pointLightsGroup.add(mastheadPointLight);
  
  return pointLightsGroup;
}

/**
 * Creates status indicator light
 * 
 * @param {THREE.Material} material - Status light material
 * @param {number} color - Status color (hex)
 * @returns {Object} Object containing light mesh and point light
 */
export function createStatusLight(material, color) {
  const geometry = new THREE.SphereGeometry(0.2, 16, 16);
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0.8, 7.2, 0); // Centered on bridge, top
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates complete navigation and communication equipment assembly
 * 
 * @param {Object} materials - Object containing all required materials
 * @returns {THREE.Group} Complete equipment assembly
 */
export function createCompleteEquipment(materials) {
  const equipmentGroup = new THREE.Group();
  
  // Mast and radar
  const mast = createMast(materials.mast);
  const radar = createRadar(materials.radar);
  equipmentGroup.add(mast);
  equipmentGroup.add(radar);
  
  // Antennas
  const antennas = createAntennas(materials.antenna);
  equipmentGroup.add(antennas);
  
  // Crane
  const crane = createCrane(materials.crane);
  equipmentGroup.add(crane);
  
  // Railings
  const railings = createDeckRailings(materials.railing);
  equipmentGroup.add(railings);
  
  // Navigation lights
  const navLights = createNavigationLights(
    materials.portLight,
    materials.starboardLight,
    materials.mastheadLight
  );
  equipmentGroup.add(navLights);
  
  // Point lights
  const pointLights = createNavigationPointLights();
  equipmentGroup.add(pointLights);
  
  return equipmentGroup;
}

export default {
  createMast,
  createRadar,
  createAntennas,
  createCrane,
  createRailing,
  createDeckRailings,
  createNavigationLights,
  createNavigationPointLights,
  createStatusLight,
  createCompleteEquipment,
};
