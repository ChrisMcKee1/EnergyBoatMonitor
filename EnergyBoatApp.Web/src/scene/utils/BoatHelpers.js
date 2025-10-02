/**
 * Helper utilities for boat positioning, colors, and status
 */

import * as THREE from 'three';
import { BOAT_COLORS, STATUS_LIGHT_COLORS, DOCK_LAT, DOCK_LON, DOCK_ROTATION, CENTER_LAT, CENTER_LON, SCALE_FACTOR_LAT, SCALE_FACTOR_LON } from './Constants';
import { latLonToSceneCoords } from './CoordinateConverter';

/**
 * Convert nautical heading (degrees) to Three.js Y-axis rotation (radians)
 * 
 * Coordinate system mapping:
 * - Nautical: 0° = North, 90° = East, 180° = South, 270° = West
 * - Our scene: -Z = North, +X = East, +Z = South, -X = West
 * - Boat default: Bow points in +X direction (East)
 * - Three.js rotation.y: 0 radians = +X axis
 * 
 * @param {number} heading - Nautical heading in degrees (0-360)
 * @returns {number} Three.js Y-axis rotation in radians
 */
export function headingToRotation(heading) {
  // Convert heading to radians and adjust for our coordinate system
  // Heading 0° (North) should point to -Z, but boat default is +X (East = 90°)
  // So we need: rotation = (90 - heading) in radians
  const rotationDegrees = 90 - heading;
  return THREE.MathUtils.degToRad(rotationDegrees);
}

/**
 * Get boat hull color based on status
 * @param {string} status - Boat status (Active, Charging, Maintenance)
 * @returns {number} Hex color code
 */
export function getBoatColor(status) {
  return BOAT_COLORS[status] || BOAT_COLORS.default;
}

/**
 * Get status indicator light color
 * @param {string} status - Boat status
 * @returns {number} Hex color code
 */
export function getStatusLightColor(status) {
  return STATUS_LIGHT_COLORS[status] || STATUS_LIGHT_COLORS.default;
}

/**
 * Calculate boat position in scene
 * Handles special case for maintenance boats moored at dock
 * @param {object} boat - Boat data from API
 * @returns {{x: number, y: number, z: number, rotationY: number}} Position and rotation
 */
export function calculateBoatPosition(boat) {
  const boatId = boat.id || boat.Id;
  const latitude = boat.latitude || boat.Latitude;
  const longitude = boat.longitude || boat.Longitude;
  const status = boat.status || boat.Status;
  const heading = boat.heading || boat.Heading || 0;

  // All boats: use geographic coordinates and heading
  const coords = latLonToSceneCoords(latitude, longitude);
  
  return {
    x: coords.x,
    y: 1.5,  // Base water level
    z: coords.z,
    rotationY: headingToRotation(heading), // Convert nautical heading to Three.js rotation
  };
}

/**
 * Normalize boat data (handle both lowercase and PascalCase API responses)
 * @param {object} boat - Raw boat data from API
 * @returns {object} Normalized boat data
 */
export function normalizeBoatData(boat) {
  return {
    id: boat.id || boat.Id,
    latitude: boat.latitude || boat.Latitude,
    longitude: boat.longitude || boat.Longitude,
    status: boat.status || boat.Status,
    heading: boat.heading || boat.Heading || 0,
    energyLevel: boat.energyLevel || boat.EnergyLevel,
    vesselName: boat.vesselName || boat.VesselName,
    speed: boat.speed || boat.Speed,
    speedKnots: boat.speedKnots || boat.SpeedKnots,
  };
}
