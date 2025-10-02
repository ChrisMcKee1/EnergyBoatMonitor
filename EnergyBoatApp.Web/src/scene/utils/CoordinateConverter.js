/**
 * Geographic coordinate to 3D scene coordinate conversion utilities
 * 
 * IMPORTANT: Scene is centered on the DOCK position for proper camera framing.
 * Dock is at origin (0,0,0), all other objects positioned relative to it.
 */

import { DOCK_LAT, DOCK_LON, SCALE_FACTOR_LAT, SCALE_FACTOR_LON } from './Constants';

/**
 * Convert latitude/longitude to 3D scene coordinates
 * Scene is centered on the dock (DOCK_LAT, DOCK_LON) which maps to origin (0,0,0)
 * @param {Object} coords - Coordinate object
 * @param {number} coords.latitude - Geographic latitude
 * @param {number} coords.longitude - Geographic longitude
 * @returns {{x: number, z: number}} Scene coordinates
 */
export function latLonToSceneCoords({ latitude, longitude }) {
  // Center scene on dock position instead of arbitrary center point
  const x = (longitude - DOCK_LON) * SCALE_FACTOR_LON;
  const z = -(latitude - DOCK_LAT) * SCALE_FACTOR_LAT; // Negative Z for north
  
  return { x, z };
}

/**
 * Convert scene coordinates back to latitude/longitude
 * @param {number} x - Scene X coordinate
 * @param {number} z - Scene Z coordinate
 * @returns {{latitude: number, longitude: number}} Geographic coordinates
 */
export function sceneCoordsToLatLon(x, z) {
  // Reverse conversion: scene is centered on dock
  const longitude = (x / SCALE_FACTOR_LON) + DOCK_LON;
  const latitude = -(z / SCALE_FACTOR_LAT) + DOCK_LAT;
  
  return { latitude, longitude };
}

/**
 * Calculate distance between two geographic points (in scene units)
 * @param {number} lat1 - First latitude
 * @param {number} lon1 - First longitude
 * @param {number} lat2 - Second latitude
 * @param {number} lon2 - Second longitude
 * @returns {number} Distance in scene units
 */
export function geoDistance(lat1, lon1, lat2, lon2) {
  const coord1 = latLonToSceneCoords({ latitude: lat1, longitude: lon1 });
  const coord2 = latLonToSceneCoords({ latitude: lat2, longitude: lon2 });
  
  const dx = coord2.x - coord1.x;
  const dz = coord2.z - coord1.z;
  
  return Math.sqrt(dx * dx + dz * dz);
}

// Export as object for convenience
export const CoordinateConverter = {
  latLonToScene: latLonToSceneCoords,
  sceneToLatLon: sceneCoordsToLatLon,
  geoDistance,
};

export default CoordinateConverter;
