import React from "react";
import { Link, Routes, Route, NavLink } from "react-router-dom";
import dexterModuleIcon from "../../assets/dexter-module-icon.svg";
import { DexterDeviceListPage } from "./DexterDeviceListPage";
import { DexterJobsPage } from "./DexterJobsPage";
import { DexterMainPage } from "./DexterMainPage";
import { DexterMacroPadPage } from "./DexterMacroPadPage";

const base = "/modules/dexter";

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
        <img
          src={dexterModuleIcon}
          alt=""
          width={36}
          height={36}
          style={{ borderRadius: 8 }}
        />
        <h1 style={{ margin: 0 }}>Dexter Module</h1>
      </header>
      <nav style={{ marginBottom: "1rem" }}>
        <NavLink to={base} end style={{ marginRight: "1rem" }}>
          Overview
        </NavLink>
        <NavLink to={`${base}/macropad`} style={{ marginRight: "1rem" }}>
          MacroPad
        </NavLink>
        <NavLink to={`${base}/devices`} style={{ marginRight: "1rem" }}>
          Devices
        </NavLink>
        <NavLink to={`${base}/jobs`}>Jobs</NavLink>
      </nav>
      <Routes>
        <Route path={base} element={<DexterMainPage />} />
        <Route path={`${base}/macropad`} element={<DexterMacroPadPage />} />
        <Route path={`${base}/devices`} element={<DexterDeviceListPage />} />
        <Route path={`${base}/jobs`} element={<DexterJobsPage />} />
      </Routes>
    </div>
  );
};
