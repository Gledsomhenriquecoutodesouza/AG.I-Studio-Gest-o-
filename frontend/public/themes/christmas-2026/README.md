# Christmas 2026 dashboard preview

The premium Christmas palette, controls, cursor, and corner decorations are now available across the application. The greeting banner and its illustration remain on the **Dashboard**. The theme changes presentation only; business rules and financial calculations are unchanged.

## Assets

- `tree.svg` — layered evergreen with gold lights and ornaments.
- `snowman.svg` — soft shaded snowman illustration.
- `ornament-red.svg` and `ornament-gold.svg` — matching hanging baubles.
- `banner-sprig.svg` — winter greenery and a red bauble for the greeting banner.
- `star.svg` — gold gradient star with a restrained glow.
- `dashboard-preview.svg` — static dashboard preview with fictional figures.

## Integration points

The dashboard renders these assets from `/themes/christmas-2026/`. The shared visual rules live in `frontend/app/globals.css` under `.theme-christmas`; Dashboard-specific styles and the banner use `.christmas-dashboard`. Theme selection, intensity, and motion settings live in `frontend/app/page.tsx`.

The theme uses the CSS variable `--christmas-intensity` from `0` to `1`. The user can turn off animation with **Animações suaves**, and can deactivate the theme or choose another existing palette from the **Tema** menu. The browser's `prefers-reduced-motion` setting also disables the seasonal animations.

The winter decorations are `aria-hidden`, while the banner exposes a named region and readable text. Assets are vector SVGs to stay crisp on high-density displays without adding external image dependencies.

## Extending after review

The global palette can be extended screen-by-screen by refining the `.theme-christmas` rules. Keep the greeting banner scoped to Dashboard unless a separate seasonal message is designed for another workflow.
