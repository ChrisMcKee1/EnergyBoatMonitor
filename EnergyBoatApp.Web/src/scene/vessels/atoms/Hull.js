/**
 * @file Hull atom
 * Defines the basic hull geometry for the vessel.
 */
import * as THREE from 'three';

import { createHullMaterial } from '../BoatMaterials.js';

export function createHull() {
    const hullShape = new THREE.Shape();
    
    // A more realistic hull shape for the cross-section
    hullShape.moveTo(0, -1.5); // Keel
    hullShape.splineThru([
        new THREE.Vector2(1.5, -1.0),
        new THREE.Vector2(1.6, 0),    // Widest point (beam)
        new THREE.Vector2(1.5, 1.5)   // Sheer line
    ]);
    hullShape.lineTo(-1.5, 1.5);
    hullShape.splineThru([
        new THREE.Vector2(-1.6, 0),
        new THREE.Vector2(-1.5, -1.0),
        new THREE.Vector2(0, -1.5)
    ]);

    const extrudeSettings = {
        steps: 2,
        depth: 9.5, // This is the length of the boat
        bevelEnabled: true,
        bevelThickness: 0.5,
        bevelSize: 0.4,
        bevelOffset: 0,
        bevelSegments: 3
    };

    const geometry = new THREE.ExtrudeGeometry(hullShape, extrudeSettings);
    
    // We need to rotate the geometry so the extrusion (depth) runs along the X-axis
    // and the shape cross-section is in the Y/Z plane.
    geometry.rotateY(Math.PI / 2);
    geometry.center(); // Center it after rotation

    const material = createHullMaterial();

    const hull = new THREE.Mesh(geometry, material);
    hull.position.y = -0.5; // Adjust vertical position to sit IN the water
    hull.castShadow = true;
    hull.receiveShadow = true;
    
    return hull;
}
