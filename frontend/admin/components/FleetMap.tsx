"use client";

import { MapContainer, TileLayer, CircleMarker, Popup } from "react-leaflet";
import "leaflet/dist/leaflet.css";

interface Vehicle {
  id: string;
  modelName: string;
  licensePlate: string;
  status: string;
  type: string;
  battery: { percentage: number };
  currentLocation: { latitude: number; longitude: number };
}

function getColor(v: Vehicle): string {
  if (v.status !== "ACTIVE") return "#9ca3af";
  if (v.type === "LUXURY") return "#378ADD";
  if (v.type === "VAN") return "#EF9F27";
  return "#1D9E75";
}

export default function FleetMap({ vehicles }: { vehicles: Vehicle[] }) {
  const validVehicles = vehicles.filter(
    (v) => v.currentLocation?.latitude && v.currentLocation?.longitude
  );

  const center: [number, number] =
    validVehicles.length > 0
      ? [
          validVehicles[0].currentLocation.latitude,
          validVehicles[0].currentLocation.longitude,
        ]
      : [51.0543, 3.7174];

  return (
    <MapContainer
      center={center}
      zoom={13}
      style={{ height: "100%", width: "100%" }}
      scrollWheelZoom={true}
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {validVehicles.map((v) => (
        <CircleMarker
          key={v.id}
          center={[v.currentLocation.latitude, v.currentLocation.longitude]}
          radius={8}
          pathOptions={{
            color: "white",
            weight: 2,
            fillColor: getColor(v),
            fillOpacity: 1,
          }}
        >
          <Popup>
            <div style={{ fontSize: 13 }}>
              <div style={{ fontWeight: 600 }}>{v.modelName}</div>
              <div style={{ color: "#6b7280" }}>{v.licensePlate}</div>
              <div style={{ marginTop: 4 }}>
                <span style={{ color: v.status === "ACTIVE" ? "#1D9E75" : "#9ca3af" }}>
                  {v.status}
                </span>
                {" · "}
                <span style={{ color: "#6b7280" }}>{v.battery?.percentage}% battery</span>
              </div>
            </div>
          </Popup>
        </CircleMarker>
      ))}
    </MapContainer>
  );
}