import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

type ViewerFormat = 'GLB' | 'GLTF' | 'STL' | 'OBJ' | string;

@Component({
    selector: 'app-design-3d-viewer',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="viewer-shell">
            <canvas #canvasEl class="viewer-canvas"></canvas>
            <div class="viewer-overlay" *ngIf="loading">Loading 3D preview...</div>
            <div class="viewer-overlay error" *ngIf="errorMessage">{{ errorMessage }}</div>
            <div class="viewer-hint" *ngIf="!errorMessage">Drag to rotate, wheel to zoom</div>
        </div>
    `,
    styles: [`
        .viewer-shell {
            position: relative;
            width: 100%;
            height: 100%;
            min-height: 100%;
            background: radial-gradient(circle at 25% 20%, #1f2d69 0%, #111a3d 40%, #0b122d 100%);
            border-radius: 0;
            overflow: hidden;
        }
        .viewer-canvas {
            width: 100%;
            height: 100%;
            display: block;
        }
        .viewer-overlay {
            position: absolute;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #dbeafe;
            font-weight: 600;
            background: rgba(7, 14, 35, 0.55);
            backdrop-filter: blur(1px);
        }
        .viewer-overlay.error {
            color: #fee2e2;
            background: rgba(80, 12, 18, 0.55);
            text-align: center;
            padding: 1rem;
        }
        .viewer-hint {
            position: absolute;
            right: 0.75rem;
            bottom: 0.65rem;
            font-size: 0.75rem;
            color: rgba(219, 234, 254, 0.85);
            background: rgba(17, 26, 61, 0.5);
            border: 1px solid rgba(147, 197, 253, 0.25);
            border-radius: 999px;
            padding: 0.2rem 0.55rem;
            pointer-events: none;
        }
    `]
})
export class Design3dViewerComponent implements AfterViewInit, OnChanges, OnDestroy {
    @Input() modelUrl = '';
    @Input() format: ViewerFormat = '';
    @Input() background = '#0f1737';

    @ViewChild('canvasEl', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

    loading = false;
    errorMessage = '';

    private started = false;
    private animationFrameId: number | null = null;
    private resizeObserver: ResizeObserver | null = null;

    private three: any;
    private scene: any;
    private camera: any;
    private renderer: any;
    private controls: any;
    private ambientLight: any;
    private keyLight: any;
    private fillLight: any;
    private currentModel: any;

    async ngAfterViewInit(): Promise<void> {
        this.started = true;
        await this.initThree();
        this.setupResizeObserver();
        await this.loadModel();
        this.animate();
    }

    async ngOnChanges(changes: SimpleChanges): Promise<void> {
        if (!this.started) return;
        if (changes['modelUrl'] || changes['format']) {
            await this.loadModel();
        }
    }

    ngOnDestroy(): void {
        if (this.animationFrameId) {
            cancelAnimationFrame(this.animationFrameId);
        }
        this.resizeObserver?.disconnect();
        this.disposeCurrentModel();
        this.controls?.dispose?.();
        this.renderer?.dispose?.();
    }

    private async initThree(): Promise<void> {
        const THREE = await import('three');
        const { OrbitControls } = await import('three/examples/jsm/controls/OrbitControls.js');

        this.three = THREE;
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(this.background);

        this.camera = new THREE.PerspectiveCamera(55, 1, 0.01, 2000);
        this.camera.position.set(2.2, 1.6, 2.2);

        this.renderer = new THREE.WebGLRenderer({
            canvas: this.canvasRef.nativeElement,
            antialias: true,
            alpha: false
        });
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        this.renderer.outputColorSpace = THREE.SRGBColorSpace;

        this.controls = new OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.08;
        this.controls.rotateSpeed = 0.85;
        this.controls.zoomSpeed = 0.8;

        this.ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
        this.keyLight = new THREE.DirectionalLight(0xffffff, 0.9);
        this.keyLight.position.set(3, 4, 5);
        this.fillLight = new THREE.DirectionalLight(0xa5b4fc, 0.5);
        this.fillLight.position.set(-3, -1, -3);
        this.scene.add(this.ambientLight, this.keyLight, this.fillLight);

        this.updateViewport();
    }

    private setupResizeObserver(): void {
        this.resizeObserver = new ResizeObserver(() => this.updateViewport());
        this.resizeObserver.observe(this.canvasRef.nativeElement);
    }

    private updateViewport(): void {
        if (!this.renderer || !this.camera) return;
        const canvas = this.canvasRef.nativeElement;
        const width = Math.max(1, canvas.clientWidth);
        const height = Math.max(1, canvas.clientHeight);
        this.renderer.setSize(width, height, false);
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
    }

    private async loadModel(): Promise<void> {
        if (!this.modelUrl?.trim()) {
            this.errorMessage = 'No preview model available.';
            return;
        }

        this.loading = true;
        this.errorMessage = '';
        this.disposeCurrentModel();

        try {
            const format = (this.format || this.extractFormatFromUrl(this.modelUrl)).toUpperCase();
            if (format === 'GLB' || format === 'GLTF') {
                await this.loadGltfModel(this.modelUrl);
            } else if (format === 'STL') {
                await this.loadStlModel(this.modelUrl);
            } else if (format === 'OBJ') {
                await this.loadObjModel(this.modelUrl);
            } else {
                this.errorMessage = `Preview not supported for ${format || 'this file type'}.`;
            }
        } catch {
            this.errorMessage = 'Failed to load 3D preview.';
        } finally {
            this.loading = false;
        }
    }

    private async loadGltfModel(url: string): Promise<void> {
        const { GLTFLoader } = await import('three/examples/jsm/loaders/GLTFLoader.js');
        const loader = new GLTFLoader();
        const gltf = await loader.loadAsync(url);
        this.currentModel = gltf.scene;
        this.scene.add(this.currentModel);
        this.centerAndFrame(this.currentModel);
    }

    private async loadStlModel(url: string): Promise<void> {
        const { STLLoader } = await import('three/examples/jsm/loaders/STLLoader.js');
        const geometry = await new STLLoader().loadAsync(url);
        geometry.computeVertexNormals();
        const material = new this.three.MeshStandardMaterial({
            color: 0x8da4ff,
            metalness: 0.15,
            roughness: 0.55
        });
        this.currentModel = new this.three.Mesh(geometry, material);
        this.scene.add(this.currentModel);
        this.centerAndFrame(this.currentModel);
    }

    private async loadObjModel(url: string): Promise<void> {
        const { OBJLoader } = await import('three/examples/jsm/loaders/OBJLoader.js');
        const obj = await new OBJLoader().loadAsync(url);
        obj.traverse((child: any) => {
            if (child.isMesh) {
                child.material = new this.three.MeshStandardMaterial({
                    color: 0x8da4ff,
                    metalness: 0.12,
                    roughness: 0.58
                });
            }
        });
        this.currentModel = obj;
        this.scene.add(this.currentModel);
        this.centerAndFrame(this.currentModel);
    }

    private centerAndFrame(object: any): void {
        const box = new this.three.Box3().setFromObject(object);
        const center = box.getCenter(new this.three.Vector3());
        const size = box.getSize(new this.three.Vector3());
        object.position.sub(center);

        const maxSize = Math.max(size.x, size.y, size.z) || 1;
        const fov = this.camera.fov * (Math.PI / 180);
        const fitHeightDistance = maxSize / (2 * Math.tan(fov / 2));
        const fitWidthDistance = fitHeightDistance / this.camera.aspect;
        const fitDist = Math.max(fitHeightDistance, fitWidthDistance) * 1.25;
        this.camera.position.set(fitDist, fitDist * 0.35, fitDist);
        this.controls.target.set(0, 0, 0);
        this.controls.minDistance = Math.max(0.1, fitDist * 0.2);
        this.controls.maxDistance = fitDist * 8;
        this.controls.update();
    }

    private disposeCurrentModel(): void {
        if (!this.currentModel || !this.scene) return;
        this.scene.remove(this.currentModel);
        this.currentModel.traverse?.((node: any) => {
            if (node.geometry) node.geometry.dispose?.();
            if (node.material) {
                if (Array.isArray(node.material)) {
                    node.material.forEach((m: any) => m.dispose?.());
                } else {
                    node.material.dispose?.();
                }
            }
        });
        this.currentModel = null;
    }

    private animate(): void {
        if (!this.renderer || !this.scene || !this.camera) return;
        this.animationFrameId = requestAnimationFrame(() => this.animate());
        this.controls?.update?.();
        this.renderer.render(this.scene, this.camera);
    }

    private extractFormatFromUrl(url: string): string {
        const cleaned = url.split('?')[0].split('#')[0];
        const ext = cleaned.split('.').pop() || '';
        return ext.toUpperCase();
    }
}
