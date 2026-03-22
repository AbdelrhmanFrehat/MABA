import React from "react";
import { Link, Routes, Route, NavLink, Navigate } from "react-router-dom";
import { MabaScaraDeviceListPage } from "./MabaScaraDeviceListPage";
import { MabaScaraDiagnosticsPage } from "./MabaScaraDiagnosticsPage";

const base = "/modules/maba-scara";

export const RootComponent: React.FC = () => {
  return (
    <div>
      <header
        style={{
          marginBottom: "1rem",
          display: "flex",
          flexWrap: "wrap",
          alignItems: "center",
          gap: "1rem",
        }}
      >
        <Link to="/modules">← Back to Modules</Link>
        <h1 style={{ margin: 0 }}>MABA SCARA Module</h1>
      </header>
      <nav style={{ marginBottom: "1rem" }}>
        <NavLink to={`${base}/devices`} style={{ marginRight: "1rem" }}>
          Devices
        </NavLink>
        <NavLink to={`${base}/diagnostics`}>Diagnostics</NavLink>
      </nav>
      <Routes>
        <Route
          path={base}
          element={<Navigate to={`${base}/devices`} replace />}
        />
        <Route path={`${base}/devices`} element={<MabaScaraDeviceListPage />} />
        <Route
          path={`${base}/diagnostics`}
          element={<MabaScaraDiagnosticsPage />}
        />
      </Routes>
    </div>
  );
};
