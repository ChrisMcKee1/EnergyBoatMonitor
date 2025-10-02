import * as THREE from 'three';
import { createHull } from './atoms/Hull.js';
import { createDeck } from './atoms/Deck.js';
import { createSuperstructure } from './molecules/Superstructure.js';
import { createDeckEquipment } from './molecules/DeckEquipment.js';

/**
 * Creates a complete 3D model for a boat.
 * This model is an 'organism' in atomic design, composed of molecules and atoms.
 * @param {object} boat - The boat data object from the API.
 * @returns {THREE.Group} A Three.js group containing the complete boat model.
 */
export function createBoatModel(boat) {
    const boatModel = new THREE.Group();
    boatModel.name = `boat-${boat.id}`;

    // Atoms
    const hull = createHull();
    const deck = createDeck();

    // Molecules
    const superstructure = createSuperstructure();
    const { railings, crane } = createDeckEquipment();

    // Assemble the model
    // The positions of these components are defined within their respective modules,
    // relative to the model's origin.
    boatModel.add(hull);
    boatModel.add(deck);
    boatModel.add(superstructure);
    boatModel.add(railings);
    boatModel.add(crane);

    // The final world position, rotation, and status-based updates
    // will be handled in the `BoatScene.jsx` component where this model is used.

    return boatModel;
}

