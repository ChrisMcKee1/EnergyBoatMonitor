/**
 * BoatMaterials Module
 * 
 * Provides PBR (Physically Based Rendering) materials for boat components.
 * All materials use MeshStandardMaterial with metalness/roughness workflow.
 * 
 * Phase 3 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { getBoatColor, getStatusLightColor } from '../utils/BoatHelpers.js';

/**
 * Boat Hull Materials
 */
export function createHullMaterial(status = 'Active') {
  return new THREE.MeshStandardMaterial({
    color: getBoatColor(status),
    metalness: 0.8,
    roughness: 0.3,
    envMapIntensity: 1.0,
  });
}

export function createStripeMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF6600, // Fugro orange
    metalness: 0.7,
    roughness: 0.4,
  });
}

/**
 * Deck Materials
 */
export function createDeckMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xCCCCCC,
    metalness: 0.6,
    roughness: 0.6,
  });
}

export function createHelipadMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFDD00,
    metalness: 0.3,
    roughness: 0.7,
  });
}

export function createHelipadMarkingMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF0000,
    metalness: 0.2,
    roughness: 0.8,
  });
}

/**
 * Superstructure Materials
 */
export function createAccommodationMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.3,
    roughness: 0.5,
  });
}

export function createBridgeMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xF0F0F0,
    metalness: 0.4,
    roughness: 0.4,
  });
}

export function createWindowMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x111111,
    metalness: 0.9,
    roughness: 0.1,
    transparent: true,
    opacity: 0.3,
  });
}

/**
 * Equipment Materials
 */
export function createMastMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xDDDDDD,
    metalness: 0.8,
    roughness: 0.2,
  });
}

export function createRadarMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.6,
    roughness: 0.3,
  });
}

export function createAntennaMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x888888,
    metalness: 0.7,
    roughness: 0.3,
  });
}

export function createCraneMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFAA00,
    metalness: 0.5,
    roughness: 0.5,
  });
}

export function createRailingMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xCCCCCC,
    metalness: 0.7,
    roughness: 0.4,
  });
}

/**
 * Navigation Light Materials
 */
export function createPortLightMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF0000, // Red (port side)
    emissive: 0xFF0000,
    emissiveIntensity: 0.8,
  });
}

export function createStarboardLightMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x00FF00, // Green (starboard side)
    emissive: 0x00FF00,
    emissiveIntensity: 0.8,
  });
}

export function createMastheadLightMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF, // White (masthead)
    emissive: 0xFFFFFF,
    emissiveIntensity: 1.0,
  });
}

export function createStatusLightMaterial(status = 'Active') {
  const statusColor = getStatusLightColor(status);
  return new THREE.MeshStandardMaterial({
    color: statusColor,
    emissive: statusColor,
    emissiveIntensity: 1.0,
  });
}

/**
 * Dock Materials
 */
export function createWoodMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x8B7355,
    metalness: 0,
    roughness: 0.9,
  });
}

export function createDarkWoodMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x654321,
    metalness: 0,
    roughness: 0.95,
  });
}

export function createPilingMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x3D2817,
    metalness: 0,
    roughness: 1.0,
  });
}

export function createCleatMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x888888,
    metalness: 0.8,
    roughness: 0.3,
  });
}

export function createRopeMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xBBAA99,
    metalness: 0,
    roughness: 0.95,
  });
}

export function createLifeRingMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF6600,
    metalness: 0.2,
    roughness: 0.8,
  });
}

export function createLightPoleMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x444444,
    metalness: 0.6,
    roughness: 0.4,
  });
}

export function createLightFixtureMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFAA,
    emissive: 0xFFFF88,
    emissiveIntensity: 0.6,
  });
}

export function createLadderMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x999999,
    metalness: 0.7,
    roughness: 0.4,
  });
}

export function createSignMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFDD00,
    metalness: 0.3,
    roughness: 0.7,
  });
}

export function createFenderMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x222222,
    metalness: 0.1,
    roughness: 0.9,
  });
}

/**
 * Building Materials
 */
export function createConcreteMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xAAAAAA,
    metalness: 0,
    roughness: 0.9,
  });
}

export function createWallMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xDDDDDD,
    metalness: 0.1,
    roughness: 0.8,
  });
}

export function createRoofMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x444444,
    metalness: 0.6,
    roughness: 0.4,
  });
}

export function createWindowFrameMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x333333,
    metalness: 0.5,
    roughness: 0.5,
  });
}

export function createGlassMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x88CCFF,
    metalness: 0.9,
    roughness: 0.1,
    transparent: true,
    opacity: 0.4,
  });
}

export function createDoorMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x8B4513,
    metalness: 0.2,
    roughness: 0.8,
  });
}

export function createDoorHandleMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xC0C0C0,
    metalness: 0.9,
    roughness: 0.2,
  });
}

export function createSignBoardMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.1,
    roughness: 0.6,
  });
}

/**
 * Buoy Materials (already in NavigationBuoys, but included for completeness)
 */
export function createBuoyBodyMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF6600,  // Navigation orange
    metalness: 0.4,
    roughness: 0.6,
  });
}

export function createBuoyStripeMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.3,
    roughness: 0.7,
  });
}

export function createBuoyLightMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    emissive: 0xFFFFFF,
    emissiveIntensity: 0.8,
  });
}

// Additional dock materials
export function createBeamMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x8B7355,
    metalness: 0.0,
    roughness: 0.8,
  });
}

export function createWhiteStripeMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.1,
    roughness: 0.6,
  });
}

export function createWarningSignMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFF0000,
    metalness: 0.1,
    roughness: 0.4,
  });
}

export function createDockSignMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0x1A3A5C,
    metalness: 0.1,
    roughness: 0.5,
  });
}

export function createDishMaterial() {
  return new THREE.MeshStandardMaterial({
    color: 0xFFFFFF,
    metalness: 0.8,
    roughness: 0.2,
  });
}

export default {
  // Boat materials
  createHullMaterial,
  createStripeMaterial,
  createDeckMaterial,
  createHelipadMaterial,
  createHelipadMarkingMaterial,
  createAccommodationMaterial,
  createBridgeMaterial,
  createWindowMaterial,
  createMastMaterial,
  createRadarMaterial,
  createAntennaMaterial,
  createCraneMaterial,
  createRailingMaterial,
  createPortLightMaterial,
  createStarboardLightMaterial,
  createMastheadLightMaterial,
  createStatusLightMaterial,
  
  // Dock materials
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
  createSignMaterial,
  createWarningSignMaterial,
  createFenderMaterial,
  
  // Building materials
  createConcreteMaterial,
  createWallMaterial,
  createRoofMaterial,
  createWindowFrameMaterial,
  createGlassMaterial,
  createDoorMaterial,
  createDoorHandleMaterial,
  createSignBoardMaterial,
  createDockSignMaterial,
  createAntennaMaterial,
  createDishMaterial,
  
  // Buoy materials
  createBuoyBodyMaterial,
  createBuoyStripeMaterial,
  createBuoyLightMaterial,
};
