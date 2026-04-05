/* ═══════════════════════════════════════════
   FREEEDIT — API KEY PAGE JS
═══════════════════════════════════════════ */

// ─── ENTRY ANIMATIONS ───────────────────────
const animEls = document.querySelectorAll('[data-anim]');
animEls.forEach(el => {
  const delay = parseInt(el.dataset.animDelay || 0);
  setTimeout(() => el.classList.add('in'), delay + 100);
});

// ─── PARTICLES ──────────────────────────────
const canvas = document.getElementById('particle-canvas');
const ctx = canvas.getContext('2d');
let W, H, particles = [];

function resize() {
  W = canvas.width = window.innerWidth;
  H = canvas.height = window.innerHeight;
}
resize();
window.addEventListener('resize', resize, { passive: true });

class Particle {
  constructor() { this.reset(true); }
  reset(init = false) {
    this.x = Math.random() * W;
    this.y = init ? Math.random() * H : H + 10;
    this.size = Math.random() * 1.5 + 0.3;
    this.speed = Math.random() * 0.4 + 0.1;
    this.opacity = Math.random() * 0.4 + 0.05;
    this.drift = (Math.random() - 0.5) * 0.3;
    const r = Math.random();
    this.color = r > 0.6 ? `rgba(0,229,255,${this.opacity})` :
                 r > 0.3 ? `rgba(155,89,255,${this.opacity})` :
                            `rgba(255,255,255,${this.opacity})`;
  }
  update() {
    this.y -= this.speed;
    this.x += this.drift;
    if (this.y < -5) this.reset();
  }
  draw() {
    ctx.beginPath();
    ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
    ctx.fillStyle = this.color;
    ctx.fill();
  }
}

for (let i = 0; i < 120; i++) particles.push(new Particle());

function animParticles() {
  ctx.clearRect(0, 0, W, H);
  particles.forEach(p => { p.update(); p.draw(); });
  requestAnimationFrame(animParticles);
}
animParticles();

// ─── TYPING EFFECT ──────────────────────────
const apiTypingEl = document.getElementById('apiTyping');
const apiPrompts = [
  'signin --email you@studio.com',
  'generate --scope unity',
  'copy fe-xxxx-xxxx-xxxx-xxxx',
  'paste → unity/freeedit/panel',
  'connect --engine unity',
];
let aIdx = 0, aChar = 0, aDel = false;

function typeApiPrompt() {
  if (!apiTypingEl) return;
  const cur = apiPrompts[aIdx];
  if (!aDel) {
    aChar++;
    apiTypingEl.textContent = cur.slice(0, aChar);
    if (aChar === cur.length) {
      aDel = true;
      setTimeout(typeApiPrompt, 2000);
      return;
    }
  } else {
    aChar--;
    apiTypingEl.textContent = cur.slice(0, aChar);
    if (aChar === 0) {
      aDel = false;
      aIdx = (aIdx + 1) % apiPrompts.length;
    }
  }
  setTimeout(typeApiPrompt, aDel ? 22 : 55);
}
setTimeout(typeApiPrompt, 800);

// ─── PASSWORD VISIBILITY TOGGLE ─────────────
const toggleBtn = document.getElementById('toggleVis');
const passwordInput = document.getElementById('passwordInput');
const eyeIcon = document.getElementById('eye-icon');
let visible = false;

if (toggleBtn) {
  toggleBtn.addEventListener('click', () => {
    visible = !visible;
    passwordInput.type = visible ? 'text' : 'password';
    eyeIcon.innerHTML = visible
      ? `<path d="M2 2l12 12M6.5 6.7a4 4 0 0 0 5.4 5.8M4 4.4A7.6 7.6 0 0 0 1.5 8s3 5 6.5 5c1.3 0 2.5-.4 3.5-1.1M9.5 3.9A6.6 6.6 0 0 1 14.5 8s-.8 1.3-2.2 2.5" stroke="currentColor" stroke-width="1.3" stroke-linecap="round"/>`
      : `<path d="M8 3C4.5 3 1.5 8 1.5 8s3 5 6.5 5 6.5-5 6.5-5-3-5-6.5-5z" stroke="currentColor" stroke-width="1.3"/><circle cx="8" cy="8" r="2" stroke="currentColor" stroke-width="1.3"/>`;
  });
}

// ─── SIGN-IN & GENERATE-KEY LOGIC ───────────
const emailInput  = document.getElementById('emailInput');
const emailWrap   = document.getElementById('emailWrap');
const passwordWrap = document.getElementById('passwordWrap');
const signinBtn   = document.getElementById('signinBtn');
const btnLabel    = document.getElementById('btnLabel');
const btnArrow    = document.getElementById('btnArrow');
const btnLoader   = document.getElementById('btnLoader');
const statusDot   = document.getElementById('statusDot');
const statusText  = document.getElementById('statusText');
const apiStatus   = document.getElementById('apiStatus');
const apiAuth     = document.getElementById('apiAuth');
const apiKeygen   = document.getElementById('apiKeygen');
const apiSuccess  = document.getElementById('apiSuccess');
const successSub  = document.getElementById('successSub');
const apiStats    = document.getElementById('apiStats');
const keyValue    = document.getElementById('keyValue');
const copyBtn     = document.getElementById('copyBtn');
const copyLabel   = document.getElementById('copyLabel');
const changeBtn   = document.getElementById('changeBtn');

function setStatus(type, text) {
  statusDot.className = 'api-status-dot ' + type;
  statusText.className = 'api-status-text ' + type;
  statusText.textContent = text;
}

function randHex(n) {
  let out = '';
  if (window.crypto && window.crypto.getRandomValues) {
    const buf = new Uint8Array(Math.ceil(n / 2));
    window.crypto.getRandomValues(buf);
    for (let i = 0; i < buf.length; i++) {
      out += buf[i].toString(16).padStart(2, '0');
    }
    return out.slice(0, n);
  }
  while (out.length < n) out += Math.floor(Math.random() * 16).toString(16);
  return out;
}

function generateKey() {
  return `fe-${randHex(4)}-${randHex(4)}-${randHex(4)}-${randHex(12)}`;
}

function clearFieldError(wrap) {
  if (wrap) wrap.classList.remove('error');
}

function showConnecting() {
  btnLabel.style.display = 'none';
  btnArrow.style.display = 'none';
  btnLoader.style.display = 'flex';
  signinBtn.disabled = true;
  emailInput.disabled = true;
  passwordInput.disabled = true;
  setStatus('validating', 'Authenticating…');
}

function resetButton() {
  btnLabel.style.display = 'block';
  btnArrow.style.display = 'block';
  btnLoader.style.display = 'none';
  signinBtn.disabled = false;
  emailInput.disabled = false;
  passwordInput.disabled = false;
}

function showError(msg, wraps = []) {
  resetButton();
  wraps.forEach(w => w && w.classList.add('error'));
  setStatus('error', msg);
  setTimeout(() => wraps.forEach(w => w && w.classList.remove('error')), 3000);
}

function showAuthenticated(key) {
  // Hide the auth form, reveal the keygen block
  apiAuth.style.display = 'none';
  apiKeygen.style.display = 'flex';

  // Populate key + success sub
  keyValue.textContent = key;
  successSub.textContent = 'Key issued — keep it safe';

  // Reset copy button in case of re-auth
  copyBtn.classList.remove('copied');
  copyLabel.textContent = 'Copy';

  // Persist
  try { sessionStorage.setItem('fe_api_key', key); } catch (e) {}
}

if (signinBtn) {
  signinBtn.addEventListener('click', () => {
    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!email && !password) {
      showError('Enter your email and password', [emailWrap, passwordWrap]);
      return;
    }
    if (!email) {
      showError('Enter your email', [emailWrap]);
      return;
    }
    if (!password) {
      showError('Enter your password', [passwordWrap]);
      return;
    }
    if (email.indexOf('@') === -1) {
      showError('Enter a valid email', [emailWrap]);
      return;
    }

    showConnecting();
    // Simulate dummy auth
    setTimeout(() => {
      showAuthenticated(generateKey());
    }, 1400);
  });
}

// Enter submits, typing clears error
[emailInput, passwordInput].forEach((input) => {
  if (!input) return;
  input.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      signinBtn.click();
    }
  });
  input.addEventListener('input', () => {
    clearFieldError(input.closest('.api-input-wrap'));
    if (statusText.classList.contains('error')) {
      setStatus('', 'Enter credentials to sign in');
    }
  });
});

// Copy generated key to clipboard
async function copyToClipboard(text) {
  if (navigator.clipboard && window.isSecureContext) {
    try { await navigator.clipboard.writeText(text); return true; } catch (e) {}
  }
  // Fallback
  try {
    const ta = document.createElement('textarea');
    ta.value = text;
    ta.setAttribute('readonly', '');
    ta.style.position = 'fixed';
    ta.style.opacity = '0';
    document.body.appendChild(ta);
    ta.select();
    const ok = document.execCommand('copy');
    document.body.removeChild(ta);
    return ok;
  } catch (e) { return false; }
}

if (copyBtn) {
  copyBtn.addEventListener('click', async () => {
    const ok = await copyToClipboard(keyValue.textContent);
    if (!ok) return;
    copyBtn.classList.add('copied');
    copyLabel.textContent = 'Copied ✓';
    setTimeout(() => {
      copyBtn.classList.remove('copied');
      copyLabel.textContent = 'Copy';
    }, 1500);
  });
}

// Sign out — reset back to credentials view
if (changeBtn) {
  changeBtn.addEventListener('click', () => {
    apiKeygen.style.display = 'none';
    apiAuth.style.display = 'block';
    emailInput.value = '';
    passwordInput.value = '';
    resetButton();
    clearFieldError(emailWrap);
    clearFieldError(passwordWrap);
    setStatus('', 'Enter credentials to sign in');
  });
}

// ─── INSTALL STEPS WALKTHROUGH ──────────────
const avfSteps = Array.from(document.querySelectorAll('#avfSteps .avf-step'));
const avfLogStatus = document.getElementById('avfLogStatus');
const stepLogMessages = [
  'Launching Unity editor…',
  'Opening Package Manager…',
  'Resolving package from git…',
  'Opening FreeEdit chat window…',
  'Awaiting API key_',
  'All set — ready to edit_',
];

let stepIdx = 0;
function advanceStep() {
  if (!avfSteps.length) return;
  const total = avfSteps.length;
  // Wrap: after the "all done" beat, reset to start
  if (stepIdx > total) stepIdx = 0;

  avfSteps.forEach((el, i) => {
    el.classList.toggle('is-active', stepIdx < total && i === stepIdx);
    el.classList.toggle('is-done',   stepIdx >= total ? true : i < stepIdx);
  });

  if (avfLogStatus) {
    avfLogStatus.textContent = stepLogMessages[Math.min(stepIdx, stepLogMessages.length - 1)];
  }

  stepIdx++;
}
if (avfSteps.length) {
  setTimeout(() => {
    advanceStep();
    setInterval(advanceStep, 2000);
  }, 700);
}

// ─── NAV SCROLLED (always on api page) ──────
// Already set in HTML as .scrolled
