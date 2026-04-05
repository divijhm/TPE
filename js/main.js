/* ═══════════════════════════════════════════
   FREEEDIT — LANDING JS
   Scroll animations, typing effect, tabs
═══════════════════════════════════════════ */

// ─── NAV SCROLL ───────────────────────────
const nav = document.getElementById('nav');
window.addEventListener('scroll', () => {
  nav.classList.toggle('scrolled', window.scrollY > 40);
}, { passive: true });

// ─── INTERSECTION OBSERVER (scroll reveals) ──
const animEls = document.querySelectorAll('[data-animate]');
const revealObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      const el = entry.target;
      const delay = parseInt(el.dataset.delay || 0);
      setTimeout(() => el.classList.add('visible'), delay);
      revealObserver.unobserve(el);
    }
  });
}, { threshold: 0.1, rootMargin: '0px 0px -60px 0px' });

animEls.forEach(el => revealObserver.observe(el));

// ─── TYPING EFFECT ──────────────────────────
const prompts = [
  'Make this alley cyberpunk — neon signs, rain, purple haze',
  'Add volumetric fog and scatter rain particle emitters',
  'Transform storefronts with neon signage',
  'Increase ambient occlusion, darken shadows, add mist',
];
let promptIdx = 0, charIdx = 0, isDeleting = false;
const promptEl = document.getElementById('typing-prompt');

function typePrompt() {
  if (!promptEl) return;
  const current = prompts[promptIdx];
  if (!isDeleting) {
    charIdx++;
    promptEl.textContent = current.slice(0, charIdx);
    if (charIdx === current.length) {
      isDeleting = true;
      setTimeout(typePrompt, 2200);
      return;
    }
  } else {
    charIdx--;
    promptEl.textContent = current.slice(0, charIdx);
    if (charIdx === 0) {
      isDeleting = false;
      promptIdx = (promptIdx + 1) % prompts.length;
    }
  }
  const speed = isDeleting ? 28 : 48;
  setTimeout(typePrompt, speed);
}
setTimeout(typePrompt, 1200);

// ─── DEMO TABS ──────────────────────────────
const tabs = document.querySelectorAll('.tab');
const scenes = document.querySelectorAll('.demo-scene');

tabs.forEach(tab => {
  tab.addEventListener('click', () => {
    const sceneIdx = tab.dataset.scene;
    tabs.forEach(t => t.classList.remove('active'));
    scenes.forEach(s => s.classList.remove('active'));
    tab.classList.add('active');
    const targetScene = document.querySelector(`.demo-scene[data-scene="${sceneIdx}"]`);
    if (targetScene) {
      targetScene.classList.add('active');
      targetScene.style.animation = 'none';
      targetScene.offsetHeight; // reflow
      targetScene.style.animation = 'scene-in 0.5s cubic-bezier(0.16,1,0.3,1) forwards';
    }
  });
});

// ─── PRODUCTION BAR ANIMATION ───────────────
const prodSection = document.getElementById('production');
const bars = document.querySelectorAll('.pcl-fill');

const barObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      bars.forEach((bar, i) => {
        bar.style.setProperty('--delay', `${i * 0.15}s`);
        bar.style.width = bar.style.getPropertyValue('--w');
      });
      barObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.3 });

if (prodSection) barObserver.observe(prodSection);

// Initially set bars to 0
bars.forEach(bar => {
  const targetW = bar.style.getPropertyValue('--w');
  bar.style.setProperty('--w-target', targetW);
  bar.style.width = '0%';
  bar.style.transition = 'width 1.6s cubic-bezier(0.16,1,0.3,1) var(--delay, 0s)';
});

const barObserver2 = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      bars.forEach((bar, i) => {
        setTimeout(() => {
          bar.style.width = bar.style.getPropertyValue('--w-target') || '100%';
        }, i * 150);
      });
      barObserver2.unobserve(entry.target);
    }
  });
}, { threshold: 0.3 });

if (prodSection) barObserver2.observe(prodSection);

// ─── SMOOTH PARALLAX HERO ───────────────────
const heroBgImg = document.querySelector('.hero-bg-img');
if (heroBgImg) {
  window.addEventListener('scroll', () => {
    const scrollY = window.scrollY;
    const speed = 0.3;
    heroBgImg.style.transform = `translateY(${scrollY * speed}px) scale(1.1)`;
  }, { passive: true });
}

// ─── CURSOR TRAIL (subtle) ──────────────────
const trail = [];
const TRAIL_LEN = 8;
for (let i = 0; i < TRAIL_LEN; i++) {
  const dot = document.createElement('div');
  dot.className = 'cursor-trail-dot';
  dot.style.cssText = `
    position: fixed;
    width: ${4 + i * 1.5}px;
    height: ${4 + i * 1.5}px;
    border-radius: 50%;
    background: rgba(0, 229, 255, ${0.12 - i * 0.012});
    pointer-events: none;
    z-index: 9999;
    transform: translate(-50%, -50%);
    transition: opacity 0.3s;
    top: 0; left: 0;
  `;
  document.body.appendChild(dot);
  trail.push({ el: dot, x: 0, y: 0 });
}

let mouseX = 0, mouseY = 0;
document.addEventListener('mousemove', e => {
  mouseX = e.clientX;
  mouseY = e.clientY;
}, { passive: true });

(function animTrail() {
  let x = mouseX, y = mouseY;
  trail.forEach((dot, i) => {
    dot.el.style.left = dot.x + 'px';
    dot.el.style.top = dot.y + 'px';
    const next = trail[i + 1] || { x: mouseX, y: mouseY };
    dot.x += (x - dot.x) * (0.22 - i * 0.018);
    dot.y += (y - dot.y) * (0.22 - i * 0.018);
    x = dot.x; y = dot.y;
  });
  requestAnimationFrame(animTrail);
})();

// ─── HOW-CARD STAGGER ───────────────────────
const howCards = document.querySelectorAll('.how-card');
const howObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      howCards.forEach((card, i) => {
        setTimeout(() => {
          card.classList.add('visible');
        }, i * 100);
        card.dataset.animate = 'fade-up';
      });
      howObserver.disconnect();
    }
  });
}, { threshold: 0.15 });
const howSection = document.querySelector('.how-grid');
if (howSection) howObserver.observe(howSection);

// ─── PIPELINE STEP STAGGER ──────────────────
const psCards = document.querySelectorAll('.ps-card');
const pipeObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      psCards.forEach((card, i) => {
        setTimeout(() => card.classList.add('visible'), i * 120);
        card.dataset.animate = 'fade-up';
      });
      pipeObserver.disconnect();
    }
  });
}, { threshold: 0.15 });
const pipeSteps = document.querySelector('.pipeline-steps');
if (pipeSteps) pipeObserver.observe(pipeSteps);

// ─── PS-CARD & HOW-CARD initial state ───────
[...psCards, ...howCards].forEach(card => {
  card.style.opacity = '0';
  card.style.transform = 'translateY(28px)';
  card.style.transition = 'opacity 0.7s cubic-bezier(0.16,1,0.3,1), transform 0.7s cubic-bezier(0.16,1,0.3,1)';
});

// Override "visible" class for these specific cards
document.head.insertAdjacentHTML('beforeend', `
  <style>
    .ps-card.visible, .how-card.visible {
      opacity: 1 !important;
      transform: translateY(0) !important;
    }
    @keyframes scene-in {
      from { opacity: 0; transform: translateY(16px) scale(0.99); }
      to   { opacity: 1; transform: translateY(0) scale(1); }
    }
    .demo-scene.active { animation: scene-in 0.5s cubic-bezier(0.16,1,0.3,1) forwards; }
  </style>
`);
