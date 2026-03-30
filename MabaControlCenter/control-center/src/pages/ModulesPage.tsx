import React from "react";
import { Link } from "react-router-dom";

export const ModulesPage: React.FC = () => {
  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Modules</h1>
      <ul>
        <li>
          <Link to="/modules/dexter">Dexter</Link>
        </li>
        <li>
          <Link to="/modules/maba-scara">MABA SCARA</Link>
        </li>
      </ul>
    </div>
  );
};
