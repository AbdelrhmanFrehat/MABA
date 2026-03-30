export type ModuleRoute = {
  id: string;
  path: string;
  label: string;
};

export interface ModuleManifest {
  id: string;
  name: string;
  icon?: string;
  version: string;
  minCoreVersion: string;
  routes: ModuleRoute[];
  deviceTypes: string[];
  capabilities: string[];
}

