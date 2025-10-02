/**
 * SceneSetup Module
 * 
 * Initializes Three.js scene, camera, and renderer with optimal settings.
 * Handles canvas configuration and render quality settings.
 * 
 * Phase 6 of refactoring - extracted from BoatScene.jsx
 */

import * as THREE from 'three';
import { CAMERA_INITIAL_POSITION } from '../utils/Constants.js';

/**
 * Creates Three.js scene
 * 
 * @returns {THREE.Scene} Initialized scene
 */
export function createScene() {
  const scene = new THREE.Scene();
  return scene;
}

/**
 * Creates perspective camera
 * 
 * @param {HTMLElement} container - Container element for aspect ratio calculation
 * @returns {THREE.PerspectiveCamera} Configured camera
 */
export function createCamera(container) {
  const camera = new THREE.PerspectiveCamera(
    60, // FOV
    container.clientWidth / container.clientHeight, // Aspect ratio
    0.1, // Near plane
    20000 // Far plane (increased for skybox)
  );
  
  camera.position.set(
    CAMERA_INITIAL_POSITION.x,
    CAMERA_INITIAL_POSITION.y,
    CAMERA_INITIAL_POSITION.z
  );
  camera.lookAt(0, 0, 0);
  
  return camera;
}

/**
 * Creates WebGL renderer with enhanced quality settings
 * 
 * @param {HTMLElement} container - Container element for renderer
 * @returns {THREE.WebGLRenderer} Configured renderer
 */
export function createRenderer(container) {
  const renderer = new THREE.WebGLRenderer({
    antialias: true,
    alpha: false,
    powerPreference: 'high-performance'
  });
  
  renderer.setSize(container.clientWidth, container.clientHeight);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  
  // Enable shadows
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;
  
  // Tone mapping for realistic brightness
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 0.5;
  
  // Canvas styling for proper interaction
  renderer.domElement.style.display = 'block';
  renderer.domElement.style.outline = 'none';
  renderer.domElement.style.touchAction = 'none';
  renderer.domElement.style.pointerEvents = 'auto';
  renderer.domElement.style.userSelect = 'none';
  
  return renderer;
}

/**
 * Handles window resize events
 * 
 * @param {THREE.Camera} camera - Camera to update
 * @param {THREE.WebGLRenderer} renderer - Renderer to update
 * @param {HTMLElement} container - Container element for new size
 */
export function handleResize(camera, renderer, container) {
  if (!container) return;
  
  camera.aspect = container.clientWidth / container.clientHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(container.clientWidth, container.clientHeight);
}

/**
 * Creates resize event handler
 * 
 * @param {THREE.Camera} camera - Camera to update
 * @param {THREE.WebGLRenderer} renderer - Renderer to update
 * @param {HTMLElement} container - Container element
 * @returns {Function} Resize handler function
 */
export function createResizeHandler(camera, renderer, container) {
  return () => handleResize(camera, renderer, container);
}

/**
 * Attaches resize listener to window
 * 
 * @param {Function} resizeHandler - Resize handler function
 */
export function attachResizeListener(resizeHandler) {
  window.addEventListener('resize', resizeHandler);
}

/**
 * Removes resize listener from window
 * 
 * @param {Function} resizeHandler - Resize handler function
 */
export function removeResizeListener(resizeHandler) {
  window.removeEventListener('resize', resizeHandler);
}

export default {
  createScene,
  createCamera,
  createRenderer,
  handleResize,
  createResizeHandler,
  attachResizeListener,
  removeResizeListener,
};
