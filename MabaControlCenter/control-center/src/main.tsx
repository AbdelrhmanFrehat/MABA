import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter, HashRouter } from "react-router-dom";
import { AppShell } from "./AppShell";
import { AppInitializer } from "./components/AppInitializer";
import "./styles.css";

const Router = window.location.protocol === "file:" ? HashRouter : BrowserRouter;

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <Router>
      <AppInitializer>
        <AppShell />
      </AppInitializer>
    </Router>
  </React.StrictMode>
);

