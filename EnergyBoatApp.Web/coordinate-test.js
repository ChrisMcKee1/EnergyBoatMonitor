// Quick test to understand the coordinate system
const CENTER_LAT = 51.5074;
const CENTER_LON = -0.1278;
const SCALE_FACTOR = 2000;

// Dock position
const DOCK_LAT = 51.5100;
const DOCK_LON = -0.1350;
const dockX = (DOCK_LON - CENTER_LON) * SCALE_FACTOR;
const dockZ = -(DOCK_LAT - CENTER_LAT) * SCALE_FACTOR;

console.log('=== COORDINATE ANALYSIS ===');
console.log('Dock position in scene:', { x: dockX.toFixed(2), z: dockZ.toFixed(2) });
console.log('Camera position:', { x: 30, y: 25, z: 30 });
console.log('Camera target:', { x: 0, y: 0, z: 0 });
console.log('');

// Example boat positions from console logs
const boats = [
  { id: 'BOAT-001', lat: 51.525, lon: -0.13 },
  { id: 'BOAT-002', lat: 51.495, lon: -0.14 },
  { id: 'BOAT-003', lat: 51.52, lon: -0.12 },
];

boats.forEach(boat => {
  const x = (boat.lon - CENTER_LON) * SCALE_FACTOR;
  const z = -(boat.lat - CENTER_LAT) * SCALE_FACTOR;
  console.log(`${boat.id} position:`, { x: x.toFixed(2), z: z.toFixed(2) });
});

console.log('');
console.log('PROBLEM: Dock is at (-14.4, -5.2), boats are at large distances');
console.log('Camera is looking at origin (0,0,0) but everything is offset!');
