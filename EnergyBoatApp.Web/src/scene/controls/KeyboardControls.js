/**
 * KeyboardControls Module
 * 
 * Handles keyboard input for camera movement (WASD and arrow keys).
 * Manages key press state and camera movement calculations.
 * 
 * Phase 5 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { KEYBOARD } from '../utils/Constants.js';

/**
 * Creates keyboard event handlers
 * 
 * @param {Object} keysPressed - Ref object to store pressed keys state
 * @returns {Object} Object containing handleKeyDown and handleKeyUp functions
 */
export function createKeyboardHandlers(keysPressed) {
  const handleKeyDown = (e) => {
    const key = e.key.toLowerCase();
    if (['w', 'a', 's', 'd', 'arrowup', 'arrowdown', 'arrowleft', 'arrowright'].includes(key)) {
      keysPressed.current[key] = true;
      e.preventDefault(); // Prevent page scrolling
    }
  };

  const handleKeyUp = (e) => {
    const key = e.key.toLowerCase();
    keysPressed.current[key] = false;
  };

  return { handleKeyDown, handleKeyUp };
}

/**
 * Attaches keyboard event listeners to window
 * 
 * @param {Function} handleKeyDown - Key down handler
 * @param {Function} handleKeyUp - Key up handler
 */
export function attachKeyboardListeners(handleKeyDown, handleKeyUp) {
  window.addEventListener('keydown', handleKeyDown);
  window.addEventListener('keyup', handleKeyUp);
}

/**
 * Removes keyboard event listeners from window
 * 
 * @param {Function} handleKeyDown - Key down handler
 * @param {Function} handleKeyUp - Key up handler
 */
export function removeKeyboardListeners(handleKeyDown, handleKeyUp) {
  window.removeEventListener('keydown', handleKeyDown);
  window.removeEventListener('keyup', handleKeyUp);
}

/**
 * Updates camera position based on keyboard input
 * 
 * @param {THREE.Camera} camera - The camera to move
 * @param {Object} controls - OrbitControls instance
 * @param {Object} keysPressed - Current key press state
 */
export function updateCameraFromKeyboard(camera, controls, keysPressed) {
  const moveSpeed = KEYBOARD.moveSpeed;
  const forward = new THREE.Vector3();
  const right = new THREE.Vector3();
  
  // Get camera forward direction (horizontal only)
  camera.getWorldDirection(forward);
  forward.y = 0; // Keep movement horizontal
  forward.normalize();
  
  // Calculate right direction
  right.crossVectors(forward, camera.up).normalize();

  // Apply movement based on pressed keys
  if (keysPressed.current['w'] || keysPressed.current['arrowup']) {
    camera.position.addScaledVector(forward, moveSpeed);
    controls.target.addScaledVector(forward, moveSpeed);
  }
  if (keysPressed.current['s'] || keysPressed.current['arrowdown']) {
    camera.position.addScaledVector(forward, -moveSpeed);
    controls.target.addScaledVector(forward, -moveSpeed);
  }
  if (keysPressed.current['a'] || keysPressed.current['arrowleft']) {
    camera.position.addScaledVector(right, -moveSpeed);
    controls.target.addScaledVector(right, -moveSpeed);
  }
  if (keysPressed.current['d'] || keysPressed.current['arrowright']) {
    camera.position.addScaledVector(right, moveSpeed);
    controls.target.addScaledVector(right, moveSpeed);
  }
}

export default {
  createKeyboardHandlers,
  attachKeyboardListeners,
  removeKeyboardListeners,
  updateCameraFromKeyboard,
};
