# Hero Parallelogram Image Ticker — Phase 0: Proposal & Gap Check

## 1) Integration point on Home (no layout shift)

**Current structure (unchanged):**
```
home-page
  └── section.hero-section
        ├── .hero-bg
        ├── .hero-circle (x3)
        └── .hero-content   ← title, tagline, buttons (DO NOT TOUCH)
```

**Proposed integration:**
```
home-page
  └── section.hero-section
        ├── .hero-bg
        ├── .hero-circle (x3)
        ├── app-hero-ticker   ← NEW: above content, inside same section
        └── .hero-content    ← unchanged
```

- **Path:** `UI/src/app/features/public/home/home.component.ts` — insert `<app-hero-ticker />` (or wrapper with ticker) immediately before `<div class="hero-content">`, inside `<section class="hero-section">`.
- **Layout shift prevention:**
  - Ticker wrapper has fixed height (e.g. 120px desktop, 0 or 72px mobile when simplified/hidden).
  - Hero section already uses `display: flex; align-items: center; justify-content: center` and `padding: calc(70px + 3rem) 2rem 2.75rem`. We add the ticker as a **sibling before** `.hero-content`, and make the hero section a **column flex** so order is: ticker (top), then content (centered). Alternatively keep hero as-is and place ticker **absolutely** at top of `.hero-section` with `position: absolute; top: 0; left: 0; right: 0; z-index: 5` so it overlays the top of the hero without affecting `.hero-content` flow. **Chosen:** Absolute positioning at top of hero so zero layout shift for existing hero content.
- **Header:** No change. Ticker sits below header (hero-section is already below any app header).

---

## 2) DOM structure + CSS approach (skew vs clip-path, fallback)

**DOM structure (minimal):**
```html
<div class="hero-ticker" role="presentation" aria-hidden="true">
  <div class="hero-ticker__mask hero-ticker__mask--left"></div>
  <div class="hero-ticker__mask hero-ticker__mask--right"></div>
  <div class="hero-ticker__track">
    <div class="hero-ticker__track-inner">
      <div class="hero-ticker__tile" *ngFor="let item of items">
        <div class="hero-ticker__tile-inner">
          <img [src]="item.imageUrl" [alt]="item.title || ''" />
        </div>
      </div>
      <!-- duplicate set for seamless loop -->
      <div class="hero-ticker__tile" *ngFor="let item of items">
        <div class="hero-ticker__tile-inner">
          <img [src]="item.imageUrl" [alt]="item.title || ''" />
        </div>
      </div>
    </div>
  </div>
</div>
```

**Parallelogram technique:**
- **Primary:** Skew wrapper (better GPU/transform, fewer jaggies). Each tile: outer `hero-ticker__tile` with `transform: skewX(-12deg)` (or similar), inner `hero-ticker__tile-inner` with `transform: skewX(12deg)` so image content is straight; image `object-fit: cover` and overflow hidden on inner.
- **Fallback:** If skew causes layout issues on a browser, we can use `clip-path: polygon(...)` on the tile to form a parallelogram. Prefer skew for performance (transform-only).
- **Sizing:** Tile width ~160–200px, height ~110–140px (desktop). Gap between tiles ~12–16px. Inner content un-skewed so no jagged text.

---

## 3) Animation (CSS keyframes, transform only)

- **Approach:** CSS only. No JS for the loop.
- **Mechanism:** `.hero-ticker__track-inner` contains two copies of the item set (duplicated in DOM). It has `display: flex; flex-wrap: nowrap` and `animation: ticker-scroll 40s linear infinite`. Keyframes move from `transform: translateX(0)` to `transform: translateX(-50%)` (so first copy moves out left, second copy follows; when animation resets it’s seamless).
- **Direction:** Always LTR for the ticker (scroll left→right means content moves left, i.e. `translateX(-50%)`). Force LTR on the ticker container: `direction: ltr` so RTL pages don’t flip the strip.
- **Performance:** Only `transform` is animated; no width/left/margin. Will-compositing: add `will-change: transform` on the animated element (or omit if we want to avoid overuse; transform is already composited).

---

## 4) Performance and safety

- **Repaint:** No. Only `transform: translateX` is animated → compositor-only.
- **No clip-path in animation:** Clip-path can trigger repaint; we use it only for static tile shape if needed, or rely on skew (transform).
- **Reduced motion:** Optional: `@media (prefers-reduced-motion: reduce)` { animation: none; } and show static strip.
- **Mobile:** Hide ticker with `display: none` below 768px, or show a single thin strip (smaller height, fewer tiles, no skew) to avoid heavy effects. Proposal: hide below 768px for Phase 1 to minimize risk.

---

## Summary

| Item | Decision |
|------|----------|
| Where | Inside `hero-section`, absolutely positioned at top, before `.hero-content` |
| DOM | Wrapper + left/right gradient masks + track + inner (flex) + tiles (skew outer, un-skew inner) + duplicated set |
| Shape | Skew (skewX) with inner un-skew; clip-path fallback only if needed |
| Animation | CSS keyframes, translateX(-50%), linear, infinite, ~40s |
| Direction | LTR forced on ticker |
| Non-interactive | pointer-events: none; no focus; role="presentation" aria-hidden="true" |
| Mobile | Hide below 768px (Phase 1) |
| Performance | Transform-only animation |

No refactor of existing hero content. Ticker is additive and fails safe (no API => hide or empty state).
