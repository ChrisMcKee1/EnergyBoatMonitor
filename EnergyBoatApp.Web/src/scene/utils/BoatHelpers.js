/**
 * Helper utilities for boat positioning, colors, and status
 */

import { BOAT_COLORS, STATUS_LIGHT_COLORS, DOCK_LAT, DOCK_LON, DOCK_ROTATION, CENTER_LAT, CENTER_LON, SCALE_FACTOR_LAT, SCALE_FACTOR_LON } from './Constants';
import { latLonToSceneCoords } from './CoordinateConverter';

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

  // Special case: Maintenance boat moored at dock
  if (boatId === 'BOAT-004' || status === 'Maintenance') {
    const dockCoords = latLonToSceneCoords(DOCK_LAT, DOCK_LON);
    
    return {
      x: dockCoords.x + 6,  // 6 units to the side
      y: 1.5,               // Base water level
      z: dockCoords.z - 2,  // Slight offset forward
      rotationY: DOCK_ROTATION, // Match dock angle
    };
  }

  // Active boats: use geographic coordinates
  const coords = latLonToSceneCoords(latitude, longitude);
  
  return {
    x: coords.x,
    y: 1.5,  // Base water level
    z: coords.z,
    rotationY: -(heading * Math.PI / 180), // Convert heading to radians
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
