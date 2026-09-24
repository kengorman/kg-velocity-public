import EmblaCarousel from 'embla-carousel';

// Store carousel instances by container ID
const carousels = new Map();

/**
 * Initialize pagination dots for a carousel
 * @param {string} containerId - The carousel container ID
 * @param {object} embla - The Embla carousel instance
 */
function initDots(containerId, embla) {
  const dotsContainer = document.getElementById(`${containerId}-dots`);
  if (!dotsContainer) return;

  const slides = embla.slideNodes();

  // Clear existing dots
  dotsContainer.innerHTML = '';

  // Create dots
  slides.forEach((_, index) => {
    const dot = document.createElement('button');
    dot.className = 'embla__dot';
    dot.type = 'button';
    dot.setAttribute('aria-label', `Go to slide ${index + 1}`);
    dot.addEventListener('click', () => embla.scrollTo(index));
    dotsContainer.appendChild(dot);
  });

  // Update active dot
  const updateDots = () => {
    const selectedIndex = embla.selectedScrollSnap();
    const dots = dotsContainer.querySelectorAll('.embla__dot');
    dots.forEach((dot, index) => {
      dot.classList.toggle('active', index === selectedIndex);
    });
  };

  // Initial update
  updateDots();

  // Update on select
  embla.on('select', updateDots);
}

/**
 * Initialize an Embla carousel on a container
 * @param {string} containerId - The ID of the carousel container element
 * @param {object} dotNetRef - Blazor .NET object reference for callbacks
 * @param {object} options - Carousel options
 */
export function initCarousel(containerId, dotNetRef, options = {}) {
  const container = document.getElementById(containerId);
  if (!container) {
    console.error(`Embla: Container #${containerId} not found`);
    return;
  }

  // Destroy existing instance if re-initializing
  if (carousels.has(containerId)) {
    carousels.get(containerId).destroy();
  }

  const defaultOptions = {
    align: 'center',
    containScroll: 'trimSnaps',
    dragFree: false,  // Disable for reliable snapping
    loop: false,
    skipSnaps: true,  // Fast swipes still skip multiple cards
    duration: 20,     // Responsive settle
    ...options
  };

  const embla = EmblaCarousel(container, defaultOptions);

  // Check if scale effect should be applied (default true, disable with noScale option)
  const shouldScale = !options.noScale;

  // Apply scale effect on init and scroll (for picker carousels)
  const applyScaleEffect = () => {
    if (!shouldScale) return;

    const slides = embla.slideNodes();
    const scrollProgress = embla.scrollProgress();
    const slidesInView = embla.slidesInView();
    const selectedIndex = embla.selectedScrollSnap();

    slides.forEach((slide, index) => {
      const isSelected = index === selectedIndex;
      const isInView = slidesInView.includes(index);

      // Calculate distance from center for smooth scaling
      const slideProgress = embla.scrollSnapList()[index];
      const distance = Math.abs(scrollProgress - slideProgress);

      // Scale: 1.0 for selected, scaling down based on distance
      // More pronounced effect to clearly show Embla is working
      const scale = isSelected ? 1.08 : Math.max(0.85, 1 - distance * 0.25);
      const opacity = isSelected ? 1 : Math.max(0.6, 1 - distance * 0.6);

      slide.style.transform = `scale(${scale})`;
      slide.style.opacity = opacity;
      slide.style.transition = 'transform 0.2s ease-out, opacity 0.2s ease-out';
    });
  };

  // Event: selection changed
  embla.on('select', () => {
    const index = embla.selectedScrollSnap();
    if (shouldScale) applyScaleEffect();

    // Notify Blazor of selection change
    if (dotNetRef) {
      dotNetRef.invokeMethodAsync('OnCarouselSelect', containerId, index);
    }
  });

  // Event: scroll in progress (for smooth scale during drag)
  if (shouldScale) {
    embla.on('scroll', () => {
      applyScaleEffect();
    });
  }

  // Initial scale application
  if (shouldScale) applyScaleEffect();

  // Initialize pagination dots
  initDots(containerId, embla);

  // Store instance
  carousels.set(containerId, embla);
}

/**
 * Scroll to the next slide
 * @param {string} containerId - The carousel container ID
 */
export function scrollNext(containerId) {
  const embla = carousels.get(containerId);
  if (embla) {
    embla.scrollNext();
  }
}

/**
 * Scroll to the previous slide
 * @param {string} containerId - The carousel container ID
 */
export function scrollPrev(containerId) {
  const embla = carousels.get(containerId);
  if (embla) {
    embla.scrollPrev();
  }
}

/**
 * Scroll to a specific slide by index
 * @param {string} containerId - The carousel container ID
 * @param {number} index - The slide index to scroll to
 */
export function scrollTo(containerId, index) {
  const embla = carousels.get(containerId);
  if (embla) {
    embla.scrollTo(index);
  }
}

/**
 * Get the currently selected slide index
 * @param {string} containerId - The carousel container ID
 * @returns {number} The selected slide index
 */
export function getSelectedIndex(containerId) {
  const embla = carousels.get(containerId);
  return embla ? embla.selectedScrollSnap() : 0;
}

/**
 * Auto-scroll through a sequence of slides to show off how many there are.
 * Cancels as soon as the user touches the carousel or its dots.
 * @param {string} containerId - The carousel container ID
 * @param {number[]} sequence - Slide indexes to visit in order; the last one is where it settles
 * @param {object} options - { dwellMs, finalDwellMs, duration }
 * @returns {Promise<boolean>} true if the tour completed, false if cancelled
 */
export function tour(containerId, sequence, options = {}) {
  const embla = carousels.get(containerId);
  if (!embla || sequence.length === 0) return Promise.resolve(false);

  const { dwellMs = 400, finalDwellMs = dwellMs, duration } = options;
  const finalIndex = sequence[sequence.length - 1];

  // Respect the OS "reduce motion" setting: skip the fly-through, just land on the final slide
  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    embla.scrollTo(finalIndex);
    return Promise.resolve(true);
  }

  // Temporarily override scroll duration. scrollTo() reads this from the engine's
  // options object on each call, so mutating it avoids a reInit.
  const engineOptions = embla.internalEngine().options;
  const baseDuration = engineOptions.duration;
  if (duration) engineOptions.duration = duration;

  const dotsContainer = document.getElementById(`${containerId}-dots`);

  return new Promise(resolve => {
    let step = 0;
    let timer = null;

    const finish = (completed) => {
      clearTimeout(timer);
      engineOptions.duration = baseDuration;
      embla.off('pointerDown', cancel);
      dotsContainer?.removeEventListener('pointerdown', cancel);
      resolve(completed);
    };
    const cancel = () => finish(false);

    const next = () => {
      // Carousel was destroyed or replaced (e.g. a new trip started)
      if (carousels.get(containerId) !== embla) return finish(false);

      embla.scrollTo(sequence[step]);
      step++;
      if (step >= sequence.length) return finish(true);

      // Linger a bit longer on the last panel before rewinding to the final slide
      const isFinalHold = step === sequence.length - 1;
      timer = setTimeout(next, isFinalHold ? finalDwellMs : dwellMs);
    };

    embla.on('pointerDown', cancel);
    dotsContainer?.addEventListener('pointerdown', cancel);
    next();
  });
}

/**
 * Smoothly scroll the page so the carousel's top edge is in view
 * @param {string} containerId - The carousel container ID
 */
export function scrollIntoView(containerId) {
  document.getElementById(containerId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

/**
 * Destroy a carousel instance
 * @param {string} containerId - The carousel container ID
 */
export function destroyCarousel(containerId) {
  const embla = carousels.get(containerId);
  if (embla) {
    embla.destroy();
    carousels.delete(containerId);
  }
}

// Expose functions globally for Blazor interop
window.emblaCarousel = {
  initCarousel,
  scrollNext,
  scrollPrev,
  scrollTo,
  getSelectedIndex,
  tour,
  scrollIntoView,
  destroyCarousel
};
