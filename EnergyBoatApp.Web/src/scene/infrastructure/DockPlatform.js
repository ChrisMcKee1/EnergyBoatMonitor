/**
 * DockPlatform Module
 * 
 * Creates the main dock platform structure including planks, pilings, and beams.
 * Provides the foundation for the dock infrastructure.
 * 
 * Phase 4 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates wooden planks for dock surface
 * 
 * @param {THREE.Material} woodMaterial - Regular wood material
 * @param {THREE.Material} darkWoodMaterial - Dark aged wood material
 * @returns {THREE.Group} Group containing all planks
 */
export function createDockPlanks(woodMaterial, darkWoodMaterial) {
  const planksGroup = new THREE.Group();
  
  for (let i = 0; i < 25; i++) {
    const plankGeometry = new THREE.BoxGeometry(0.35, 0.15, 8);
    const material = i % 3 === 0 ? darkWoodMaterial : woodMaterial;
    const plank = new THREE.Mesh(plankGeometry, material);
    plank.position.set(-4.5 + i * 0.4, 0.5, 0);
    plank.castShadow = true;
    plank.receiveShadow = true;
    planksGroup.add(plank);
  }
  
  return planksGroup;
}

/**
 * Creates vertical support pilings that extend underwater
 * 
 * @param {THREE.Material} pilingMaterial - Piling material
 * @returns {THREE.Group} Group containing all pilings
 */
export function createDockPilings(pilingMaterial) {
  const pilingsGroup = new THREE.Group();
  
  for (let i = 0; i < 6; i++) {
    const pilingGeometry = new THREE.CylinderGeometry(0.25, 0.28, 5, 16);
    
    // Near side pilings
    const piling = new THREE.Mesh(pilingGeometry, pilingMaterial);
    piling.position.set(-4 + i * 2, -1.75, 2.5);
    piling.castShadow = true;
    piling.receiveShadow = true;
    pilingsGroup.add(piling);
    
    // Far side pilings
    const piling2 = piling.clone();
    piling2.position.set(-4 + i * 2, -1.75, -2.5);
    pilingsGroup.add(piling2);
  }
  
  return pilingsGroup;
}

/**
 * Creates horizontal support beams
 * 
 * @param {THREE.Material} beamMaterial - Beam material
 * @returns {THREE.Group} Group containing support beams
 */
export function createSupportBeams(beamMaterial) {
  const beamsGroup = new THREE.Group();
  
  for (let i = 0; i < 2; i++) {
    const beamGeometry = new THREE.BoxGeometry(10, 0.3, 0.3);
    const beam = new THREE.Mesh(beamGeometry, beamMaterial);
    beam.position.set(0, 0.2, i === 0 ? 2.5 : -2.5);
    beam.castShadow = true;
    beamsGroup.add(beam);
  }
  
  return beamsGroup;
}

/**
 * Creates diagonal cross braces underwater
 * 
 * @param {THREE.Material} braceMaterial - Brace material
 * @returns {THREE.Group} Group containing cross braces
 */
export function createCrossBraces(braceMaterial) {
  const bracesGroup = new THREE.Group();
  
  for (let i = 0; i < 5; i++) {
    const braceGeometry = new THREE.BoxGeometry(0.2, 0.2, 5.5);
    const brace = new THREE.Mesh(braceGeometry, braceMaterial);
    brace.rotation.y = Math.PI / 6;
    brace.position.set(-3.5 + i * 2, -1, 0);
    bracesGroup.add(brace);
  }
  
  return bracesGroup;
}

/**
 * Creates complete dock platform structure
 * 
 * @param {Object} materials - Object containing all required materials
 * @returns {THREE.Group} Complete platform assembly
 */
export function createCompleteDockPlatform(materials) {
  const platformGroup = new THREE.Group();
  
  // Add planks
  const planks = createDockPlanks(materials.wood, materials.darkWood);
  platformGroup.add(planks);
  
  // Add pilings
  const pilings = createDockPilings(materials.piling);
  platformGroup.add(pilings);
  
  // Add support beams
  const beams = createSupportBeams(materials.darkWood);
  platformGroup.add(beams);
  
  // Add cross braces
  const braces = createCrossBraces(materials.piling);
  platformGroup.add(braces);
  
  return platformGroup;
}

export default {
  createDockPlanks,
  createDockPilings,
  createSupportBeams,
  createCrossBraces,
  createCompleteDockPlatform,
};
