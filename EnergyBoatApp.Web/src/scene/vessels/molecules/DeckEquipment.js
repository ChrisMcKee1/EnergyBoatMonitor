/**
 * @file DeckEquipment molecule
 * Assembles various pieces of equipment found on the deck.
 */
import * as THREE from 'three';
import { createRailingMaterial, createCraneMaterial } from '../BoatMaterials.js';

// Helper function to create a single railing section
function createRailingSection(length, orientation) {
    const railing = new THREE.Group();
    const railMaterial = createRailingMaterial();
    
    // Use a BoxGeometry for rails for a cleaner, more modern look.
    const railGeom = new THREE.BoxGeometry(length, 0.05, 0.05);
    const topRail = new THREE.Mesh(railGeom, railMaterial);
    topRail.position.y = 0.5; // Height of the railing
    railing.add(topRail);

    // Vertical posts
    const postGeom = new THREE.CylinderGeometry(0.03, 0.03, 0.5, 6);
    const numPosts = Math.max(2, Math.floor(length / 0.8)); // Ensure at least 2 posts

    for (let i = 0; i <= numPosts; i++) {
        const post = new THREE.Mesh(postGeom, railMaterial);
        const pos = (i / numPosts - 0.5) * length;
        
        if (orientation === 'x') { // Railing runs along the X-axis
            post.position.x = pos;
        } else { // Railing runs along the Z-axis
            post.position.z = pos;
        }
        post.position.y = 0.25; // Position posts under the rail
        railing.add(post);
    }
    
    // Rotate the entire group for Z-axis alignment
    if (orientation === 'z') {
        railing.rotation.y = Math.PI / 2;
    }

    return railing;
}

/**
 * Creates and assembles all deck equipment, including railings and a crane.
 */
export function createDeckEquipment() {
    // 1. Create Railings
    const railings = new THREE.Group();
    const deckWidth = 3.2;
    const deckLength = 8.5;
    const deckY = 1.6; // Y position of the main deck

    const sideRailingLength = deckLength * 0.95;
    const portRailing = createRailingSection(sideRailingLength, 'x');
    portRailing.position.set(0, deckY, -deckWidth / 2);
    railings.add(portRailing);

    const starboardRailing = createRailingSection(sideRailingLength, 'x');
    starboardRailing.position.set(0, deckY, deckWidth / 2);
    railings.add(starboardRailing);
    
    const endRailingLength = deckWidth;
    const bowRailing = createRailingSection(endRailingLength, 'x');
    bowRailing.rotation.y = Math.PI / 2;
    bowRailing.position.set(deckLength / 2 * 0.95, deckY, 0);
    railings.add(bowRailing);

    const sternRailing = createRailingSection(endRailingLength, 'x');
    sternRailing.rotation.y = Math.PI / 2;
    sternRailing.position.set(-deckLength / 2 * 0.95, deckY, 0);
    railings.add(sternRailing);

    // 2. Create Crane
    const crane = new THREE.Group();
    const baseMaterial = createCraneMaterial();
    const armMaterial = new THREE.MeshStandardMaterial({ color: 0xBBAA00, metalness: 0.8, roughness: 0.4 });

    // Base of the crane (cylinder height = 0.8)
    const baseHeight = 0.8;
    const base = new THREE.Mesh(
        new THREE.CylinderGeometry(0.25, 0.3, baseHeight, 12), 
        baseMaterial
    );
    // Position base so its BOTTOM sits at y=0 (the crane group origin, which is on the deck)
    // Cylinder is centered by default, so we need to move it up by half its height
    base.position.y = baseHeight / 2; // 0.4
    base.castShadow = true;
    
    // Arm of the crane - PARENTED to base and positioned at its TOP
    const armLength = 1.8;
    const armWidth = 0.15;
    const arm = new THREE.Mesh(
        new THREE.BoxGeometry(armWidth, armLength, armWidth), 
        armMaterial
    );
    
    // Position arm RELATIVE TO BASE:
    // - Base's local space has its top at y = baseHeight/2 = 0.4
    // - We want arm's bottom at base's top, so arm center is at 0.4 + armLength/2
    // - But base is already at y=0.4, so relative position is: (baseHeight/2) + (armLength/2)
    arm.position.y = (baseHeight / 2)+ (armLength / 2 ) - .2; // 0.4 + 0.9 = 1.3
    arm.position.x = -0.5;
    arm.rotation.z = Math.PI / 5; // Tilt the arm from its attachment point
    arm.castShadow = true;
    
    // Parent arm to base so they move together and rotation pivot is correct
    base.add(arm);
    
    // Add base (with arm as child) to crane group
    crane.add(base);

    // Position the entire crane assembly on the deck surface
    crane.position.set(-3.5, deckY, -1.0);

    // 3. Return all equipment
    return { railings, crane };
}

