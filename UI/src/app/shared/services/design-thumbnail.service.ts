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
            return this.renderTightThumbnail(renderer.domElement, width, height);
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
        const fitDist = Math.max(fitHeightDistance, fitWidthDistance) * 1.12;
        camera.position.set(fitDist, fitDist * 0.45, fitDist * 0.9);
        camera.lookAt(0, 0, 0);
        camera.updateProjectionMatrix();
    }

    private renderTightThumbnail(source: HTMLCanvasElement, width: number, height: number): string {
        const srcCtx = source.getContext('2d', { willReadFrequently: true });
        if (!srcCtx) return source.toDataURL('image/png');

        const imageData = srcCtx.getImageData(0, 0, source.width, source.height).data;
        const bg = { r: 15, g: 29, b: 79 };
        const tolerance = 10;

        let minX = source.width;
        let minY = source.height;
        let maxX = 0;
        let maxY = 0;
        let found = false;

        for (let y = 0; y < source.height; y++) {
            for (let x = 0; x < source.width; x++) {
                const idx = (y * source.width + x) * 4;
                const r = imageData[idx];
                const g = imageData[idx + 1];
                const b = imageData[idx + 2];
                if (Math.abs(r - bg.r) > tolerance || Math.abs(g - bg.g) > tolerance || Math.abs(b - bg.b) > tolerance) {
                    found = true;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!found) return source.toDataURL('image/png');

        const cropW = Math.max(1, maxX - minX + 1);
        const cropH = Math.max(1, maxY - minY + 1);
        const margin = Math.round(Math.max(cropW, cropH) * 0.22);
        const sx = Math.max(0, minX - margin);
        const sy = Math.max(0, minY - margin);
        const sw = Math.min(source.width - sx, cropW + margin * 2);
        const sh = Math.min(source.height - sy, cropH + margin * 2);

        const output = document.createElement('canvas');
        output.width = width;
        output.height = height;
        const outCtx = output.getContext('2d');
        if (!outCtx) return source.toDataURL('image/png');

        outCtx.fillStyle = '#0f1d4f';
        outCtx.fillRect(0, 0, width, height);
        outCtx.drawImage(source, sx, sy, sw, sh, 0, 0, width, height);
        return output.toDataURL('image/png');
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
