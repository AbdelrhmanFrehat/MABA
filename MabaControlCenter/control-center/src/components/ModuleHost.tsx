import React, { useEffect, useState } from "react";

type ModuleHostProps = {
  moduleId: string;
};

export const ModuleHost: React.FC<ModuleHostProps> = ({ moduleId }) => {
  const [Loaded, setLoaded] = useState<React.ComponentType | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        // For now, assume local modules under src/modules
        const mod = await import(`../modules/${moduleId}/index`);
        if (!cancelled && mod && mod.RootComponent) {
          setLoaded(() => mod.RootComponent);
        }
      } catch (err) {
        console.error("Failed to load module", moduleId, err);
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, [moduleId]);

  if (!Loaded) {
    return <div>Loading module...</div>;
  }

  return <Loaded />;
};

