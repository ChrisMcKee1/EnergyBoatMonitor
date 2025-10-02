/**
 * @file Superstructure molecule
 * Composes the main superstructure from smaller geometric parts.
 */
import * as THREE from 'three';
import { createAccommodationMaterial, createBridgeMaterial, createWindowMaterial } from '../BoatMaterials.js';

function createBlock(width, height, depth, material) {
    const geometry = new THREE.BoxGeometry(width, height, depth);
    return new THREE.Mesh(geometry, material);
}

export function createSuperstructure() {
    const superstructure = new THREE.Group();
    const accomMaterial = createAccommodationMaterial();
    const bridgeMaterial = createBridgeMaterial();
    const windowMaterial = createWindowMaterial();

    // Main accommodation block
    const mainBlock = createBlock(3.0, 1.5, 2.8, accomMaterial);
    mainBlock.position.set(-1.0, 1.95, 0); // Positioned on the main deck
    mainBlock.castShadow = true;
    superstructure.add(mainBlock);

    // Bridge level
    const bridge = createBlock(2.5, 1.2, 2.5, bridgeMaterial);
    bridge.position.set(-0.8, 3.3, 0);
    bridge.castShadow = true;
    superstructure.add(bridge);

    // Bridge Windows (as a single, slightly inset block)
    const bridgeWindows = createBlock(2.55, 0.7, 2.2, windowMaterial);
    bridgeWindows.position.set(-0.8, 3.4, 0);
    superstructure.add(bridgeWindows);

    return superstructure;
}
