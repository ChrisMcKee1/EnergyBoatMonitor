/**
 * @file Mast organism
 * Assembles the mast from a pole, radar, and antennas.
 */
import * as THREE from 'three';
import { createMastMaterial, createRadarMaterial } from '../BoatMaterials.js';

// The mast will be assembled at the origin and then placed.
export function createMastAssembly() {
    const mastAssembly = new THREE.Group();
    const mastMaterial = createMastMaterial();
    const radarMaterial = createRadarMaterial();

    // Main vertical pole
    const poleGeom = new THREE.CylinderGeometry(0.08, 0.12, 3.5, 10);
    const pole = new THREE.Mesh(poleGeom, mastMaterial);
    pole.castShadow = true;
    mastAssembly.add(pole);

    // Cross-arm for antennas/lights
    const armGeom = new THREE.BoxGeometry(1.5, 0.1, 0.1);
    const crossArm = new THREE.Mesh(armGeom, mastMaterial);
    crossArm.position.y = 1.0; // Position on the pole
    crossArm.castShadow = true;
    mastAssembly.add(crossArm);

    // Radar dome on top
    const radarGeom = new THREE.SphereGeometry(0.35, 16, 12);
    const radar = new THREE.Mesh(radarGeom, radarMaterial);
    radar.position.y = 1.9; // Position on top of the pole
    radar.castShadow = true;
    mastAssembly.add(radar);

    // Position the entire assembly on top of the bridge
    mastAssembly.position.set(-0.8, 5.4, 0);

    return mastAssembly;
}
