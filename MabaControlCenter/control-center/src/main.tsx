import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { AppShell } from "./AppShell";
import { AppInitializer } from "./components/AppInitializer";
import "./styles.css";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <BrowserRouter>
      <AppInitializer>
        <AppShell />
      </AppInitializer>
    </BrowserRouter>
  </React.StrictMode>
);

