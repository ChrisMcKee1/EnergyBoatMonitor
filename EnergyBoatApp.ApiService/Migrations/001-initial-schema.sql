-- =====================================================
-- Migration: 001-initial-schema.sql
-- Purpose: Create initial database schema for ContosoSeaDB
-- Feature: 001-migrate-from-mock
-- Created: October 3, 2025
-- =====================================================

-- Drop tables if they exist (for clean re-runs during development)
DROP TABLE IF EXISTS waypoints CASCADE;
DROP TABLE IF EXISTS routes CASCADE;
DROP TABLE IF EXISTS boat_states CASCADE;
DROP TABLE IF EXISTS boats CASCADE;

-- =====================================================
-- Table: boats (Static Vessel Metadata)
-- =====================================================
CREATE TABLE boats (
    id              VARCHAR(20) PRIMARY KEY,  -- e.g., "BOAT-001"
    vessel_name     VARCHAR(100) NOT NULL,    -- e.g., "Contoso Sea Voyager"
    crew_count      INT NOT NULL CHECK (crew_count > 0),
    equipment       TEXT NOT NULL,            -- e.g., "Multibeam Sonar, Magnetometer"
    project         VARCHAR(255) NOT NULL,    -- e.g., "Dogger Bank Offshore Wind Farm"
    survey_type     VARCHAR(100) NOT NULL,    -- e.g., "Geophysical Survey"
    
    created_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for boats table
CREATE INDEX idx_boats_vessel_name ON boats(vessel_name);

COMMENT ON TABLE boats IS 'Static vessel metadata (equipment, crew, project details)';
COMMENT ON COLUMN boats.id IS 'Unique boat identifier (e.g., BOAT-001)';
COMMENT ON COLUMN boats.vessel_name IS 'Vessel display name';

-- =====================================================
-- Table: boat_states (Dynamic Runtime State)
-- =====================================================
CREATE TABLE boat_states (
    boat_id                 VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    
    -- Geographic position
    latitude                DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude               DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    heading                 DOUBLE PRECISION NOT NULL CHECK (heading BETWEEN 0 AND 360),
    
    -- Speed and energy
    speed_knots             DOUBLE PRECISION NOT NULL CHECK (speed_knots >= 0),
    original_speed_knots    DOUBLE PRECISION NOT NULL CHECK (original_speed_knots >= 0),
    energy_level            DOUBLE PRECISION NOT NULL CHECK (energy_level BETWEEN 0 AND 100),
    
    -- Status and conditions
    status                  VARCHAR(20) NOT NULL CHECK (status IN ('Active', 'Charging', 'Maintenance')),
    speed                   VARCHAR(50) NOT NULL,      -- Human-readable (e.g., "12 knots", "Station keeping")
    conditions              VARCHAR(255) NOT NULL,     -- e.g., "Moderate seas, 2m swell"
    area_covered            DOUBLE PRECISION DEFAULT 0 CHECK (area_covered >= 0),
    
    -- Navigation state
    current_waypoint_index  INT NOT NULL DEFAULT 0 CHECK (current_waypoint_index >= 0),
    
    -- Audit
    last_updated            TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for boat_states table
CREATE INDEX idx_boat_states_status ON boat_states(status);
CREATE INDEX idx_boat_states_energy ON boat_states(energy_level);
CREATE INDEX idx_boat_states_updated ON boat_states(last_updated DESC);

COMMENT ON TABLE boat_states IS 'Dynamic boat runtime state (position, heading, energy)';
COMMENT ON COLUMN boat_states.latitude IS 'Current latitude (-90 to 90 degrees)';
COMMENT ON COLUMN boat_states.longitude IS 'Current longitude (-180 to 180 degrees)';
COMMENT ON COLUMN boat_states.heading IS 'Current heading (0-360 degrees, 0=North)';
COMMENT ON COLUMN boat_states.status IS 'Boat operational status: Active, Charging, or Maintenance';

-- =====================================================
-- Table: routes (Survey Path Metadata)
-- =====================================================
CREATE TABLE routes (
    boat_id     VARCHAR(20) PRIMARY KEY REFERENCES boats(id) ON DELETE CASCADE,
    route_name  VARCHAR(100) NOT NULL,  -- e.g., "Rectangle Pattern - NE Quadrant"
    
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE routes IS 'Survey route metadata (one route per boat)';
COMMENT ON COLUMN routes.route_name IS 'Human-readable route description';

-- =====================================================
-- Table: waypoints (Geographic Survey Points)
-- =====================================================
CREATE TABLE waypoints (
    id          SERIAL PRIMARY KEY,
    boat_id     VARCHAR(20) NOT NULL REFERENCES boats(id) ON DELETE CASCADE,
    latitude    DOUBLE PRECISION NOT NULL CHECK (latitude BETWEEN -90 AND 90),
    longitude   DOUBLE PRECISION NOT NULL CHECK (longitude BETWEEN -180 AND 180),
    sequence    INT NOT NULL CHECK (sequence >= 0),
    
    created_at  TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE (boat_id, sequence)  -- Ensure no duplicate sequence numbers per boat
);

-- Indexes for waypoints table
CREATE INDEX idx_waypoints_boat_sequence ON waypoints(boat_id, sequence);

COMMENT ON TABLE waypoints IS 'Ordered geographic survey waypoints for each boat route';
COMMENT ON COLUMN waypoints.sequence IS 'Waypoint order (0-based index)';
COMMENT ON COLUMN waypoints.latitude IS 'Waypoint latitude (-90 to 90 degrees)';
COMMENT ON COLUMN waypoints.longitude IS 'Waypoint longitude (-180 to 180 degrees)';

-- =====================================================
-- Verification Queries (for development/testing)
-- =====================================================
-- Uncomment to verify schema after running migration:
-- SELECT table_name, column_name, data_type, is_nullable 
-- FROM information_schema.columns 
-- WHERE table_schema = 'public' 
-- ORDER BY table_name, ordinal_position;

-- SELECT indexname, indexdef 
-- FROM pg_indexes 
-- WHERE schemaname = 'public' 
-- ORDER BY tablename, indexname;
