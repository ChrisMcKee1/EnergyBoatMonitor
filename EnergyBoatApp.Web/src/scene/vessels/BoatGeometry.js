/**
 * BoatGeometry Module
 * 
 * Provides geometry creation functions for boat structural components.
 * Includes hull, deck, superstructure, and basic ship architecture.
 * 
 * Phase 3 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates the main hull geometry using ExtrudeGeometry
 * Hull is wider at waterline, narrower at top for realistic naval architecture
 * 
 * @returns {THREE.ExtrudeGeometry} Hull geometry
 */
export function createHullGeometry() {
  const hullShape = new THREE.Shape();
  hullShape.moveTo(-2.5, 0);
  hullShape.lineTo(-2.5, 0.8);
  hullShape.lineTo(-2.2, 1.2);
  hullShape.lineTo(2.2, 1.2);
  hullShape.lineTo(2.5, 0.8);
  hullShape.lineTo(2.5, 0);
  hullShape.lineTo(-2.5, 0);
  
  const extrudeSettings = {
    steps: 2,
    depth: 2.5,
    bevelEnabled: true,
    bevelThickness: 0.2,
    bevelSize: 0.2,
    bevelSegments: 5
  };
  
  return new THREE.ExtrudeGeometry(hullShape, extrudeSettings);
}

/**
 * Creates the hull stripe geometry (Fugro branding)
 * 
 * @returns {THREE.BoxGeometry} Stripe geometry
 */
export function createStripeGeometry() {
  return new THREE.BoxGeometry(5, 0.3, 2.6);
}

/**
 * Creates the main deck geometry
 * 
 * @returns {THREE.BoxGeometry} Main deck geometry
 */
export function createMainDeckGeometry() {
  return new THREE.BoxGeometry(4.5, 0.15, 2.3);
}

/**
 * Creates the helipad geometry (circular deck at stern)
 * 
 * @returns {THREE.CylinderGeometry} Helipad geometry
 */
export function createHelipadGeometry() {
  return new THREE.CylinderGeometry(0.9, 0.9, 0.1, 32);
}

/**
 * Creates the helipad "H" marking geometry
 * 
 * @returns {THREE.TorusGeometry} H marking geometry
 */
export function createHelipadMarkingGeometry() {
  return new THREE.TorusGeometry(0.3, 0.05, 8, 32);
}

/**
 * Creates the lower accommodation block geometry
 * 
 * @returns {THREE.BoxGeometry} Lower accommodation geometry
 */
export function createLowerAccommodationGeometry() {
  return new THREE.BoxGeometry(2.5, 1.2, 2);
}

/**
 * Creates the bridge (wheelhouse) geometry
 * 
 * @returns {THREE.BoxGeometry} Bridge geometry
 */
export function createBridgeGeometry() {
  return new THREE.BoxGeometry(2, 1.5, 1.8);
}

/**
 * Creates the bridge windows geometry
 * 
 * @returns {THREE.BoxGeometry} Window geometry
 */
export function createBridgeWindowsGeometry() {
  return new THREE.BoxGeometry(1.9, 0.8, 1.81);
}

/**
 * Creates a complete hull mesh with proper positioning
 * 
 * @param {THREE.Material} material - Hull material
 * @returns {THREE.Mesh} Positioned hull mesh
 */
export function createHullMesh(material) {
  const geometry = createHullGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.rotation.x = Math.PI / 2;
  mesh.position.set(0, 0.6, 0);  // Center the hull properly
  mesh.castShadow = true;
  mesh.receiveShadow = true;
  return mesh;
}

/**
 * Creates a complete stripe mesh with proper positioning
 * 
 * @param {THREE.Material} material - Stripe material
 * @returns {THREE.Mesh} Positioned stripe mesh
 */
export function createStripeMesh(material) {
  const geometry = createStripeGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0, 0.4, 0);
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete main deck mesh with proper positioning
 * 
 * @param {THREE.Material} material - Deck material
 * @returns {THREE.Mesh} Positioned deck mesh
 */
export function createMainDeckMesh(material) {
  const geometry = createMainDeckGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.y = 1.35;
  mesh.castShadow = true;
  mesh.receiveShadow = true;
  return mesh;
}

/**
 * Creates a complete helipad mesh with proper positioning
 * 
 * @param {THREE.Material} material - Helipad material
 * @returns {THREE.Mesh} Positioned helipad mesh
 */
export function createHelipadMesh(material) {
  const geometry = createHelipadGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(1.8, 1.5, 0);
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete helipad marking mesh with proper positioning
 * 
 * @param {THREE.Material} material - Marking material
 * @returns {THREE.Mesh} Positioned marking mesh
 */
export function createHelipadMarkingMesh(material) {
  const geometry = createHelipadMarkingGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.rotation.x = Math.PI / 2;
  mesh.position.set(1.8, 1.55, 0);
  return mesh;
}

/**
 * Creates a complete lower accommodation mesh with proper positioning
 * 
 * @param {THREE.Material} material - Accommodation material
 * @returns {THREE.Mesh} Positioned accommodation mesh
 */
export function createLowerAccommodationMesh(material) {
  const geometry = createLowerAccommodationGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(-0.8, 2, 0);
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete bridge mesh with proper positioning
 * 
 * @param {THREE.Material} material - Bridge material
 * @returns {THREE.Mesh} Positioned bridge mesh
 */
export function createBridgeMesh(material) {
  const geometry = createBridgeGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(-0.8, 3.2, 0);
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete bridge windows mesh with proper positioning
 * 
 * @param {THREE.Material} material - Window material
 * @returns {THREE.Mesh} Positioned windows mesh
 */
export function createBridgeWindowsMesh(material) {
  const geometry = createBridgeWindowsGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(-0.8, 3.4, 0);
  return mesh;
}

export default {
  // Geometry creators
  createHullGeometry,
  createStripeGeometry,
  createMainDeckGeometry,
  createHelipadGeometry,
  createHelipadMarkingGeometry,
  createLowerAccommodationGeometry,
  createBridgeGeometry,
  createBridgeWindowsGeometry,
  
  // Mesh creators (geometry + positioning)
  createHullMesh,
  createStripeMesh,
  createMainDeckMesh,
  createHelipadMesh,
  createHelipadMarkingMesh,
  createLowerAccommodationMesh,
  createBridgeMesh,
  createBridgeWindowsMesh,
};
