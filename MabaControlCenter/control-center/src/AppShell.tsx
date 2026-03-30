import React, { Suspense } from "react";
import { Link, NavLink, Route, Routes } from "react-router-dom";
import { ModuleHost } from "./components/ModuleHost";
import { ModuleGuard } from "./components/ModuleGuard";
import { DashboardPage } from "./pages/DashboardPage";
import { ModulesPage } from "./pages/ModulesPage";
import { DevicesPage } from "./pages/DevicesPage";

const JobsPage = () => (
  <div>
    <h1 style={{ marginTop: 0 }}>Jobs</h1>
    <p>Job management will appear here.</p>
  </div>
);

const SettingsPage = () => (
  <div>
    <h1 style={{ marginTop: 0 }}>Settings</h1>
    <p>Application settings.</p>
  </div>
);

const DexterModuleRoute: React.FC = () => (
  <ModuleGuard>
    <Suspense fallback={<div>Loading module…</div>}>
      <ModuleHost moduleId="dexter" />
    </Suspense>
  </ModuleGuard>
);

const ScaraModuleRoute: React.FC = () => (
  <ModuleGuard>
    <Suspense fallback={<div>Loading module…</div>}>
      <ModuleHost moduleId="maba-scara" />
    </Suspense>
  </ModuleGuard>
);

export const AppShell: React.FC = () => {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="logo">
          <Link to="/">MABA Control Center</Link>
        </div>
        <nav>
          <ul>
            <li>
              <NavLink to="/" end>
                Dashboard
              </NavLink>
            </li>
            <li>
              <NavLink to="/modules">Modules</NavLink>
            </li>
            <li>
              <NavLink to="/devices">Devices</NavLink>
            </li>
            <li>
              <NavLink to="/jobs">Jobs</NavLink>
            </li>
            <li>
              <NavLink to="/settings">Settings</NavLink>
            </li>
          </ul>
        </nav>
      </aside>
      <main className="main">
        <header className="topbar">
          <div>Online status: TBD</div>
          <div>Sync status: TBD</div>
          <div>User: TBD</div>
        </header>
        <section className="content">
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/modules" element={<ModulesPage />} />
            <Route path="/modules/dexter/*" element={<DexterModuleRoute />} />
            <Route path="/modules/maba-scara/*" element={<ScaraModuleRoute />} />
            <Route path="/devices" element={<DevicesPage />} />
            <Route path="/jobs" element={<JobsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Routes>
        </section>
      </main>
    </div>
  );
};
