/**
 * DockStructure Module
 * 
 * Assembles complete dock structure by combining platform, equipment, and building.
 * Handles positioning, rotation, and integration with scene coordinate system.
 * 
 * Phase 4 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { CoordinateConverter } from '../utils/CoordinateConverter.js';
import { createCompleteDockPlatform } from './DockPlatform.js';
import { createCompleteDockEquipment } from './DockEquipment.js';
import { createCompleteDockBuilding } from './DockBuilding.js';
import {
  createWoodMaterial,
  createDarkWoodMaterial,
  createPilingMaterial,
  createBeamMaterial,
  createCleatMaterial,
  createRopeMaterial,
  createLifeRingMaterial,
  createWhiteStripeMaterial,
  createLightPoleMaterial,
  createLightFixtureMaterial,
  createLadderMaterial,
  createWarningSignMaterial,
  createFenderMaterial,
  createConcreteMaterial,
  createWallMaterial,
  createRoofMaterial,
  createWindowFrameMaterial,
  createGlassMaterial,
  createDoorMaterial,
  createDoorHandleMaterial,
  createDockSignMaterial,
  createAntennaMaterial,
  createDishMaterial,
} from '../vessels/BoatMaterials.js';

/**
 * Creates all materials needed for dock construction
 * 
 * @returns {Object} Object containing all dock materials
 */
export function createDockMaterials() {
  return {
    // Platform materials
    wood: createWoodMaterial(),
    darkWood: createDarkWoodMaterial(),
    piling: createPilingMaterial(),
    beam: createBeamMaterial(),
    
    // Equipment materials
    cleat: createCleatMaterial(),
    rope: createRopeMaterial(),
    lifeRing: createLifeRingMaterial(),
    whiteStripe: createWhiteStripeMaterial(),
    lightPole: createLightPoleMaterial(),
    lightFixture: createLightFixtureMaterial(),
    ladder: createLadderMaterial(),
    sign: createWarningSignMaterial(),
    fender: createFenderMaterial(),
    
    // Building materials
    concrete: createConcreteMaterial(),
    wall: createWallMaterial(),
    roof: createRoofMaterial(),
    windowFrame: createWindowFrameMaterial(),
    glass: createGlassMaterial(),
    door: createDoorMaterial(),
    doorHandle: createDoorHandleMaterial(),
    dockSign: createDockSignMaterial(),
    antenna: createAntennaMaterial(),
    dish: createDishMaterial(),
  };
}

/**
 * Creates complete dock structure with all components
 * Positioned at London coordinates (51.5100°N, -0.1350°W)
 * 
 * @returns {THREE.Group} Complete dock assembly
 */
export function createCompleteDock() {
  const dockGroup = new THREE.Group();
  
  // Create all materials
  const materials = createDockMaterials();
  
  // Create platform (planks, pilings, beams, braces)
  const platform = createCompleteDockPlatform({
    wood: materials.wood,
    darkWood: materials.darkWood,
    piling: materials.piling,
    beam: materials.beam,
    brace: materials.piling, // Use piling material for braces
  });
  dockGroup.add(platform);
  
  // Create equipment (cleats, ropes, rings, lights, ladder, sign, fenders)
  const equipment = createCompleteDockEquipment(materials);
  dockGroup.add(equipment);
  
  // Create building (foundation, walls, roof, windows, door, sign, antenna, dish)
  const building = createCompleteDockBuilding(materials);
  dockGroup.add(building);
  
  // Position dock at London coordinates
  const dockLatLon = { latitude: 51.5100, longitude: -0.1350 };
  const dockPos = CoordinateConverter.latLonToScene(dockLatLon);
  dockGroup.position.set(dockPos.x, 0, dockPos.z);
  
  // Rotate for visual interest
  dockGroup.rotation.y = Math.PI / 4;
  
  return dockGroup;
}

/**
 * Disposes dock structure and cleans up resources
 * 
 * @param {THREE.Group} dockGroup - Dock group to dispose
 */
export function disposeDock(dockGroup) {
  if (!dockGroup) return;
  
  dockGroup.traverse((child) => {
    if (child.isMesh) {
      if (child.geometry) {
        child.geometry.dispose();
      }
      if (child.material) {
        if (Array.isArray(child.material)) {
          child.material.forEach(material => material.dispose());
        } else {
          child.material.dispose();
        }
      }
    }
  });
  
  dockGroup.clear();
}

export default {
  createDockMaterials,
  createCompleteDock,
  disposeDock,
};
