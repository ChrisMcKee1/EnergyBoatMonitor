/**
 * BoatModel Component
 * 
 * Complete boat assembly combining geometry, materials, and equipment.
 * Creates realistic offshore survey vessels for the Energy Boat application.
 * 
 * Phase 3 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import * as BoatGeometry from './BoatGeometry.js';
import * as BoatEquipment from './BoatEquipment.js';
import * as BoatMaterials from './BoatMaterials.js';
import { getStatusLightColor } from '../utils/BoatHelpers.js';

/**
 * Creates a complete, highly detailed offshore survey vessel
 * 
 * @param {Object} boatData - Boat configuration data
 * @param {string} boatData.status - Boat status ('Active', 'Charging', 'Maintenance')
 * @param {string} boatData.id - Boat identifier
 * @returns {THREE.Group} Complete boat assembly
 */
export function createBoat(boatData) {
  const boatGroup = new THREE.Group();
  const status = boatData.status || boatData.Status || 'Active';
  
  // Store boat data for reference
  boatGroup.userData.boatData = boatData;
  boatGroup.userData.status = status;
  
  // === HULL ===
  const hullMaterial = BoatMaterials.createHullMaterial(status);
  const hull = BoatGeometry.createHullMesh(hullMaterial);
  boatGroup.add(hull);
  
  // Hull stripe (Contoso-Sea branding)
  const stripeMaterial = BoatMaterials.createStripeMaterial();
  const stripe = BoatGeometry.createStripeMesh(stripeMaterial);
  boatGroup.add(stripe);
  
  // === DECK ===
  const deckMaterial = BoatMaterials.createDeckMaterial();
  const mainDeck = BoatGeometry.createMainDeckMesh(deckMaterial);
  boatGroup.add(mainDeck);
  
  // Helipad
  const helipadMaterial = BoatMaterials.createHelipadMaterial();
  const helipad = BoatGeometry.createHelipadMesh(helipadMaterial);
  boatGroup.add(helipad);
  
  // Helipad marking
  const helipadMarkingMaterial = BoatMaterials.createHelipadMarkingMaterial();
  const helipadMarking = BoatGeometry.createHelipadMarkingMesh(helipadMarkingMaterial);
  boatGroup.add(helipadMarking);
  
  // === SUPERSTRUCTURE ===
  const accomMaterial = BoatMaterials.createAccommodationMaterial();
  const lowerAccom = BoatGeometry.createLowerAccommodationMesh(accomMaterial);
  boatGroup.add(lowerAccom);
  
  const midAccom = BoatGeometry.createMidAccommodationMesh(accomMaterial);
  boatGroup.add(midAccom);
  
  const bridgeMaterial = BoatMaterials.createBridgeMaterial();
  const bridge = BoatGeometry.createBridgeMesh(bridgeMaterial);
  boatGroup.add(bridge);
  
  const windowMaterial = BoatMaterials.createWindowMaterial();
  const windows = BoatGeometry.createBridgeWindowsMesh(windowMaterial);
  boatGroup.add(windows);
  
  // === FUNNEL ===
  const funnelMaterial = BoatMaterials.createFunnelMaterial();
  const funnel = BoatGeometry.createFunnelMesh(funnelMaterial);
  boatGroup.add(funnel);
  
  const funnelCapMaterial = BoatMaterials.createFunnelCapMaterial();
  const funnelCap = BoatGeometry.createFunnelCapMesh(funnelCapMaterial);
  boatGroup.add(funnelCap);
  
  // === EQUIPMENT ===
  const mastMaterial = BoatMaterials.createMastMaterial();
  const mast = BoatEquipment.createMast(mastMaterial);
  boatGroup.add(mast);
  
  const radarMaterial = BoatMaterials.createRadarMaterial();
  const radar = BoatEquipment.createRadar(radarMaterial);
  boatGroup.add(radar);
  
  const antennaMaterial = BoatMaterials.createAntennaMaterial();
  const antennas = BoatEquipment.createAntennas(antennaMaterial);
  boatGroup.add(antennas);
  
  const craneMaterial = BoatMaterials.createCraneMaterial();
  const crane = BoatEquipment.createCrane(craneMaterial);
  boatGroup.add(crane);
  
  const railingMaterial = BoatMaterials.createRailingMaterial();
  const railings = BoatEquipment.createDeckRailings(railingMaterial);
  boatGroup.add(railings);
  
  // === NAVIGATION LIGHTS ===
  const portLightMaterial = BoatMaterials.createPortLightMaterial();
  const starboardLightMaterial = BoatMaterials.createStarboardLightMaterial();
  const mastheadLightMaterial = BoatMaterials.createMastheadLightMaterial();
  
  const navLights = BoatEquipment.createNavigationLights(
    portLightMaterial,
    starboardLightMaterial,
    mastheadLightMaterial
  );
  boatGroup.add(navLights);
  
  const navPointLights = BoatEquipment.createNavigationPointLights();
  boatGroup.add(navPointLights);
  
  // === STATUS INDICATOR ===
  const statusColor = getStatusLightColor(status);
  const statusLightMaterial = BoatMaterials.createStatusLightMaterial(status);
  const statusLighting = BoatEquipment.createStatusLight(statusLightMaterial, statusColor);
  boatGroup.add(statusLighting.light);
  boatGroup.add(statusLighting.pointLight);
  
  // Scale boat to appropriate size (boats are designed at unit scale 1)
  boatGroup.scale.set(1.5, 1.5, 1.5);
  
  return boatGroup;
}

/**
 * Updates boat status and visual indicators
 * 
 * @param {THREE.Group} boatGroup - Boat group to update
 * @param {string} newStatus - New status ('Active', 'Charging', 'Maintenance')
 */
export function updateBoatStatus(boatGroup, newStatus) {
  if (!boatGroup || !boatGroup.userData) return;
  
  boatGroup.userData.status = newStatus;
  
  // Update hull color
  boatGroup.traverse((child) => {
    if (child.isMesh && child.geometry.type === 'ExtrudeGeometry') {
      // This is the hull
      const newHullMaterial = BoatMaterials.createHullMaterial(newStatus);
      child.material.dispose();
      child.material = newHullMaterial;
    }
  });
  
  // Update status light
  const statusColor = getStatusLightColor(newStatus);
  boatGroup.traverse((child) => {
    if (child.isLight && child.type === 'PointLight' && child.position.y === 6.5) {
      child.color.setHex(statusColor);
    }
    if (child.isMesh && child.geometry.type === 'SphereGeometry' && child.position.y === 6.5) {
      const newStatusMaterial = BoatMaterials.createStatusLightMaterial(newStatus);
      child.material.dispose();
      child.material = newStatusMaterial;
    }
  });
}

/**
 * Gets boat bounding box for collision detection or camera positioning
 * 
 * @param {THREE.Group} boatGroup - Boat group
 * @returns {THREE.Box3} Bounding box
 */
export function getBoatBoundingBox(boatGroup) {
  return new THREE.Box3().setFromObject(boatGroup);
}

/**
 * Highlights boat (used for selection)
 * 
 * @param {THREE.Group} boatGroup - Boat group
 * @param {boolean} highlighted - Whether to highlight or unhighlight
 */
export function highlightBoat(boatGroup, highlighted) {
  const scale = highlighted ? 1.3 : 1.0;
  const emissiveIntensity = highlighted ? 1.5 : 0.3;
  
  boatGroup.scale.set(scale, scale, scale);
  
  boatGroup.traverse((child) => {
    if (child.isMesh && child.material.emissive) {
      if (!child.userData.originalEmissive) {
        child.userData.originalEmissive = child.material.emissiveIntensity;
      }
      child.material.emissiveIntensity = highlighted ? 
        emissiveIntensity : 
        (child.userData.originalEmissive || 0.3);
    }
  });
}

/**
 * Disposes of boat resources (geometry and materials)
 * 
 * @param {THREE.Group} boatGroup - Boat group to dispose
 */
export function disposeBoat(boatGroup) {
  boatGroup.traverse((child) => {
    if (child.isMesh) {
      if (child.geometry) child.geometry.dispose();
      if (child.material) {
        if (Array.isArray(child.material)) {
          child.material.forEach(mat => mat.dispose());
        } else {
          child.material.dispose();
        }
      }
    }
  });
}

export default {
  createBoat,
  updateBoatStatus,
  getBoatBoundingBox,
  highlightBoat,
  disposeBoat,
};
