import { useState, useEffect } from 'react';
import BoatScene from './components/BoatScene';
import './App.css';

function App() {
  const [boats, setBoats] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastUpdate, setLastUpdate] = useState(new Date());
  const [selectedBoat, setSelectedBoat] = useState(null);
  const [resetTrigger, setResetTrigger] = useState(0);
  const [speedMultiplier, setSpeedMultiplier] = useState(1.0); // 0.1x to 10x
  const [simulatedTime, setSimulatedTime] = useState(new Date()); // Simulated time that advances with speed

  // Fetch boat data from API
  useEffect(() => {
    const fetchBoats = async () => {
      try {
        // Use service discovery URL pattern for Aspire
        const apiUrl = import.meta.env.VITE_API_URL || '/api/boats';
        // Pass speed multiplier to API for time simulation
        const urlWithSpeed = `${apiUrl}?speed=${speedMultiplier}`;
        const response = await fetch(urlWithSpeed);
        
        if (!response.ok) {
          throw new Error('Failed to fetch boat data');
        }
        
        const data = await response.json();
        console.log('🚢 Fetched boat data:', data.map(b => ({ 
          id: b.id || b.Id, 
          lat: b.latitude || b.Latitude, 
          lon: b.longitude || b.Longitude,
          status: b.status || b.Status 
        })));
        setBoats(data);
        setLoading(false);
        setLastUpdate(new Date());
      } catch (err) {
        setError(err.message);
        setLoading(false);
      }
    };

    fetchBoats();
    
    // Adjust polling rate based on speed multiplier
    // Faster speed = more frequent updates for smoothness
    // Slower speed = less frequent updates to reduce traffic
    const baseInterval = 200; // 200ms base (5 Hz)
    const adjustedInterval = Math.max(50, Math.min(2000, baseInterval / Math.sqrt(speedMultiplier)));
    
    console.log(`⚙️ API polling interval: ${adjustedInterval.toFixed(0)}ms (speed: ${speedMultiplier}x)`);
    
    const interval = setInterval(fetchBoats, adjustedInterval);
    return () => clearInterval(interval);
  }, [speedMultiplier]); // Re-create interval when speed changes

  // Simulated time advancement - ticks faster/slower based on speedMultiplier
  useEffect(() => {
    const tickInterval = 100; // Update simulated time every 100ms
    const interval = setInterval(() => {
      setSimulatedTime(prevTime => {
        // Advance simulated time by (real elapsed time * speedMultiplier)
        const advanceMs = tickInterval * speedMultiplier;
        return new Date(prevTime.getTime() + advanceMs);
      });
    }, tickInterval);
    
    return () => clearInterval(interval);
  }, [speedMultiplier]);

  // Reset scene handler
  const handleResetScene = async () => {
    console.log('🔄 Resetting scene...');
    
    // 1. Reset speed multiplier to 1.0x
    setSpeedMultiplier(1.0);
    
    // 2. Reset simulated time to current real time
    setSimulatedTime(new Date());
    
    // 3. Reset selected boat
    setSelectedBoat(null);
    
    // 4. Call API to reset boats to initial positions
    try {
      const apiUrl = import.meta.env.VITE_API_URL || '/api/boats';
      const resetUrl = `${apiUrl}/reset`;
      const response = await fetch(resetUrl, { method: 'POST' });
      
      if (response.ok) {
        console.log('✅ Boats reset to initial positions via API');
      } else {
        console.error('❌ Failed to reset boats via API');
      }
    } catch (err) {
      console.error('❌ Error calling reset API:', err);
    }
    
    // 5. Trigger BoatScene reset (camera position, day/night)
    setResetTrigger(prev => prev + 1);
  };

  // Speed multiplier handler
  const handleSpeedChange = (newSpeed) => {
    console.log(`🎚️ Speed multiplier changed: ${newSpeed}x`);
    setSpeedMultiplier(newSpeed);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Active':
        return '#00FF00';
      case 'Charging':
        return '#FFAA00';
      case 'Maintenance':
        return '#FF0000';
      default:
        return '#888888';
    }
  };

  const getEnergyColor = (level) => {
    if (level > 70) return '#00FF00';
    if (level > 30) return '#FFAA00';
    return '#FF0000';
  };

  const handleBoatClick = (boat) => {
    setSelectedBoat(boat.id || boat.Id);
  };

  return (
    <div className="app">
      <header className="app-header">
        <div className="header-content">
          <div className="brand">
            <div className="fugro-logo">FUGRO</div>
            <h1>Marine Fleet Monitor</h1>
            <p className="tagline">Real-time Geo-data Visualization & Fleet Management</p>
          </div>
          <div className="header-stats">
            <div className="stat">
              <span className="stat-label">Total Vessels</span>
              <span className="stat-value">{boats.length}</span>
            </div>
            <div className="stat">
              <span className="stat-label">Active</span>
              <span className="stat-value">
                {boats.filter(b => (b.status || b.Status) === 'Active').length}
              </span>
            </div>
            <div className="stat">
              <span className="stat-label">Simulated Time</span>
              <span className="stat-value">{simulatedTime.toLocaleTimeString()}</span>
            </div>
          </div>
        </div>
      </header>
      
      {loading && <div className="loading-screen">Loading 3D Environment...</div>}
      {error && <div className="error-message">Error: {error}</div>}

      {!loading && !error && (
        <div className="content">
          <div className="scene-container">
            <BoatScene 
              boats={boats} 
              selectedBoatId={selectedBoat} 
              onResetScene={handleResetScene}
              resetTrigger={resetTrigger}
              speedMultiplier={speedMultiplier}
              onSpeedChange={handleSpeedChange}
            />
            <div className="scene-overlay">
              <div className="legend">
                <h3>Status Indicators</h3>
                <div className="legend-item">
                  <span className="legend-color" style={{ backgroundColor: '#00FF00' }}></span>
                  <span>Active Operations</span>
                </div>
                <div className="legend-item">
                  <span className="legend-color" style={{ backgroundColor: '#FFAA00' }}></span>
                  <span>Charging/Standby</span>
                </div>
                <div className="legend-item">
                  <span className="legend-color" style={{ backgroundColor: '#FF0000' }}></span>
                  <span>Maintenance Required</span>
                </div>
              </div>
            </div>
          </div>
          
          <div className="boat-list">
            <h2>Fleet Status Dashboard</h2>
            <div className="boat-cards">
              {boats.map((boat) => {
                const boatId = boat.id || boat.Id;
                const isSelected = selectedBoat === boatId;
                return (
                  <div 
                    key={boatId} 
                    className={`boat-card ${isSelected ? 'selected' : ''}`}
                    onClick={() => handleBoatClick(boat)}
                    style={{ cursor: 'pointer' }}
                  >
                    <div className="card-header">
                      <div>
                        <h3>{boatId}</h3>
                        <p className="vessel-name">{boat.vesselName || boat.VesselName || 'Survey Vessel'}</p>
                      </div>
                      <span 
                        className="status-indicator"
                        style={{ backgroundColor: getStatusColor(boat.status || boat.Status) }}
                      ></span>
                    </div>
                    <div className="card-body">
                      <div className="boat-info-row">
                        <span className="info-label">Status:</span>
                        <span className="info-value" style={{ color: getStatusColor(boat.status || boat.Status) }}>
                          {boat.status || boat.Status}
                        </span>
                      </div>
                      
                      {(boat.project || boat.Project) && (
                        <div className="boat-info-row">
                          <span className="info-label">Project:</span>
                          <span className="info-value project-name">{boat.project || boat.Project}</span>
                        </div>
                      )}
                      
                      {(boat.surveyType || boat.SurveyType) && (
                        <div className="boat-info-row">
                          <span className="info-label">Survey Type:</span>
                          <span className="info-value">{boat.surveyType || boat.SurveyType}</span>
                        </div>
                      )}
                      
                      <div className="boat-info-row">
                        <span className="info-label">Energy:</span>
                        <span className="info-value">
                          <span style={{ color: getEnergyColor(boat.energyLevel || boat.EnergyLevel) }}>
                            {((boat.energyLevel || boat.EnergyLevel) ?? 0).toFixed(1)}%
                          </span>
                        </span>
                      </div>
                      <div className="energy-bar-container">
                        <div className="energy-bar-label">
                          <span>Battery</span>
                          <span>{((boat.energyLevel || boat.EnergyLevel) ?? 0).toFixed(0)}%</span>
                        </div>
                        <div className="energy-bar-bg">
                          <div 
                            className="energy-bar-fill" 
                            style={{ 
                              width: `${(boat.energyLevel || boat.EnergyLevel) ?? 0}%`,
                              backgroundColor: getEnergyColor(boat.energyLevel || boat.EnergyLevel)
                            }}
                          ></div>
                        </div>
                      </div>
                      
                      {(boat.equipment || boat.Equipment) && (
                        <div className="boat-info-row">
                          <span className="info-label">Equipment:</span>
                          <span className="info-value equipment">{boat.equipment || boat.Equipment}</span>
                        </div>
                      )}
                      
                      {(boat.areaCovered || boat.AreaCovered) !== undefined && (boat.areaCovered || boat.AreaCovered) > 0 && (
                        <div className="boat-info-row">
                          <span className="info-label">Area Covered:</span>
                          <span className="info-value">{(boat.areaCovered || boat.AreaCovered).toFixed(1)} km²</span>
                        </div>
                      )}
                      
                      {(boat.speed || boat.Speed) && (
                        <div className="boat-info-row">
                          <span className="info-label">Speed:</span>
                          <span className="info-value">{boat.speed || boat.Speed}</span>
                        </div>
                      )}
                      
                      {(boat.crewCount || boat.CrewCount) && (
                        <div className="boat-info-row">
                          <span className="info-label">Crew:</span>
                          <span className="info-value">{boat.crewCount || boat.CrewCount} personnel</span>
                        </div>
                      )}
                      
                      {(boat.conditions || boat.Conditions) && (
                        <div className="boat-info-row">
                          <span className="info-label">Conditions:</span>
                          <span className="info-value conditions">{boat.conditions || boat.Conditions}</span>
                        </div>
                      )}
                      
                      <div className="boat-info-row">
                        <span className="info-label">Position:</span>
                        <span className="info-value coordinates">
                          {((boat.latitude || boat.Latitude) ?? 0).toFixed(4)}°N, {Math.abs((boat.longitude || boat.Longitude) ?? 0).toFixed(4)}°W
                        </span>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}
      
      <footer className="app-footer">
        <p>© 2025 Fugro | Creating a safe and liveable world through Geo-data intelligence</p>
      </footer>
    </div>
  );
}

export default App;

