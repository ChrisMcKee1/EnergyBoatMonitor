/**
 * DockEquipment Module
 * 
 * Creates dock accessories, safety equipment, and navigation aids.
 * Includes mooring cleats, ropes, life rings, lights, ladders, signage, and fenders.
 * 
 * Phase 4 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates mooring cleats with posts
 * 
 * @param {THREE.Material} cleatMaterial - Metal cleat material
 * @returns {THREE.Group} Group containing all cleats
 */
export function createMooringCleats(cleatMaterial) {
  const cleatsGroup = new THREE.Group();
  
  for (let i = 0; i < 6; i++) {
    // Cleat ring
    const cleatGeometry = new THREE.TorusGeometry(0.15, 0.05, 8, 16);
    const cleat = new THREE.Mesh(cleatGeometry, cleatMaterial);
    cleat.rotation.x = Math.PI / 2;
    cleat.position.set(-4 + i * 2, 0.7, i % 2 === 0 ? 3.8 : -3.8);
    cleat.castShadow = true;
    cleatsGroup.add(cleat);
    
    // Cleat post
    const postGeometry = new THREE.CylinderGeometry(0.08, 0.08, 0.4, 12);
    const post = new THREE.Mesh(postGeometry, cleatMaterial);
    post.position.set(-4 + i * 2, 0.5, i % 2 === 0 ? 3.8 : -3.8);
    cleatsGroup.add(post);
  }
  
  return cleatsGroup;
}

/**
 * Creates rope coils on dock
 * 
 * @param {THREE.Material} ropeMaterial - Manila rope material
 * @returns {THREE.Group} Group containing rope coils
 */
export function createRopeCoils(ropeMaterial) {
  const ropesGroup = new THREE.Group();
  
  for (let i = 0; i < 3; i++) {
    const ropeGeometry = new THREE.TorusGeometry(0.2, 0.08, 12, 24);
    const rope = new THREE.Mesh(ropeGeometry, ropeMaterial);
    rope.rotation.x = Math.PI / 2;
    rope.position.set(-3 + i * 3, 0.65, 3);
    ropesGroup.add(rope);
  }
  
  return ropesGroup;
}

/**
 * Creates life preserver rings with safety stripes
 * 
 * @param {THREE.Material} ringMaterial - Orange ring material
 * @param {THREE.Material} stripeMaterial - White stripe material
 * @returns {THREE.Group} Group containing life rings
 */
export function createLifeRings(ringMaterial, stripeMaterial) {
  const ringsGroup = new THREE.Group();
  
  for (let i = 0; i < 2; i++) {
    // Main ring
    const ringGeometry = new THREE.TorusGeometry(0.3, 0.1, 16, 32);
    const ring = new THREE.Mesh(ringGeometry, ringMaterial);
    ring.position.set(-3 + i * 6, 1.2, -3.5);
    ringsGroup.add(ring);
    
    // White safety stripe
    const stripeGeometry = new THREE.TorusGeometry(0.3, 0.05, 16, 32);
    const stripe = new THREE.Mesh(stripeGeometry, stripeMaterial);
    stripe.position.copy(ring.position);
    stripe.position.z -= 0.05;
    ringsGroup.add(stripe);
  }
  
  return ringsGroup;
}

/**
 * Creates dock navigation lights with poles
 * 
 * @param {THREE.Material} poleMaterial - Light pole material
 * @param {THREE.Material} fixtureMaterial - Light fixture material
 * @returns {Object} Object containing lights group and point lights group
 */
export function createDockLights(poleMaterial, fixtureMaterial) {
  const lightsGroup = new THREE.Group();
  const pointLightsGroup = new THREE.Group();
  
  for (let i = 0; i < 3; i++) {
    // Light pole
    const poleGeometry = new THREE.CylinderGeometry(0.08, 0.1, 2.5, 12);
    const pole = new THREE.Mesh(poleGeometry, poleMaterial);
    pole.position.set(-4 + i * 4, 1.8, -3.8);
    pole.castShadow = true;
    lightsGroup.add(pole);
    
    // Light fixture
    const fixtureGeometry = new THREE.SphereGeometry(0.2, 16, 16);
    const fixture = new THREE.Mesh(fixtureGeometry, fixtureMaterial);
    fixture.position.set(-4 + i * 4, 3.1, -3.8);
    lightsGroup.add(fixture);
    
    // Point light for atmospheric effect
    const dockLight = new THREE.PointLight(0xFFFFAA, 0.8, 15);
    dockLight.position.copy(fixture.position);
    pointLightsGroup.add(dockLight);
  }
  
  return { lights: lightsGroup, pointLights: pointLightsGroup };
}

/**
 * Creates ladder for boat access
 * 
 * @param {THREE.Material} ladderMaterial - Metal ladder material
 * @returns {THREE.Group} Group containing ladder
 */
export function createAccessLadder(ladderMaterial) {
  const ladderGroup = new THREE.Group();
  
  // Ladder sides
  const ladderSideGeometry = new THREE.BoxGeometry(0.1, 3, 0.1);
  const ladderLeft = new THREE.Mesh(ladderSideGeometry, ladderMaterial);
  ladderLeft.position.set(4, -0.4, 3.5);
  ladderGroup.add(ladderLeft);
  
  const ladderRight = ladderLeft.clone();
  ladderRight.position.z = 4.2;
  ladderGroup.add(ladderRight);
  
  // Ladder rungs
  for (let i = 0; i < 8; i++) {
    const rungGeometry = new THREE.CylinderGeometry(0.05, 0.05, 0.7, 12);
    const rung = new THREE.Mesh(rungGeometry, ladderMaterial);
    rung.rotation.z = Math.PI / 2;
    rung.position.set(4, -1.5 + i * 0.4, 3.85);
    ladderGroup.add(rung);
  }
  
  return ladderGroup;
}

/**
 * Creates warning signage
 * 
 * @param {THREE.Material} signMaterial - Sign material (red)
 * @param {THREE.Material} postMaterial - Sign post material
 * @returns {THREE.Group} Group containing sign and post
 */
export function createWarningSign(signMaterial, postMaterial) {
  const signGroup = new THREE.Group();
  
  // Sign board
  const signGeometry = new THREE.BoxGeometry(0.8, 0.5, 0.05);
  const sign = new THREE.Mesh(signGeometry, signMaterial);
  sign.position.set(-4, 1.5, 3.9);
  sign.castShadow = true;
  signGroup.add(sign);
  
  // Sign post
  const signPostGeometry = new THREE.CylinderGeometry(0.05, 0.05, 1, 12);
  const signPost = new THREE.Mesh(signPostGeometry, postMaterial);
  signPost.position.set(-4, 1, 3.9);
  signGroup.add(signPost);
  
  return signGroup;
}

/**
 * Creates floating fenders (bumpers)
 * 
 * @param {THREE.Material} fenderMaterial - Black rubber material
 * @returns {THREE.Group} Group containing all fenders
 */
export function createDockFenders(fenderMaterial) {
  const fendersGroup = new THREE.Group();
  
  for (let i = 0; i < 8; i++) {
    const fenderGeometry = new THREE.CylinderGeometry(0.2, 0.2, 1.2, 16);
    const fender = new THREE.Mesh(fenderGeometry, fenderMaterial);
    fender.rotation.x = Math.PI / 2;
    fender.position.set(-4 + i * 1.5, 0.3, 4);
    fendersGroup.add(fender);
  }
  
  return fendersGroup;
}

/**
 * Creates complete dock equipment assembly
 * 
 * @param {Object} materials - Object containing all required materials
 * @returns {THREE.Group} Complete equipment assembly
 */
export function createCompleteDockEquipment(materials) {
  const equipmentGroup = new THREE.Group();
  
  // Add cleats
  const cleats = createMooringCleats(materials.cleat);
  equipmentGroup.add(cleats);
  
  // Add ropes
  const ropes = createRopeCoils(materials.rope);
  equipmentGroup.add(ropes);
  
  // Add life rings
  const lifeRings = createLifeRings(materials.lifeRing, materials.whiteStripe);
  equipmentGroup.add(lifeRings);
  
  // Add dock lights
  const dockLights = createDockLights(materials.lightPole, materials.lightFixture);
  equipmentGroup.add(dockLights.lights);
  equipmentGroup.add(dockLights.pointLights);
  
  // Add ladder
  const ladder = createAccessLadder(materials.ladder);
  equipmentGroup.add(ladder);
  
  // Add warning sign
  const sign = createWarningSign(materials.sign, materials.lightPole);
  equipmentGroup.add(sign);
  
  // Add fenders
  const fenders = createDockFenders(materials.fender);
  equipmentGroup.add(fenders);
  
  return equipmentGroup;
}

export default {
  createMooringCleats,
  createRopeCoils,
  createLifeRings,
  createDockLights,
  createAccessLadder,
  createWarningSign,
  createDockFenders,
  createCompleteDockEquipment,
};
