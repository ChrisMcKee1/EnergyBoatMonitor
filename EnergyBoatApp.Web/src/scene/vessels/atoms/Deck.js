/**
 * @file Deck atom
 * Defines the basic deck geometry for the vessel.
 */
import * as THREE from 'three';

export function createDeck() {
    const geometry = new THREE.BoxGeometry(8.5, 0.2, 3.2); // Swapped width and depth
    const material = new THREE.MeshStandardMaterial({
        color: 0x777777, // Grey
        metalness: 0.5,
        roughness: 0.7
    });
    const deck = new THREE.Mesh(geometry, material);
    deck.position.y = 1.5; // Position it on top of the hull
    deck.receiveShadow = true;
    return deck;
}
