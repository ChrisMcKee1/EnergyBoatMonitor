/**
 * DockBuilding Module
 * 
 * Creates the dock control building structure including foundation, walls,
 * roof, windows, door, signage, and rooftop equipment.
 * 
 * Phase 4 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';

/**
 * Creates building foundation/base
 * 
 * @param {THREE.Material} concreteMaterial - Concrete material
 * @returns {THREE.Mesh} Foundation mesh
 */
export function createBuildingFoundation(concreteMaterial) {
  const foundationGeometry = new THREE.BoxGeometry(2.8, 0.3, 2.8);
  const foundation = new THREE.Mesh(foundationGeometry, concreteMaterial);
  foundation.position.set(0, 0.8, 0);
  foundation.castShadow = true;
  foundation.receiveShadow = true;
  return foundation;
}

/**
 * Creates main building walls
 * 
 * @param {THREE.Material} wallMaterial - Light gray wall material
 * @returns {THREE.Mesh} Walls mesh
 */
export function createBuildingWalls(wallMaterial) {
  const wallsGeometry = new THREE.BoxGeometry(2.8, 2, 2.8);
  const walls = new THREE.Mesh(wallsGeometry, wallMaterial);
  walls.position.set(0, 1.95, 0);
  walls.castShadow = true;
  walls.receiveShadow = true;
  return walls;
}

/**
 * Creates building roof
 * 
 * @param {THREE.Material} roofMaterial - Brown roof material
 * @returns {THREE.Mesh} Roof mesh
 */
export function createBuildingRoof(roofMaterial) {
  const roofGeometry = new THREE.ConeGeometry(2.2, 1.2, 4);
  const roof = new THREE.Mesh(roofGeometry, roofMaterial);
  roof.rotation.y = Math.PI / 4;
  roof.position.set(0, 3.55, 0);
  roof.castShadow = true;
  return roof;
}

/**
 * Creates front windows with frames
 * 
 * @param {THREE.Material} frameMaterial - Window frame material
 * @param {THREE.Material} glassMaterial - Glass material
 * @returns {THREE.Group} Group containing front windows
 */
export function createFrontWindows(frameMaterial, glassMaterial) {
  const windowsGroup = new THREE.Group();
  
  for (let i = 0; i < 4; i++) {
    // Window frame
    const frameGeometry = new THREE.BoxGeometry(0.5, 0.6, 0.05);
    const frame = new THREE.Mesh(frameGeometry, frameMaterial);
    frame.position.set(-1 + i * 0.7, 2.3, 1.43);
    windowsGroup.add(frame);
    
    // Window glass
    const glassGeometry = new THREE.BoxGeometry(0.45, 0.55, 0.02);
    const glass = new THREE.Mesh(glassGeometry, glassMaterial);
    glass.position.set(-1 + i * 0.7, 2.3, 1.44);
    windowsGroup.add(glass);
  }
  
  return windowsGroup;
}

/**
 * Creates side windows with frames
 * 
 * @param {THREE.Material} frameMaterial - Window frame material
 * @param {THREE.Material} glassMaterial - Glass material
 * @returns {THREE.Group} Group containing side windows
 */
export function createSideWindows(frameMaterial, glassMaterial) {
  const windowsGroup = new THREE.Group();
  
  for (let i = 0; i < 2; i++) {
    // Left side window frame
    const leftFrame = new THREE.Mesh(
      new THREE.BoxGeometry(0.05, 0.6, 0.5),
      frameMaterial
    );
    leftFrame.position.set(-1.43, 2.3, -0.5 + i * 1);
    windowsGroup.add(leftFrame);
    
    // Left side window glass
    const leftGlass = new THREE.Mesh(
      new THREE.BoxGeometry(0.02, 0.55, 0.45),
      glassMaterial
    );
    leftGlass.position.set(-1.44, 2.3, -0.5 + i * 1);
    windowsGroup.add(leftGlass);
    
    // Right side window frame
    const rightFrame = leftFrame.clone();
    rightFrame.position.x = 1.43;
    windowsGroup.add(rightFrame);
    
    // Right side window glass
    const rightGlass = leftGlass.clone();
    rightGlass.position.x = 1.44;
    windowsGroup.add(rightGlass);
  }
  
  return windowsGroup;
}

/**
 * Creates building door with handle
 * 
 * @param {THREE.Material} doorMaterial - Brown door material
 * @param {THREE.Material} handleMaterial - Gold handle material
 * @returns {THREE.Group} Group containing door and handle
 */
export function createBuildingDoor(doorMaterial, handleMaterial) {
  const doorGroup = new THREE.Group();
  
  // Door
  const doorGeometry = new THREE.BoxGeometry(0.8, 1.4, 0.1);
  const door = new THREE.Mesh(doorGeometry, doorMaterial);
  door.position.set(0, 1.65, 1.45);
  door.castShadow = true;
  doorGroup.add(door);
  
  // Door handle
  const handleGeometry = new THREE.SphereGeometry(0.08, 16, 16);
  const handle = new THREE.Mesh(handleGeometry, handleMaterial);
  handle.position.set(0.3, 1.65, 1.5);
  doorGroup.add(handle);
  
  return doorGroup;
}

/**
 * Creates "DOCK CONTROL" sign above door
 * 
 * @param {THREE.Material} signMaterial - Navy blue sign material
 * @returns {THREE.Mesh} Sign mesh
 */
export function createDockControlSign(signMaterial) {
  const signGeometry = new THREE.BoxGeometry(1.2, 0.3, 0.05);
  const sign = new THREE.Mesh(signGeometry, signMaterial);
  sign.position.set(0, 2.5, 1.43);
  return sign;
}

/**
 * Creates rooftop antenna
 * 
 * @param {THREE.Material} antennaMaterial - Metal antenna material
 * @returns {THREE.Group} Group containing antenna pole and elements
 */
export function createRoofAntenna(antennaMaterial) {
  const antennaGroup = new THREE.Group();
  
  // Main antenna pole
  const poleGeometry = new THREE.CylinderGeometry(0.03, 0.03, 1.5, 12);
  const pole = new THREE.Mesh(poleGeometry, antennaMaterial);
  pole.position.set(-0.6, 4.9, 0);
  antennaGroup.add(pole);
  
  // Antenna elements (crossbars)
  for (let i = 0; i < 3; i++) {
    const elementGeometry = new THREE.CylinderGeometry(0.02, 0.02, 0.6, 8);
    const element = new THREE.Mesh(elementGeometry, antennaMaterial);
    element.rotation.z = Math.PI / 2;
    element.position.set(-0.6, 4.3 + i * 0.3, 0);
    antennaGroup.add(element);
  }
  
  return antennaGroup;
}

/**
 * Creates satellite dish on roof
 * 
 * @param {THREE.Material} dishMaterial - White dish material
 * @returns {THREE.Group} Group containing dish and mount
 */
export function createSatelliteDish(dishMaterial) {
  const dishGroup = new THREE.Group();
  
  // Dish
  const dishGeometry = new THREE.SphereGeometry(0.4, 16, 16, 0, Math.PI * 2, 0, Math.PI / 2);
  const dish = new THREE.Mesh(dishGeometry, dishMaterial);
  dish.rotation.x = -Math.PI / 3;
  dish.position.set(0.6, 4.3, 0);
  dishGroup.add(dish);
  
  // Dish mount
  const mountGeometry = new THREE.CylinderGeometry(0.05, 0.05, 0.3, 12);
  const mount = new THREE.Mesh(mountGeometry, dishMaterial);
  mount.position.set(0.6, 4.15, 0);
  dishGroup.add(mount);
  
  return dishGroup;
}

/**
 * Creates complete dock building assembly
 * 
 * @param {Object} materials - Object containing all required materials
 * @returns {THREE.Group} Complete building assembly
 */
export function createCompleteDockBuilding(materials) {
  const buildingGroup = new THREE.Group();
  
  // Add foundation
  const foundation = createBuildingFoundation(materials.concrete);
  buildingGroup.add(foundation);
  
  // Add walls
  const walls = createBuildingWalls(materials.wall);
  buildingGroup.add(walls);
  
  // Add roof
  const roof = createBuildingRoof(materials.roof);
  buildingGroup.add(roof);
  
  // Add windows
  const frontWindows = createFrontWindows(materials.windowFrame, materials.glass);
  buildingGroup.add(frontWindows);
  
  const sideWindows = createSideWindows(materials.windowFrame, materials.glass);
  buildingGroup.add(sideWindows);
  
  // Add door
  const door = createBuildingDoor(materials.door, materials.doorHandle);
  buildingGroup.add(door);
  
  // Add sign
  const sign = createDockControlSign(materials.sign);
  buildingGroup.add(sign);
  
  // Add rooftop equipment
  const antenna = createRoofAntenna(materials.antenna);
  buildingGroup.add(antenna);
  
  const dish = createSatelliteDish(materials.dish);
  buildingGroup.add(dish);
  
  return buildingGroup;
}

export default {
  createBuildingFoundation,
  createBuildingWalls,
  createBuildingRoof,
  createFrontWindows,
  createSideWindows,
  createBuildingDoor,
  createDockControlSign,
  createRoofAntenna,
  createSatelliteDish,
  createCompleteDockBuilding,
};
