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
 * With bow taper for realistic ship profile
 * 
 * @returns {THREE.ExtrudeGeometry} Hull geometry
 */
export function createHullGeometry() {
  const hullShape = new THREE.Shape();
  // Narrower hull profile (width E-W)
  hullShape.moveTo(-1.8, 0);     // Bottom width (narrower)
  hullShape.lineTo(-1.8, 0.5);   // Lower hull
  hullShape.lineTo(-1.5, 1.5);   // Upper hull curve - extended height
  hullShape.lineTo(1.5, 1.5);    // Deck level - higher to meet deck
  hullShape.lineTo(1.8, 0.5);    // Symmetrical curve
  hullShape.lineTo(1.8, 0);      // Bottom
  hullShape.lineTo(-1.8, 0);
  
  const extrudeSettings = {
    steps: 4,
    depth: 8.0,           // LONGER hull (N-S direction)
    bevelEnabled: true,
    bevelThickness: 0.5,  // Bow taper
    bevelSize: 0.6,       // More pronounced bow
    bevelSegments: 8      // Smoother bow curve
  };
  
  return new THREE.ExtrudeGeometry(hullShape, extrudeSettings);
}

/**
 * Creates the hull stripe geometry (Contoso-Sea branding)
 * 
 * @returns {THREE.BoxGeometry} Stripe geometry
 */
export function createStripeGeometry() {
  return new THREE.BoxGeometry(3.2, 0.4, 8.2);
}

/**
 * Creates the main deck geometry
 * 
 * @returns {THREE.BoxGeometry} Main deck geometry
 */
export function createMainDeckGeometry() {
  return new THREE.BoxGeometry(7.5, 0.3, 3.0);
}

/**
 * Creates the helipad geometry (circular deck at stern)
 * 
 * @returns {THREE.CylinderGeometry} Helipad geometry
 */
export function createHelipadGeometry() {
  return new THREE.CylinderGeometry(1.2, 1.2, 0.15, 32);
}

/**
 * Creates the helipad "H" marking geometry
 * 
 * @returns {THREE.TorusGeometry} H marking geometry
 */
export function createHelipadMarkingGeometry() {
  return new THREE.TorusGeometry(0.5, 0.05, 8, 32);
}

/**
 * Creates the lower accommodation block geometry
 * 
 * @returns {THREE.BoxGeometry} Lower accommodation geometry
 */
export function createLowerAccommodationGeometry() {
  return new THREE.BoxGeometry(4.0, 1.8, 2.4);
}

/**
 * Creates the mid accommodation block geometry (sits on lower accommodation)
 * 
 * @returns {THREE.BoxGeometry} Mid accommodation geometry
 */
export function createMidAccommodationGeometry() {
  return new THREE.BoxGeometry(3.5, 1.2, 2.0);
}

/**
 * Creates the bridge (wheelhouse) geometry
 * 
 * @returns {THREE.BoxGeometry} Bridge geometry
 */
export function createBridgeGeometry() {
  return new THREE.BoxGeometry(3.0, 1.3, 1.8);
}

/**
 * Creates the bridge windows geometry
 * 
 * @returns {THREE.BoxGeometry} Window geometry
 */
export function createBridgeWindowsGeometry() {
  return new THREE.BoxGeometry(2.95, 0.9, 1.75);
}

/**
 * Creates funnel/exhaust stack geometry
 * 
 * @returns {THREE.CylinderGeometry} Funnel geometry
 */
export function createFunnelGeometry() {
  return new THREE.CylinderGeometry(0.35, 0.42, 1.2, 16);
}

/**
 * Creates funnel cap geometry
 * 
 * @returns {THREE.CylinderGeometry} Funnel cap geometry
 */
export function createFunnelCapGeometry() {
  return new THREE.CylinderGeometry(0.42, 0.38, 0.25, 16);
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
  mesh.rotation.z = -Math.PI / 2;
  mesh.position.set(0, 0.75, 0);
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
  mesh.position.set(0, 0.6, 0);
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
  mesh.position.y = 2.1;
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
  mesh.position.set(-2.5, 2.28, 0); // Moved forward to prevent overhang
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
  mesh.position.set(-2.5, 2.36, 0); // Moved forward to match helipad
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
  mesh.position.set(0.8, 3.15, 0);
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete mid accommodation mesh with proper positioning
 * 
 * @param {THREE.Material} material - Accommodation material
 * @returns {THREE.Mesh} Positioned accommodation mesh
 */
export function createMidAccommodationMesh(material) {
  const geometry = createMidAccommodationGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0.8, 4.65, 0);
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
  mesh.position.set(0.8, 5.85, 0);
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
  mesh.position.set(0, 6.0, 0.8);  // Sits in bridge front
  return mesh;
}

/**
 * Creates a complete funnel mesh with proper positioning
 * 
 * @param {THREE.Material} material - Funnel material
 * @returns {THREE.Mesh} Positioned funnel mesh
 */
export function createFunnelMesh(material) {
  const geometry = createFunnelGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0, 5.95, -0.5);  // Behind bridge, centered
  mesh.castShadow = true;
  return mesh;
}

/**
 * Creates a complete funnel cap mesh with proper positioning
 * 
 * @param {THREE.Material} material - Funnel cap material
 * @returns {THREE.Mesh} Positioned funnel cap mesh
 */
export function createFunnelCapMesh(material) {
  const geometry = createFunnelCapGeometry();
  const mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(0, 6.7, -0.5);  // Top of funnel
  mesh.castShadow = true;
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
  createMidAccommodationGeometry,
  createBridgeGeometry,
  createBridgeWindowsGeometry,
  createFunnelGeometry,
  createFunnelCapGeometry,
  
  // Mesh creators (geometry + positioning)
  createHullMesh,
  createStripeMesh,
  createMainDeckMesh,
  createHelipadMesh,
  createHelipadMarkingMesh,
  createLowerAccommodationMesh,
  createMidAccommodationMesh,
  createBridgeMesh,
  createBridgeWindowsMesh,
  createFunnelMesh,
  createFunnelCapMesh,
};
