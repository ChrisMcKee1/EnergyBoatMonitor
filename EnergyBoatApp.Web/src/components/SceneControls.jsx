import React from 'react';
import './SceneControls.css';

const SceneControls = ({ isDaytime, onToggleDayNight, onResetScene, speedMultiplier = 1.0, onSpeedChange }) => {
  const handleSpeedChange = (e) => {
    const value = parseFloat(e.target.value);
    onSpeedChange(value);
  };

  const getSpeedLabel = (speed) => {
    if (speed < 0.5) return 'Very Slow';
    if (speed < 1) return 'Slow';
    if (speed === 1) return 'Normal';
    if (speed <= 2) return 'Fast';
    return 'Very Fast';
  };

  return (
    <div className="scene-controls">
      <button 
        className="control-button day-night-toggle" 
        onClick={onToggleDayNight}
        title={isDaytime ? "Switch to Night" : "Switch to Day"}
      >
        <span className="icon">{isDaytime ? '🌙' : '☀️'}</span>
        <span className="label">{isDaytime ? 'Night' : 'Day'}</span>
      </button>
      
      <button 
        className="control-button reset-button" 
        onClick={onResetScene}
        title="Reset Scene"
      >
        <span className="icon">🔄</span>
        <span className="label">Reset</span>
      </button>

      <div className="speed-control">
        <div className="speed-header">
          <span className="speed-icon">⚡</span>
          <span className="speed-label">Simulation Speed</span>
        </div>
        <input
          type="range"
          min="0.1"
          max="10"
          step="0.1"
          value={speedMultiplier}
          onChange={handleSpeedChange}
          className="speed-slider"
          title={`Speed: ${speedMultiplier.toFixed(1)}x`}
        />
        <div className="speed-display">
          <span className="speed-value">{speedMultiplier.toFixed(1)}x</span>
          <span className="speed-description">{getSpeedLabel(speedMultiplier)}</span>
        </div>
      </div>
      
      <div className="keyboard-hint">
        <p>🎮 <strong>Camera Controls:</strong></p>
        <p>WASD / Arrow Keys - Move camera</p>
        <p>Mouse Drag - Rotate view</p>
        <p>Mouse Wheel - Zoom in/out</p>
      </div>
    </div>
  );
};

export default SceneControls;
