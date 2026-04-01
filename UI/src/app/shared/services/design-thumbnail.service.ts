import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class DesignThumbnailService {
    async generateThumbnail(modelUrl: string, format: string, width = 720, height = 420): Promise<string | null> {
        if (!modelUrl?.trim()) return null;

        const ext = (format || this.extractFormat(modelUrl)).toUpperCase();
        if (!['GLB', 'GLTF', 'STL', 'OBJ'].includes(ext)) return null;

        const THREE = await import('three');
        const scene = new THREE.Scene();
        scene.background = new THREE.Color(0x0f1d4f);

        const camera = new THREE.PerspectiveCamera(50, width / height, 0.01, 4000);
        camera.position.set(2.8, 1.9, 2.8);

        const renderer = new THREE.WebGLRenderer({
            antialias: true,
            alpha: false,
            preserveDrawingBuffer: true
        });
        renderer.setSize(width, height, false);
        renderer.setPixelRatio(1);
        renderer.outputColorSpace = THREE.SRGBColorSpace;

        scene.add(new THREE.AmbientLight(0xffffff, 0.75));
        const keyLight = new THREE.DirectionalLight(0xffffff, 0.95);
        keyLight.position.set(3, 4, 6);
        scene.add(keyLight);
        const fillLight = new THREE.DirectionalLight(0x9eb0ff, 0.45);
        fillLight.position.set(-3, -1, -4);
        scene.add(fillLight);

        let modelRoot: any = null;
        try {
            if (ext === 'GLB' || ext === 'GLTF') {
                const { GLTFLoader } = await import('three/examples/jsm/loaders/GLTFLoader.js');
                const gltf = await new GLTFLoader().loadAsync(modelUrl);
                modelRoot = gltf.scene;
            } else if (ext === 'STL') {
                const { STLLoader } = await import('three/examples/jsm/loaders/STLLoader.js');
                const geometry = await new STLLoader().loadAsync(modelUrl);
                geometry.computeVertexNormals();
                modelRoot = new THREE.Mesh(geometry, new THREE.MeshStandardMaterial({
                    color: 0x8da4ff,
                    metalness: 0.12,
                    roughness: 0.58
                }));
            } else if (ext === 'OBJ') {
                const { OBJLoader } = await import('three/examples/jsm/loaders/OBJLoader.js');
                modelRoot = await new OBJLoader().loadAsync(modelUrl);
                modelRoot.traverse((child: any) => {
                    if (child.isMesh) {
                        child.material = new THREE.MeshStandardMaterial({
                            color: 0x8da4ff,
                            metalness: 0.12,
                            roughness: 0.58
                        });
                    }
                });
            }

            if (!modelRoot) return null;
            scene.add(modelRoot);
            this.centerAndFrame(modelRoot, THREE, camera);
            renderer.render(scene, camera);
            return renderer.domElement.toDataURL('image/png');
        } catch {
            return null;
        } finally {
            if (modelRoot) {
                this.disposeObject(modelRoot);
            }
            renderer.dispose();
        }
    }

    private centerAndFrame(object: any, THREE: any, camera: any): void {
        const box = new THREE.Box3().setFromObject(object);
        const center = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());
        object.position.sub(center);

        const maxSize = Math.max(size.x, size.y, size.z) || 1;
        const fov = camera.fov * (Math.PI / 180);
        const fitHeightDistance = maxSize / (2 * Math.tan(fov / 2));
        const fitWidthDistance = fitHeightDistance / camera.aspect;
        const fitDist = Math.max(fitHeightDistance, fitWidthDistance) * 1.35;
        camera.position.set(fitDist, fitDist * 0.42, fitDist);
        camera.lookAt(0, 0, 0);
        camera.updateProjectionMatrix();
    }

    private disposeObject(root: any): void {
        root.traverse?.((node: any) => {
            node.geometry?.dispose?.();
            if (node.material) {
                if (Array.isArray(node.material)) {
                    node.material.forEach((m: any) => m?.dispose?.());
                } else {
                    node.material.dispose?.();
                }
            }
        });
    }

    private extractFormat(url: string): string {
        const clean = url.split('?')[0].split('#')[0];
        return (clean.split('.').pop() || '').toUpperCase();
    }
}
