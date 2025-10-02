import { useEffect, useRef, useState } from 'react';
import * as THREE from 'three';
import SceneControls from './SceneControls';

// Scene utilities
import { CAMERA_INITIAL_POSITION } from '../scene/utils/Constants';
import { getBoatColor, getStatusLightColor } from '../scene/utils/BoatHelpers';
import { CoordinateConverter } from '../scene/utils/CoordinateConverter';

// Core systems
import { createScene, createCamera, createRenderer, createResizeHandler, attachResizeListener, removeResizeListener } from '../scene/core/SceneSetup.js';
import { createLightingSystem } from '../scene/core/LightingSystem.js';
import { createAnimationLoop, startAnimation } from '../scene/core/AnimationLoop.js';

// Environment
import { createOcean } from '../scene/environment/OceanEnvironment.jsx';
import { createSkySystem, toggleDayNight as toggleDayNightSystem } from '../scene/environment/SkySystem.jsx';
import { createBoundaryBuoys } from '../scene/environment/NavigationBuoys.jsx';

// Vessels
import { createBoat as createBoatModel } from '../scene/vessels/BoatModel.jsx';

// Infrastructure  
import { createCompleteDock } from '../scene/infrastructure/DockStructure.jsx';

// Controls
import { createKeyboardHandlers, attachKeyboardListeners, removeKeyboardListeners } from '../scene/controls/KeyboardControls.js';
import { createOrbitControls, resetCamera as resetCameraControls } from '../scene/controls/CameraControls.js';

const BoatScene = ({ 
  boats = [], 
  selectedBoatId = null, 
  onResetScene, 
  resetTrigger = 0,
  speedMultiplier = 1.0,
  onSpeedChange
}) => {
  const mountRef = useRef(null);
  const sceneRef = useRef(null);
  const boatMeshesRef = useRef({});
  const cameraRef = useRef(null);
  const controlsRef = useRef(null);
  const skyRef = useRef(null);
  const sunLightRef = useRef(null);
  const sunRef = useRef(null); // Add sun vector ref for day/night toggle
  const moonRef = useRef(null); // Add moon ref for day/night toggle
  const rendererRef = useRef(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDaytime, setIsDaytime] = useState(true);
  
  // Store previous boat positions for interpolation
  const previousPositionsRef = useRef({});
  
  // Keyboard movement state
  const keysPressed = useRef({});
  
  // Ocean ref for animation
  const oceanRef = useRef(null);
  
  // Speed multiplier ref for animation loop (so speed changes work in real-time)
  const speedMultiplierRef = useRef(speedMultiplier);
  
  // Update speed multiplier ref when prop changes
  useEffect(() => {
    speedMultiplierRef.current = speedMultiplier;
  }, [speedMultiplier]);

  // Toggle day/night cycle using SkySystem module
  const toggleDayNight = () => {
    if (!skyRef.current || !sunLightRef.current || !sunRef.current || !moonRef.current || !rendererRef.current) return;

    // Use SkySystem module function - it will toggle internally and return new state
    const newIsDaytime = toggleDayNightSystem(
      skyRef.current,
      sunLightRef.current,
      rendererRef.current,
      sunRef.current, // Pass the sun vector
      moonRef.current, // Pass the moon mesh
      isDaytime // Pass CURRENT state, function will toggle it
    );
    
    // Update React state with the new value returned from toggleDayNightSystem
    setIsDaytime(newIsDaytime);
  };

  useEffect(() => {
    if (!mountRef.current) return;

    // ========================================
    // SCENE INITIALIZATION using modules
    // ========================================
    
    // Create scene, camera, and renderer
    const scene = createScene();
    sceneRef.current = scene;

    const camera = createCamera(mountRef.current);
    cameraRef.current = camera;

    const renderer = createRenderer(mountRef.current);
    rendererRef.current = renderer;
    mountRef.current.appendChild(renderer.domElement);

    // Create orbit controls for camera
    const controls = createOrbitControls(camera, renderer.domElement);
    controlsRef.current = controls;

    // Create sky system with sun
    const { sky, sunLight, sun, moon } = createSkySystem(scene);
    skyRef.current = sky;
    sunLightRef.current = sunLight;
    sunRef.current = sun; // Store sun vector for day/night toggle
    moonRef.current = moon; // Store moon mesh for day/night toggle

    // Create lighting system
    const lightingSystem = createLightingSystem(scene, sun);

    // Create animated ocean
    const ocean = createOcean(scene, sun, speedMultiplier);
    scene.add(ocean);
    oceanRef.current = ocean;

    // Create navigation buoys at boundaries
    const buoys = createBoundaryBuoys(scene);

    // Create dock structure
    const dock = createCompleteDock();
    scene.add(dock);

    // ========================================
    // KEYBOARD CONTROLS
    // ========================================
    
    const { handleKeyDown, handleKeyUp } = createKeyboardHandlers(keysPressed);
    attachKeyboardListeners(handleKeyDown, handleKeyUp);

    setIsLoading(false);

    // ========================================
    // ANIMATION LOOP
    // ========================================
    
    const animate = createAnimationLoop({
      scene,
      camera,
      renderer,
      controls,
      ocean,
      boatMeshes: boatMeshesRef.current,
      keysPressed,
      speedMultiplierRef,
    });

    startAnimation(animate);

    // ========================================
    // WINDOW RESIZE HANDLER
    // ========================================
    
    const handleResize = createResizeHandler(camera, renderer, mountRef.current);
    attachResizeListener(handleResize);

    // ========================================
    // CLEANUP
    // ========================================
    
    return () => {
      removeResizeListener(handleResize);
      removeKeyboardListeners(handleKeyDown, handleKeyUp);
      if (mountRef.current && renderer.domElement) {
        mountRef.current.removeChild(renderer.domElement);
      }
      renderer.dispose();
      controls.dispose();
      
      // Dispose geometries and materials
      scene.traverse((object) => {
        if (object.geometry) object.geometry.dispose();
        if (object.material) {
          if (Array.isArray(object.material)) {
            object.material.forEach(material => material.dispose());
          } else {
            object.material.dispose();
          }
        }
      });
    };
  }, []); // Empty deps - only run once on mount

  // Update boat positions when data changes
  useEffect(() => {
    if (!sceneRef.current || boats.length === 0) return;

    console.log('🚢 Updating boats, count:', boats.length, 'boats:', boats);

    boats.forEach((boat) => {
      const boatId = boat.id || boat.Id;
      const latitude = boat.latitude || boat.Latitude;
      const longitude = boat.longitude || boat.Longitude;
      const status = boat.status || boat.Status;
      
      console.log(`🚢 Processing boat ${boatId}:`, { latitude, longitude, status });
      
      // Create boat mesh if it doesn't exist
      if (!boatMeshesRef.current[boatId]) {
        console.log(`🚢 Creating new boat mesh for ${boatId}`);
        const boatMesh = createBoatModel(boat);
        boatMeshesRef.current[boatId] = boatMesh;
        sceneRef.current.add(boatMesh);
        console.log(`🚢 Boat mesh created and added to scene for ${boatId}`);
      }
      
      // Update boat position
      const boatMesh = boatMeshesRef.current[boatId];
      if (boatMesh && latitude !== undefined && longitude !== undefined) {
        // Special positioning for maintenance boats - moor alongside dock
        if (boatId === 'BOAT-004' || status === 'Maintenance') {
          const dockLatLon = { latitude: 51.5100, longitude: -0.1350 };
          const dockPos = CoordinateConverter.latLonToScene(dockLatLon);
          
          boatMesh.position.x = dockPos.x + 6;
          boatMesh.position.y = 1.0; // Float above water
          boatMesh.position.z = dockPos.z - 2;
          boatMesh.rotation.y = Math.PI / 4;
          console.log(`🚢 Positioned maintenance boat ${boatId} at:`, { x: (dockPos.x + 6).toFixed(2), z: (dockPos.z - 2).toFixed(2) });
        } else {
          // Active boats use their GPS coordinates
          const boatLatLon = { latitude, longitude };
          const boatPos = CoordinateConverter.latLonToScene(boatLatLon);
          
          boatMesh.position.x = boatPos.x;
          boatMesh.position.y = 1.0; // Float above water
          boatMesh.position.z = boatPos.z;
          
          const heading = boat.heading || boat.Heading || 0;
          boatMesh.rotation.y = -(heading * Math.PI / 180);
          console.log(`🚢 Positioned active boat ${boatId} at:`, { x: boatPos.x.toFixed(2), z: boatPos.z.toFixed(2) }, 'heading:', heading);
        }
      }
    });
  }, [boats]);

  // Handle boat selection and camera focus
  useEffect(() => {
    if (!selectedBoatId || !boatMeshesRef.current[selectedBoatId] || !cameraRef.current || !controlsRef.current) {
      // Reset all boats to normal scale
      Object.values(boatMeshesRef.current).forEach((boatMesh) => {
        if (boatMesh) {
          boatMesh.scale.set(1, 1, 1);
          // Reset emissive for all lights
          boatMesh.traverse((child) => {
            if (child.isMesh && child.material.emissive) {
              child.material.emissiveIntensity = child.userData.originalEmissive || 0.3;
            }
          });
        }
      });
      return;
    }

    const selectedBoat = boatMeshesRef.current[selectedBoatId];
    
    // Highlight selected boat
    Object.entries(boatMeshesRef.current).forEach(([id, boatMesh]) => {
      if (id === selectedBoatId) {
        // Scale up and add glow to selected boat
        boatMesh.scale.set(1.3, 1.3, 1.3);
        boatMesh.traverse((child) => {
          if (child.isMesh && child.material.emissive) {
            if (!child.userData.originalEmissive) {
              child.userData.originalEmissive = child.material.emissiveIntensity;
            }
            child.material.emissiveIntensity = 1.5;
          }
        });
      } else {
        // Normal scale for other boats
        boatMesh.scale.set(1, 1, 1);
        boatMesh.traverse((child) => {
          if (child.isMesh && child.material.emissive) {
            child.material.emissiveIntensity = child.userData.originalEmissive || 0.3;
          }
        });
      }
    });

    // Smoothly move camera to focus on selected boat
    const targetPosition = selectedBoat.position.clone();
    const camera = cameraRef.current;
    const controls = controlsRef.current;
    
    // Calculate a good viewing position
    const offset = new THREE.Vector3(15, 12, 15);
    const newCameraPos = targetPosition.clone().add(offset);
    
    // Smoothly animate camera
    const startPos = camera.position.clone();
    const startTarget = controls.target.clone();
    let progress = 0;
    
    const animateCamera = () => {
      progress += 0.05;
      if (progress < 1) {
        camera.position.lerpVectors(startPos, newCameraPos, progress);
        controls.target.lerpVectors(startTarget, targetPosition, progress);
        controls.update();
        requestAnimationFrame(animateCamera);
      } else {
        camera.position.copy(newCameraPos);
        controls.target.copy(targetPosition);
        controls.update();
      }
    };
    
    animateCamera();
  }, [selectedBoatId]);

  // Handle reset trigger from parent
  useEffect(() => {
    if (resetTrigger === 0) return; // Skip initial mount
    
    console.log('🔄 BoatScene reset triggered');
    
    // Reset camera position using CameraControls module
    if (cameraRef.current && controlsRef.current) {
      resetCameraControls(
        cameraRef.current, 
        controlsRef.current,
        CAMERA_INITIAL_POSITION,
        { x: 0, y: 0, z: 0 }
      );
    }
    
    // Reset to daytime - check current state and toggle if needed
    // Use setTimeout to avoid state update during render
    setTimeout(() => {
      if (!isDaytime) {
        toggleDayNight(); // This will toggle from night to day and update all lighting
      }
    }, 0);
  }, [resetTrigger]); // Only depend on resetTrigger to avoid infinite loops

  return (
    <div style={{ 
      position: 'relative', 
      width: '100%', 
      height: '100%', 
      overflow: 'hidden',
      pointerEvents: 'auto'
    }}>
      <div 
        ref={mountRef} 
        style={{ 
          width: '100%', 
          height: '100%',
          cursor: 'grab',
          touchAction: 'none',
          pointerEvents: 'auto',
          userSelect: 'none',
          position: 'relative',
          zIndex: 1
        }}
      />
      {isLoading && (
        <div style={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          color: 'white',
          fontSize: '24px',
          fontWeight: 'bold',
          textShadow: '2px 2px 4px rgba(0,0,0,0.8)'
        }}>
          Loading 3D Environment...
        </div>
      )}
      <SceneControls 
        isDaytime={isDaytime}
        onToggleDayNight={toggleDayNight}
        onResetScene={onResetScene}
        speedMultiplier={speedMultiplier}
        onSpeedChange={onSpeedChange}
      />
    </div>
  );
};

export default BoatScene;
