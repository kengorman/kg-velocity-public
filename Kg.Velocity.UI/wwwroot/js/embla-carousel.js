import EmblaCarousel from 'embla-carousel';

// Store carousel instances by container ID
const carousels = new Map();

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

  // Apply scale effect on init and scroll
  const applyScaleEffect = () => {
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
    applyScaleEffect();

    // Notify Blazor of selection change
    if (dotNetRef) {
      dotNetRef.invokeMethodAsync('OnCarouselSelect', containerId, index);
    }
  });

  // Event: scroll in progress (for smooth scale during drag)
  embla.on('scroll', () => {
    applyScaleEffect();
  });

  // Initial scale application
  applyScaleEffect();

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
  destroyCarousel
};
