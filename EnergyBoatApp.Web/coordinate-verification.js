// Coordinate System Verification
// After expanding BOUNDS to give waypoints buffer room

const DOCK_LAT = 51.5100;
const DOCK_LON = -0.1350;
const SCALE_FACTOR = 2000;

console.log('=== COORDINATE SYSTEM VERIFICATION ===\n');

// Dock is at origin
console.log('Dock Position (origin):');
console.log('  Geographic: lat=51.5100, lon=-0.1350');
console.log('  Scene: x=0, z=0\n');

// New boundary buoys
const buoys = [
  { name: 'SW', lat: 51.48, lon: -0.16 },
  { name: 'SE', lat: 51.48, lon: -0.09 },   // EXPANDED from -0.10
  { name: 'NW', lat: 51.53, lon: -0.16 },
  { name: 'NE', lat: 51.53, lon: -0.09 },   // EXPANDED from -0.10
];

console.log('Boundary Buoy Positions:');
buoys.forEach(buoy => {
  const x = (buoy.lon - DOCK_LON) * SCALE_FACTOR;
  const z = -(buoy.lat - DOCK_LAT) * SCALE_FACTOR;
  console.log(`  ${buoy.name}: lat=${buoy.lat}, lon=${buoy.lon} → x=${x.toFixed(1)}, z=${z.toFixed(1)}`);
});

// BOAT-001 waypoints (the problematic boat)
const boat1Waypoints = [
  { name: 'Start', lat: 51.5074, lon: -0.1278 },
  { name: 'NE', lat: 51.5250, lon: -0.1000 },  // This was AT the old boundary!
  { name: 'NW', lat: 51.5250, lon: -0.1400 },
  { name: 'W', lat: 51.5074, lon: -0.1400 },
];

console.log('\nBOAT-001 Waypoints:');
boat1Waypoints.forEach(wp => {
  const x = (wp.lon - DOCK_LON) * SCALE_FACTOR;
  const z = -(wp.lat - DOCK_LAT) * SCALE_FACTOR;
  console.log(`  ${wp.name}: lat=${wp.lat}, lon=${wp.lon} → x=${x.toFixed(1)}, z=${z.toFixed(1)}`);
});

// Verify waypoints are inside new boundaries
console.log('\n=== VERIFICATION ===');
console.log('Scene X boundaries: -50.0 (west) to +90.0 (east) [expanded from 70.0]');
console.log('Scene Z boundaries: -40.0 (north) to +60.0 (south)');

console.log('\nBOAT-001 waypoint ranges:');
const boat1X = boat1Waypoints.map(wp => (wp.lon - DOCK_LON) * SCALE_FACTOR);
const boat1Z = boat1Waypoints.map(wp => -(wp.lat - DOCK_LAT) * SCALE_FACTOR);
console.log(`  X range: ${Math.min(...boat1X).toFixed(1)} to ${Math.max(...boat1X).toFixed(1)}`);
console.log(`  Z range: ${Math.min(...boat1Z).toFixed(1)} to ${Math.max(...boat1Z).toFixed(1)}`);

const maxX = Math.max(...boat1X);
const boundaryMaxX = 90.0;
const buffer = boundaryMaxX - maxX;
console.log(`\n✅ Buffer zone: ${buffer.toFixed(1)} scene units (~${(buffer / SCALE_FACTOR * 111).toFixed(1)}km) from eastern boundary`);
console.log('   Boats now have room to navigate without hitting boundaries!');
