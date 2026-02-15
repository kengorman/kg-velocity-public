// ====================================================================
// trip-movie.js — Consolidated cinematic zoom-out animation
// ====================================================================
// Renders a relativistic journey animation on a canvas element.
// Three rendering paths selected by category:
//   "Solar System"   — AU scale (Moon through Pluto)
//   "Milky Way"      — Light-year scale (Proxima Centauri through MW Center)
//   "Extragalactic"  — Intergalactic scale (to Andromeda)
//
// API:
//   window.tripMovie.start(canvasId, category, destinationName,
//                          destinationDistance, tripData, dotNetObjRef)
//   window.tripMovie.stop(canvasId)
// ====================================================================

window.tripMovie = (function () {
  'use strict';

  // Active animation instances keyed by canvasId
  const instances = {};

  // ================================================================
  // SHARED: Easing functions
  // ================================================================

  function easeInCubicOut(t) {
    if (t < 0.85) {
      const st = t / 0.85;
      return 0.85 * (st * st * st);
    }
    const st = (t - 0.85) / 0.15;
    return 0.85 + 0.15 * (1 - Math.pow(1 - st, 2));
  }

  // ================================================================
  // SHARED: Coordinate conversion (world space to screen pixels)
  // ================================================================

  function toScreen(state, wx, wy, parallax) {
    const dx = (wx - state.camX) * parallax;
    const dy = (wy - state.camY) * parallax;
    return {
      x: state.cssWidth / 2 + dx * state.camZoom,
      y: state.cssHeight / 2 - dy * state.camZoom,
    };
  }

  // ================================================================
  // SHARED: High-DPI canvas setup
  // ================================================================

  function resizeCanvas(state) {
    const dpr = window.devicePixelRatio || 1;
    const canvas = state.canvas;
    state.cssWidth = canvas.parentElement
      ? canvas.parentElement.clientWidth || window.innerWidth
      : window.innerWidth;
    state.cssHeight = canvas.parentElement
      ? canvas.parentElement.clientHeight || window.innerHeight
      : window.innerHeight;
    canvas.width = state.cssWidth * dpr;
    canvas.height = state.cssHeight * dpr;
    canvas.style.width = state.cssWidth + 'px';
    canvas.style.height = state.cssHeight + 'px';
    state.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  // ================================================================
  // SHARED: Star field generation
  // ================================================================

  function generateStarLayers(layerDefs, centerY) {
    const layers = [];
    for (const def of layerDefs) {
      const stars = [];
      for (let i = 0; i < def.count; i++) {
        stars.push({
          x: (Math.random() - 0.5) * def.spread,
          y: (Math.random() - 0.5) * def.spread + centerY,
          brightness: 0.2 + Math.random() * 0.8,
          size: 0.5 + Math.random() * (def.maxSize || 1.2),
          twinkleSpeed: 1 + Math.random() * 3,
          twinkleOffset: Math.random() * Math.PI * 2,
        });
      }
      layers.push({ stars, parallax: def.parallax });
    }
    return layers;
  }

  // ================================================================
  // SHARED: Star field rendering (parallax layers with twinkle)
  // ================================================================

  function drawStars(state, time, alphaBase, alphaRange) {
    const ctx = state.ctx;
    for (const layer of state.starLayers) {
      for (const star of layer.stars) {
        const pos = toScreen(state, star.x, star.y, layer.parallax);
        if (pos.x < -10 || pos.x > state.cssWidth + 10 ||
            pos.y < -10 || pos.y > state.cssHeight + 10) continue;
        const twinkle = 0.5 + 0.5 * Math.sin(time * star.twinkleSpeed + star.twinkleOffset);
        const alpha = star.brightness * (alphaBase + alphaRange * twinkle);
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, star.size, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(255, 255, 255, ${alpha})`;
        ctx.fill();
      }
    }
  }

  // ================================================================
  // SHARED: Ship trail (dashed line + moving dot)
  // ================================================================

  function drawTrail(state, progress, earthPos, destPos) {
    if (progress <= 0) return;
    const ctx = state.ctx;
    const e = easeInCubicOut(progress);
    const trailEnd = {
      x: earthPos.x + (destPos.x - earthPos.x) * e,
      y: earthPos.y + (destPos.y - earthPos.y) * e,
    };
    // Clamp ship to top of viewport
    trailEnd.y = Math.max(60, trailEnd.y);

    ctx.save();
    ctx.setLineDash([6, 6]);
    ctx.strokeStyle = 'rgba(126, 184, 255, 0.4)';
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.moveTo(earthPos.x, earthPos.y);
    ctx.lineTo(trailEnd.x, trailEnd.y);
    ctx.stroke();
    ctx.setLineDash([]);

    // Ship dot + glow (hidden near arrival)
    if (progress < 0.98) {
      ctx.beginPath();
      ctx.arc(trailEnd.x, trailEnd.y, 3, 0, Math.PI * 2);
      ctx.fillStyle = '#7eb8ff';
      ctx.fill();
      ctx.beginPath();
      ctx.arc(trailEnd.x, trailEnd.y, 8, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(126, 184, 255, 0.15)';
      ctx.fill();
    }
    ctx.restore();
  }

  // ================================================================
  // SHARED: Arrival arrow (triangle pointing at destination)
  // ================================================================

  function drawArrivalArrow(ctx, x, y, arrivalAmount) {
    if (arrivalAmount <= 0) return;
    const size = 5;
    const alpha = arrivalAmount * 0.9;
    ctx.save();
    ctx.fillStyle = `rgba(126, 184, 255, ${alpha})`;
    ctx.beginPath();
    ctx.moveTo(x + size, y);
    ctx.lineTo(x - size, y - size);
    ctx.lineTo(x - size, y + size);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }

  // ================================================================
  // SHARED: Trip data overlay panel (drawn on canvas)
  // ================================================================

  function drawTripDataPanel(state, progress) {
    if (progress <= 0.88) return;
    const ctx = state.ctx;
    const w = state.cssWidth;
    const h = state.cssHeight;
    const td = state.tripData;
    if (!td) return;

    const panelAlpha = Math.min(1, (progress - 0.88) / 0.08);

    // Panel dimensions
    const panelW = Math.min(340, w - 40);
    const panelH = 100;
    const panelX = (w - panelW) / 2;
    const panelY = h - panelH - 50; // 50px from bottom (safe area handled by Blazor)

    ctx.save();
    ctx.globalAlpha = panelAlpha;

    // Background
    ctx.fillStyle = 'rgba(0, 0, 0, 0.75)';
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.15)';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.roundRect(panelX, panelY, panelW, panelH, 12);
    ctx.fill();
    ctx.stroke();

    // Heading
    ctx.font = "300 13px 'Segoe UI', system-ui, sans-serif";
    ctx.fillStyle = '#fff';
    ctx.textAlign = 'center';
    ctx.letterSpacing = '2px';
    ctx.fillText(('Earth \u2192 ' + state.destinationName).toUpperCase(), w / 2, panelY + 22);
    ctx.letterSpacing = '0px';

    // Stats row
    const stats = [];
    if (td.shipTime) stats.push({ value: td.shipTime, label: 'SHIP TIME' });
    if (td.earthTime) stats.push({ value: td.earthTime, label: 'EARTH TIME' });
    if (td.distance) stats.push({ value: td.distance, label: 'DISTANCE' });
    if (td.speed) stats.push({ value: td.speed, label: 'SPEED' });

    if (stats.length > 0) {
      const totalGap = 24;
      const slotW = (panelW - 32) / stats.length;
      const baseX = panelX + 16;
      const valY = panelY + 52;
      const lblY = panelY + 68;

      for (let i = 0; i < stats.length; i++) {
        const cx = baseX + slotW * i + slotW / 2;

        // Value
        ctx.font = "600 17px 'Segoe UI', system-ui, sans-serif";
        ctx.fillStyle = '#7eb8ff';
        ctx.textAlign = 'center';
        ctx.fillText(stats[i].value, cx, valY);

        // Label
        ctx.font = "400 9px 'Segoe UI', system-ui, sans-serif";
        ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
        ctx.letterSpacing = '1px';
        ctx.fillText(stats[i].label, cx, lblY);
        ctx.letterSpacing = '0px';
      }
    }

    ctx.restore();
  }

  // ================================================================
  // SHARED: Log-zoom camera state computation
  // ================================================================

  function computeCameraState(state, progress, delayedPan) {
    const e = easeInCubicOut(progress);
    const logZoom = state.logZoomStart + (state.logZoomEnd - state.logZoomStart) * e;
    const zoom = Math.exp(logZoom);

    // Clamp pan so source never drops below viewport
    const bottomMargin = 60;
    const maxCY = state.sourceY + (state.cssHeight / 2 - bottomMargin) / zoom;

    // Extragalactic uses delayed vertical pan (e*e) so MW stays in frame
    const panE = delayedPan ? e * e : e;
    const cy = Math.min(
      state.hold.cy + (state.finalState.cy - state.hold.cy) * panE,
      maxCY
    );

    return {
      cx: state.hold.cx + (state.finalState.cx - state.hold.cx) * e,
      cy: cy,
      zoom: zoom,
    };
  }

  // ================================================================
  // SHARED: Solar system icon (used by Milky Way and Extragalactic)
  // ================================================================

  function drawSolarSystemIcon(state, pos) {
    const ctx = state.ctx;
    const r = Math.max(3, Math.min(60, state.camZoom * 0.008));

    const orbits = [0.35, 0.55, 0.75, 1.0];
    const planetColors = ['#c1440e', '#c88b3a', '#d4a94b', '#3f54ba'];
    const planetSizes = [0.8, 1.4, 1.2, 0.9];

    ctx.save();
    for (let i = 0; i < orbits.length; i++) {
      const orbitR = r * orbits[i] * 2.5;

      // Orbit ring
      ctx.beginPath();
      ctx.ellipse(pos.x, pos.y, orbitR, orbitR * 0.35, -0.3, 0, Math.PI * 2);
      ctx.strokeStyle = 'rgba(255,255,255,0.45)';
      ctx.lineWidth = 0.5;
      ctx.stroke();

      // Planet dot on ring
      const angle = i * 1.8 + 0.5;
      const px = pos.x + orbitR * Math.cos(angle);
      const py = pos.y + orbitR * 0.35 * Math.sin(angle - 0.3);
      const dotR = Math.max(0.8, planetSizes[i]);
      ctx.beginPath();
      ctx.arc(px, py, dotR, 0, Math.PI * 2);
      ctx.globalAlpha = 0.7;
      ctx.fillStyle = planetColors[i];
      ctx.fill();
      ctx.globalAlpha = 1;
    }

    // Sun glow
    const gradient = ctx.createRadialGradient(pos.x, pos.y, 0, pos.x, pos.y, r * 0.8);
    gradient.addColorStop(0, 'rgba(253, 184, 19, 0.5)');
    gradient.addColorStop(1, 'transparent');
    ctx.beginPath();
    ctx.arc(pos.x, pos.y, r * 0.8, 0, Math.PI * 2);
    ctx.fillStyle = gradient;
    ctx.fill();

    // Sun dot
    ctx.beginPath();
    ctx.arc(pos.x, pos.y, Math.max(1.5, r * 0.18), 0, Math.PI * 2);
    ctx.fillStyle = '#FDB813';
    ctx.fill();
    ctx.restore();

    return r; // return radius for label positioning
  }


  // ================================================================
  // ================================================================
  //  SOLAR SYSTEM RENDERER
  // ================================================================
  // ================================================================

  const solarSystem = {

    // All destinations in this category with AU positions
    // Moon nudged to 1.15 AU for visual separation from Earth
    DESTINATIONS: {
      'The Moon':  { au: 1.15 },
      'Mercury':   { au: 0.39 },
      'The Sun':   { au: 0 },
      'Mars':      { au: 1.52 },
      'Jupiter':   { au: 5.2 },
      'Saturn':    { au: 9.5 },
      'Pluto':     { au: 39.5 },
    },

    // Planet data for rendering
    planets: [
      { name: 'Sun',     au: 0,     baseR: 22, color: '#FDB813', glow: '#FDB81350', ring: false },
      { name: 'Mercury', au: 0.39,  baseR: 2,  color: '#8c7e6d', glow: null, ring: false },
      { name: 'Venus',   au: 0.72,  baseR: 4.8,color: '#c9a860', glow: null, ring: false },
      { name: 'Earth',   au: 1.0,   baseR: 5,  color: '#4a90d9', glow: '#4a90d940', ring: false },
      { name: 'Moon',    au: 1.15,  baseR: 1.2,color: '#aaa9a5', glow: null, ring: false },
      { name: 'Mars',    au: 1.52,  baseR: 3,  color: '#c1440e', glow: null, ring: false },
      { name: 'Jupiter', au: 5.2,   baseR: 14, color: '#c88b3a', glow: '#c88b3a20', ring: false },
      { name: 'Saturn',  au: 9.5,   baseR: 12, color: '#d4a94b', glow: '#d4a94b30', ring: true },
      { name: 'Uranus',  au: 19.2,  baseR: 6,  color: '#7ec8c8', glow: '#7ec8c820', ring: false },
      { name: 'Neptune', au: 30.1,  baseR: 5.8,color: '#3f54ba', glow: '#3f54ba20', ring: false },
      { name: 'Pluto',   au: 39.5,  baseR: 1.2,color: '#c4b59a', glow: null, ring: false },
    ],

    init(state) {
      // Resolve destination AU
      const destEntry = this.DESTINATIONS[state.destinationName];
      state.destAU = destEntry ? destEntry.au : 9.5; // fallback to Saturn

      // Generate star layers (AU-scale spread)
      state.starLayers = [];
      for (let layer = 0; layer < 3; layer++) {
        const stars = [];
        const count = 300 + layer * 200; // 300 / 500 / 700
        for (let i = 0; i < count; i++) {
          stars.push({
            x: (Math.random() - 0.5) * 120,
            y: (Math.random() - 0.5) * 120,
            brightness: 0.2 + Math.random() * 0.8,
            size: 0.5 + Math.random() * (1.5 - layer * 0.3),
            twinkleSpeed: 1 + Math.random() * 3,
            twinkleOffset: Math.random() * Math.PI * 2,
          });
        }
        state.starLayers.push({ stars, parallax: 0.3 + layer * 0.35 }); // 0.3 / 0.65 / 1.0
      }

      // Source is Earth at 1.0 AU
      state.sourceY = 1.0;
    },

    computeFraming(state) {
      const screenH = state.cssHeight || 800;
      const PANEL_MARGIN = 200;
      const TOP_MARGIN = 40;
      const usableH = screenH - PANEL_MARGIN - TOP_MARGIN;

      const maxAU = Math.max(state.destAU, 1.5);
      const totalSpan = maxAU; // minAU = 0 (Sun)
      const finalZoom = Math.max(2, Math.min(200, usableH / Math.max(totalSpan, 0.5)));

      const camCY = (screenH / 2 - PANEL_MARGIN) / finalZoom;

      state.hold = { cx: 0, cy: 1.0, zoom: 600 };
      state.finalState = { cx: 0, cy: camCY, zoom: finalZoom };
      state.logZoomStart = Math.log(state.hold.zoom);
      state.logZoomEnd = Math.log(state.finalState.zoom);
    },

    frame(state, progress, time) {
      const ctx = state.ctx;

      // Camera
      const cam = computeCameraState(state, progress, false);
      state.camX = cam.cx;
      state.camY = cam.cy;
      state.camZoom = cam.zoom;

      // Clear
      ctx.fillStyle = '#030308';
      ctx.fillRect(0, 0, state.cssWidth, state.cssHeight);

      // Arrival highlight
      const arrivalAmount = Math.min(1, Math.max(0, (progress - 0.75) / 0.15));

      // Stars
      drawStars(state, time, 0.4, 0.6);

      // Orbit hints
      ctx.save();
      ctx.strokeStyle = 'rgba(255,255,255,0.04)';
      ctx.lineWidth = 0.5;
      for (const p of solarSystem.planets) {
        if (p.au === 0) continue;
        const center = toScreen(state, 0, 0, 1.0);
        const r = p.au * state.camZoom;
        if (r > 5 && r < state.cssHeight * 3) {
          ctx.beginPath();
          ctx.arc(center.x, center.y, r, 0, Math.PI * 2);
          ctx.stroke();
        }
      }
      ctx.restore();

      // Trail
      const earthPos = toScreen(state, 0, 1.0, 1.0);
      const destPos = toScreen(state, 0, state.destAU, 1.0);
      drawTrail(state, progress, earthPos, destPos);

      // Planets
      for (const planet of solarSystem.planets) {
        solarSystem.drawPlanet(state, planet, arrivalAmount);
      }

      // Trip data panel
      drawTripDataPanel(state, progress);
    },

    drawPlanet(state, planet, arrivalAmount) {
      const ctx = state.ctx;
      const pos = toScreen(state, 0, planet.au, 1.0);
      const r = planet.baseR * (state.camZoom / 60);
      const minR = planet.name === 'Sun' ? 4 : 1.5;
      const drawR = Math.max(minR, Math.min(r, planet.name === 'Sun' ? 60 : 30));

      if (pos.x < -100 || pos.x > state.cssWidth + 100 ||
          pos.y < -100 || pos.y > state.cssHeight + 100) return;

      // Is this the destination?
      const isDestPlanet = planet.name === state.destinationName ||
        (state.destinationName === 'The Moon' && planet.name === 'Moon') ||
        (state.destinationName === 'The Sun' && planet.name === 'Sun');

      // Arrival arrow
      if (isDestPlanet && arrivalAmount > 0) {
        drawArrivalArrow(ctx, pos.x - drawR - 14, pos.y, arrivalAmount);
      }

      // Glow
      if (planet.glow && drawR > 3) {
        const gradient = ctx.createRadialGradient(pos.x, pos.y, drawR, pos.x, pos.y, drawR * 3);
        gradient.addColorStop(0, planet.glow);
        gradient.addColorStop(1, 'transparent');
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, drawR * 3, 0, Math.PI * 2);
        ctx.fillStyle = gradient;
        ctx.fill();
      }

      // Body
      ctx.beginPath();
      ctx.arc(pos.x, pos.y, drawR, 0, Math.PI * 2);
      ctx.fillStyle = planet.color;
      ctx.fill();

      // Saturn rings
      if (planet.ring && drawR > 3) {
        ctx.save();
        ctx.beginPath();
        ctx.ellipse(pos.x, pos.y, drawR * 2.2, drawR * 0.5, -0.2, 0, Math.PI * 2);
        ctx.strokeStyle = 'rgba(210, 180, 120, 0.6)';
        ctx.lineWidth = Math.max(1, drawR * 0.25);
        ctx.stroke();
        ctx.beginPath();
        ctx.ellipse(pos.x, pos.y, drawR * 1.7, drawR * 0.38, -0.2, 0, Math.PI * 2);
        ctx.strokeStyle = 'rgba(190, 160, 100, 0.35)';
        ctx.lineWidth = Math.max(0.5, drawR * 0.15);
        ctx.stroke();
        ctx.restore();
      }

      // Label
      if (drawR > 2.5 || planet.name === 'Earth' || isDestPlanet) {
        const arrived = isDestPlanet && arrivalAmount > 0;
        const fontSize = arrived ? 18 : Math.max(14, Math.min(16, drawR * 1.2));
        ctx.font = `${arrived ? '600' : '400'} ${fontSize}px 'Segoe UI', system-ui, sans-serif`;
        ctx.fillStyle = arrived ? `rgba(126, 184, 255, ${0.5 + arrivalAmount * 0.5})` :
                        isDestPlanet ? 'rgba(126, 184, 255, 0.8)' : 'rgba(255,255,255,0.6)';
        ctx.textAlign = 'left';
        ctx.fillText(planet.name, pos.x + drawR + 8, pos.y + 4);
      }
    },
  };


  // ================================================================
  // ================================================================
  //  MILKY WAY RENDERER
  // ================================================================
  // ================================================================

  const milkyWay = {

    // Log-scale positioning constants
    LOG_MIN: Math.log10(1),      // 1 ly
    LOG_MAX: Math.log10(30000),  // just above MW center

    DESTINATIONS: {
      'Proxima Centauri':    { ly: 4.24 },
      'Polaris':             { ly: 433 },
      'Betelgeuse':          { ly: 700 },
      'Horsehead Nebula':    { ly: 1500 },
      'Crab Nebula':         { ly: 6500 },
      'Pillars of Creation': { ly: 6500 },
      'Milky Way Center':    { ly: 26000 },
    },

    // Galactic landmarks
    galacticObjects: [
      { name: 'Solar System',        ly: 0,     baseR: 5,  color: '#4a90d9', glow: '#4a90d940' },
      { name: 'Proxima Centauri',    ly: 4.24,  baseR: 4,  color: '#ff6b4a', glow: '#ff6b4a30' },
      { name: 'Polaris',             ly: 433,   baseR: 5,  color: '#fffbe0', glow: '#fffbe040' },
      { name: 'Betelgeuse',          ly: 700,   baseR: 7,  color: '#ff4500', glow: '#ff450040' },
      { name: 'Horsehead Nebula',    ly: 1500,  baseR: 6,  color: '#8b4585', glow: '#8b458540' },
      { name: 'Crab Nebula',         ly: 6500,  baseR: 6,  color: '#4ecdc4', glow: '#4ecdc430' },
      { name: 'Pillars of Creation', ly: 6500,  baseR: 6,  color: '#c79a3e', glow: '#c79a3e30' },
      { name: 'Milky Way Center',    ly: 26000, baseR: 8,  color: '#ffd700', glow: '#ffd70050' },
    ],

    lyToY(ly) {
      if (ly <= 0) return 0;
      return (Math.log10(ly) - this.LOG_MIN) / (this.LOG_MAX - this.LOG_MIN);
    },

    init(state) {
      // Resolve destination
      const destEntry = this.DESTINATIONS[state.destinationName];
      const destLY = destEntry ? destEntry.ly : 6500;
      state.destY = this.lyToY(destLY);

      // Prepare galactic objects with Y positions
      const xOffsets = [0, 0.01, -0.01, 0.015, -0.015, 0.01, -0.01, 0];
      state.galacticObjects = this.galacticObjects.map((obj, i) => {
        const result = { ...obj };
        result.y = this.lyToY(obj.ly);
        result.x = xOffsets[i] || 0;
        return result;
      });
      // Nudge Pillars of Creation apart from Crab Nebula
      const pillars = state.galacticObjects.find(o => o.name === 'Pillars of Creation');
      if (pillars) pillars.y += 0.02;

      // Star layers (normalized-space spread)
      state.starLayers = generateStarLayers([
        { count: 500, spread: 10, parallax: 0.1 },
        { count: 350, spread: 6,  parallax: 0.2 },
        { count: 200, spread: 3,  parallax: 0.4 },
      ], 0.5);

      // Source is Solar System at y=0
      state.sourceY = 0;
    },

    computeFraming(state) {
      const screenH = state.cssHeight || 800;
      const midpoint = state.destY / 2;
      const finalZoom = Math.max(screenH * 0.4, (screenH * 0.55) / Math.max(state.destY, 0.15));

      state.hold = { cx: 0, cy: 0.0, zoom: screenH * 12 };
      state.finalState = { cx: 0, cy: midpoint, zoom: finalZoom };
      state.logZoomStart = Math.log(state.hold.zoom);
      state.logZoomEnd = Math.log(state.finalState.zoom);
    },

    frame(state, progress, time) {
      const ctx = state.ctx;

      // Camera
      const cam = computeCameraState(state, progress, false);
      state.camX = cam.cx;
      state.camY = cam.cy;
      state.camZoom = cam.zoom;

      // Clear
      ctx.fillStyle = '#030308';
      ctx.fillRect(0, 0, state.cssWidth, state.cssHeight);

      // Arrival highlight
      const arrivalAmount = Math.min(1, Math.max(0, (progress - 0.75) / 0.15));

      // Stars
      drawStars(state, time, 0.3, 0.7);

      // Trail
      const earthPos = toScreen(state, 0, 0, 1.0);
      const destObj = state.galacticObjects.find(o => o.name === state.destinationName);
      const destPos = toScreen(state, destObj ? destObj.x : 0, state.destY, 1.0);
      drawTrail(state, progress, earthPos, destPos);

      // Galactic objects
      milkyWay.drawGalacticObjects(state, arrivalAmount);

      // Trip data panel
      drawTripDataPanel(state, progress);
    },

    drawGalacticObjects(state, arrivalAmount) {
      const ctx = state.ctx;

      for (const obj of state.galacticObjects) {
        const pos = toScreen(state, obj.x, obj.y, 1.0);

        if (pos.x < -100 || pos.x > state.cssWidth + 100 ||
            pos.y < -100 || pos.y > state.cssHeight + 100) continue;

        const zoomFactor = state.camZoom / (state.cssHeight * 2);
        const drawR = Math.max(2, Math.min(obj.baseR * Math.sqrt(zoomFactor), 20));

        const isDest = obj.name === state.destinationName;
        const isEarth = obj.name === 'Solar System';

        // Arrival arrow
        if (isDest && arrivalAmount > 0) {
          drawArrivalArrow(ctx, pos.x - drawR - 14, pos.y, arrivalAmount);
        }

        // Solar System gets icon rendering
        if (isEarth) {
          const iconR = drawSolarSystemIcon(state, pos);
          // Label for Solar System
          if (iconR > 1.5) {
            const labelR = iconR * 2.5;
            const fontSize = Math.max(14, Math.min(16, iconR * 0.4));
            ctx.font = `400 ${fontSize}px 'Segoe UI', system-ui, sans-serif`;
            ctx.fillStyle = 'rgba(255,255,255,0.55)';
            ctx.textAlign = 'left';
            ctx.fillText('Solar System', pos.x + labelR + 8, pos.y + 4);
          }
        } else {
          // Glow
          if (obj.glow && drawR > 2.5) {
            const gradient = ctx.createRadialGradient(pos.x, pos.y, drawR, pos.x, pos.y, drawR * 3);
            gradient.addColorStop(0, obj.glow);
            gradient.addColorStop(1, 'transparent');
            ctx.beginPath();
            ctx.arc(pos.x, pos.y, drawR * 3, 0, Math.PI * 2);
            ctx.fillStyle = gradient;
            ctx.fill();
          }

          // Body
          ctx.beginPath();
          ctx.arc(pos.x, pos.y, drawR, 0, Math.PI * 2);
          ctx.fillStyle = obj.color;
          ctx.fill();
        }

        // Label (non-Solar System, or Solar System handled above)
        if (!isEarth && (drawR > 1.5 || isDest)) {
          const arrived = isDest && arrivalAmount > 0;
          const fontSize = arrived ? 18 : Math.max(14, Math.min(16, drawR * 1.3));
          ctx.font = `${arrived ? '600' : '400'} ${fontSize}px 'Segoe UI', system-ui, sans-serif`;
          ctx.fillStyle = arrived ? `rgba(126, 184, 255, ${0.5 + arrivalAmount * 0.5})` :
                          isDest ? 'rgba(126, 184, 255, 0.9)' : 'rgba(255,255,255,0.55)';
          if (obj.x >= 0) {
            ctx.textAlign = 'left';
            ctx.fillText(obj.name, pos.x + drawR + 8, pos.y + 4);
          } else {
            ctx.textAlign = 'right';
            ctx.fillText(obj.name, pos.x - drawR - 8, pos.y + 4);
          }
        }
      }
    },
  };


  // ================================================================
  // ================================================================
  //  EXTRAGALACTIC RENDERER
  // ================================================================
  // ================================================================

  const extragalactic = {

    EARTH_Y: 0.2,

    init(state) {
      const EARTH_Y = this.EARTH_Y;
      state.earthY = EARTH_Y;

      // Milky Way center positioned so Earth sits in an outer arm
      const milkyWayPos = { x: 0, y: EARTH_Y + 0.02 };
      state.milkyWayPos = milkyWayPos;

      // Andromeda position
      state.andromedaPos = { x: 0.03, y: 0.9 };

      // Star layers
      state.starLayers = generateStarLayers([
        { count: 500, spread: 10, parallax: 0.1 },
        { count: 350, spread: 6,  parallax: 0.2 },
        { count: 200, spread: 3,  parallax: 0.4 },
      ], 0.45); // centered on mid-journey

      // Milky Way spiral stars
      state.spiralStars = [];
      const ARM_COUNT = 4;
      const STARS_PER_ARM = 300;
      for (let arm = 0; arm < ARM_COUNT; arm++) {
        const armOffset = (arm / ARM_COUNT) * Math.PI * 2;
        for (let i = 0; i < STARS_PER_ARM; i++) {
          const t = i / STARS_PER_ARM;
          const angle = armOffset + t * Math.PI * 2.5;
          const radius = 0.005 + t * 0.1;
          const spread = 0.004 + t * 0.012;
          const r = radius + (Math.random() - 0.5) * spread;
          const a = angle + (Math.random() - 0.5) * 0.4;
          state.spiralStars.push({
            x: milkyWayPos.x + r * Math.cos(a),
            y: milkyWayPos.y + r * Math.sin(a) * 0.4,
            brightness: 0.3 + Math.random() * 0.7,
            size: 0.3 + Math.random() * 1.2,
            twinkleSpeed: 0.3 + Math.random() * 1,
            twinkleOffset: Math.random() * Math.PI * 2,
          });
        }
      }
      state.coreGlow = { x: milkyWayPos.x, y: milkyWayPos.y, radius: 0.025 };

      // Andromeda spiral stars
      state.andromedaStars = [];
      for (let arm = 0; arm < 3; arm++) {
        const armOffset = (arm / 3) * Math.PI * 2;
        for (let i = 0; i < 150; i++) {
          const t = i / 150;
          const angle = armOffset + t * Math.PI * 2;
          const radius = 0.002 + t * 0.03;
          const spread = 0.002 + t * 0.005;
          const r = radius + (Math.random() - 0.5) * spread;
          const a = angle + (Math.random() - 0.5) * 0.5;
          state.andromedaStars.push({
            x: state.andromedaPos.x + r * Math.cos(a),
            y: state.andromedaPos.y + r * Math.sin(a) * 0.5,
            brightness: 0.2 + Math.random() * 0.6,
            size: 0.3 + Math.random() * 0.8,
            twinkleSpeed: 0.3 + Math.random() * 1,
            twinkleOffset: Math.random() * Math.PI * 2,
          });
        }
      }

      // Source is Solar System at EARTH_Y
      state.sourceY = EARTH_Y;
    },

    computeFraming(state) {
      const screenH = state.cssHeight || 800;
      state.hold = { cx: 0, cy: state.earthY, zoom: screenH * 12 };
      state.finalState = { cx: 0.015, cy: 0.55, zoom: screenH * 0.7 };
      state.logZoomStart = Math.log(state.hold.zoom);
      state.logZoomEnd = Math.log(state.finalState.zoom);
    },

    frame(state, progress, time) {
      const ctx = state.ctx;

      // Camera (with delayed vertical pan for MW framing)
      const cam = computeCameraState(state, progress, true);
      state.camX = cam.cx;
      state.camY = cam.cy;
      state.camZoom = cam.zoom;

      // Clear
      ctx.fillStyle = '#030308';
      ctx.fillRect(0, 0, state.cssWidth, state.cssHeight);

      // Arrival highlight
      const arrivalAmount = Math.min(1, Math.max(0, (progress - 0.75) / 0.15));

      // Background stars
      drawStars(state, time, 0.3, 0.7);

      // Milky Way
      extragalactic.drawMilkyWay(state, time, progress);

      // Andromeda
      extragalactic.drawAndromeda(state, time, progress, arrivalAmount);

      // Trail
      const earthPos = toScreen(state, 0, state.earthY, 1.0);
      const destPos = toScreen(state, state.andromedaPos.x, state.andromedaPos.y, 1.0);
      drawTrail(state, progress, earthPos, destPos);

      // Solar system icon
      const ssPos = toScreen(state, 0, state.earthY, 1.0);
      const iconR = drawSolarSystemIcon(state, ssPos);
      // Label when big enough
      if (iconR > 8) {
        const labelR = iconR * 2.5;
        ctx.font = `400 ${Math.max(14, Math.min(16, iconR * 0.4))}px 'Segoe UI', system-ui, sans-serif`;
        ctx.fillStyle = 'rgba(255,255,255,0.55)';
        ctx.textAlign = 'left';
        ctx.fillText('Solar System', ssPos.x + labelR + 8, ssPos.y + 4);
      }

      // Trip data panel
      drawTripDataPanel(state, progress);
    },

    drawMilkyWay(state, time, progress) {
      const ctx = state.ctx;
      const mwAlpha = Math.min(1, Math.max(0, (progress - 0.08) / 0.20));
      if (mwAlpha <= 0) return;

      // Core glow
      const corePos = toScreen(state, state.coreGlow.x, state.coreGlow.y, 1.0);
      const coreR = state.coreGlow.radius * state.camZoom;
      if (coreR > 2 && coreR < state.cssWidth * 2) {
        const drawR = Math.min(coreR, 150);
        const gradient = ctx.createRadialGradient(corePos.x, corePos.y, 0, corePos.x, corePos.y, drawR);
        gradient.addColorStop(0, `rgba(255, 240, 200, ${mwAlpha * 0.4})`);
        gradient.addColorStop(0.3, `rgba(255, 220, 150, ${mwAlpha * 0.15})`);
        gradient.addColorStop(1, 'transparent');
        ctx.beginPath();
        ctx.arc(corePos.x, corePos.y, drawR, 0, Math.PI * 2);
        ctx.fillStyle = gradient;
        ctx.fill();
      }

      // Spiral arm stars
      for (const star of state.spiralStars) {
        const pos = toScreen(state, star.x, star.y, 1.0);
        if (pos.x < -5 || pos.x > state.cssWidth + 5 ||
            pos.y < -5 || pos.y > state.cssHeight + 5) continue;
        const twinkle = 0.5 + 0.5 * Math.sin(time * star.twinkleSpeed + star.twinkleOffset);
        const alpha = mwAlpha * star.brightness * (0.3 + 0.7 * twinkle);
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, star.size, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(230, 220, 200, ${alpha})`;
        ctx.fill();
      }

      // Label
      if (mwAlpha > 0.5) {
        const labelPos = toScreen(state, state.milkyWayPos.x + 0.12, state.milkyWayPos.y, 1.0);
        if (labelPos.y > 0 && labelPos.y < state.cssHeight) {
          ctx.font = '14px "Segoe UI", system-ui, sans-serif';
          ctx.fillStyle = `rgba(255,255,255,${mwAlpha * 0.5})`;
          ctx.textAlign = 'left';
          ctx.fillText('Milky Way', labelPos.x, labelPos.y);
        }
      }
    },

    drawAndromeda(state, time, progress, arrivalAmount) {
      const ctx = state.ctx;
      const andAlpha = Math.min(1, Math.max(0, (progress - 0.45) / 0.25));
      if (andAlpha <= 0) return;

      const corePos = toScreen(state, state.andromedaPos.x, state.andromedaPos.y, 1.0);
      const coreR = 0.01 * state.camZoom;

      // Arrival arrow
      if (arrivalAmount > 0) {
        drawArrivalArrow(ctx, corePos.x - 20, corePos.y, arrivalAmount);
      }

      // Core glow
      if (coreR > 1 && coreR < state.cssWidth) {
        const drawR = Math.min(coreR, 100);
        const gradient = ctx.createRadialGradient(corePos.x, corePos.y, 0, corePos.x, corePos.y, drawR);
        gradient.addColorStop(0, `rgba(200, 210, 255, ${andAlpha * 0.35})`);
        gradient.addColorStop(0.4, `rgba(180, 190, 230, ${andAlpha * 0.1})`);
        gradient.addColorStop(1, 'transparent');
        ctx.beginPath();
        ctx.arc(corePos.x, corePos.y, drawR, 0, Math.PI * 2);
        ctx.fillStyle = gradient;
        ctx.fill();
      }

      // Spiral stars
      for (const star of state.andromedaStars) {
        const pos = toScreen(state, star.x, star.y, 1.0);
        if (pos.x < -5 || pos.x > state.cssWidth + 5 ||
            pos.y < -5 || pos.y > state.cssHeight + 5) continue;
        const twinkle = 0.5 + 0.5 * Math.sin(time * star.twinkleSpeed + star.twinkleOffset);
        const alpha = andAlpha * star.brightness * (0.3 + 0.7 * twinkle);
        ctx.beginPath();
        ctx.arc(pos.x, pos.y, star.size, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(200, 210, 255, ${alpha})`;
        ctx.fill();
      }

      // Label
      if (andAlpha > 0.3) {
        const labelPos = toScreen(state, state.andromedaPos.x, state.andromedaPos.y - 0.06, 1.0);
        if (labelPos.y > 0 && labelPos.y < state.cssHeight) {
          const arrived = arrivalAmount > 0;
          const fontSize = arrived ? 18 : 14;
          ctx.font = `${arrived ? '600' : '400'} ${fontSize}px "Segoe UI", system-ui, sans-serif`;
          ctx.fillStyle = arrived ? `rgba(126, 184, 255, ${0.5 + arrivalAmount * 0.5})` :
                          `rgba(255,255,255,${andAlpha * 0.5})`;
          ctx.textAlign = 'center';
          ctx.fillText('Andromeda Galaxy', labelPos.x, labelPos.y);
        }
      }
    },
  };


  // ================================================================
  // RENDERER SELECTION
  // ================================================================

  function getRenderer(category) {
    switch (category) {
      case 'Solar System':   return solarSystem;
      case 'Milky Way':      return milkyWay;
      case 'Extragalactic':  return extragalactic;
      default:               return solarSystem;
    }
  }


  // ================================================================
  // PUBLIC API
  // ================================================================

  function start(canvasId, category, destinationName, destinationDistance, tripData, dotNetObjRef) {
    // Stop any existing animation on this canvas
    stop(canvasId);

    const canvas = document.getElementById(canvasId);
    if (!canvas) {
      console.error('[trip-movie] Canvas not found:', canvasId);
      return;
    }
    const ctx = canvas.getContext('2d');

    // Animation state object — passed to all functions instead of globals
    const state = {
      canvas,
      ctx,
      canvasId,
      category,
      destinationName,
      destinationDistance,
      tripData,
      dotNetObjRef,
      duration: 6000,

      // Camera (updated per frame)
      camX: 0,
      camY: 0,
      camZoom: 1,
      cssWidth: 0,
      cssHeight: 0,

      // Framing (computed once, recomputed on resize)
      hold: null,
      finalState: null,
      logZoomStart: 0,
      logZoomEnd: 0,
      sourceY: 0,

      // Star layers (populated by renderer.init)
      starLayers: [],

      // Animation loop
      startTime: null,
      animId: null,
      completeFired: false,
    };

    // Extragalactic uses slightly longer duration
    if (category === 'Extragalactic') {
      state.duration = 7000;
    }

    const renderer = getRenderer(category);

    // Resize handler
    state.resizeHandler = function () {
      resizeCanvas(state);
      renderer.computeFraming(state);
    };
    window.addEventListener('resize', state.resizeHandler);

    // Initial size + framing
    resizeCanvas(state);
    renderer.init(state);
    renderer.computeFraming(state);

    // Animation loop
    function frame(timestamp) {
      if (!state.animId) return; // stopped

      if (!state.startTime) state.startTime = timestamp;
      const elapsed = timestamp - state.startTime;
      const progress = Math.min(1, elapsed / state.duration);
      const time = timestamp / 1000;

      renderer.frame(state, progress, time);

      // Fire completion callback exactly once
      if (progress >= 1 && !state.completeFired) {
        state.completeFired = true;
        if (dotNetObjRef) {
          try {
            dotNetObjRef.invokeMethodAsync('OnMovieComplete');
          } catch (e) {
            console.warn('[trip-movie] OnMovieComplete callback failed:', e);
          }
        }
      }

      // Keep rendering after completion (stars twinkle)
      state.animId = requestAnimationFrame(frame);
    }

    state.animId = requestAnimationFrame(frame);
    instances[canvasId] = state;
  }

  function stop(canvasId) {
    const state = instances[canvasId];
    if (!state) return;

    if (state.animId) {
      cancelAnimationFrame(state.animId);
      state.animId = null;
    }

    if (state.resizeHandler) {
      window.removeEventListener('resize', state.resizeHandler);
      state.resizeHandler = null;
    }

    // Clear canvas
    if (state.canvas && state.ctx) {
      const dpr = window.devicePixelRatio || 1;
      state.ctx.setTransform(1, 0, 0, 1, 0, 0);
      state.ctx.clearRect(0, 0, state.canvas.width, state.canvas.height);
    }

    // Null references for GC
    state.canvas = null;
    state.ctx = null;
    state.dotNetObjRef = null;
    state.starLayers = null;
    state.spiralStars = null;
    state.andromedaStars = null;
    state.galacticObjects = null;

    delete instances[canvasId];
  }

  return { start, stop };
})();
