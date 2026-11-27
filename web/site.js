(() => {
  const toggle = document.getElementById("lang");
  const page = document.documentElement.lang === "en" ? "en" : "es";
  const other = page === "en" ? "es" : "en";

  function savedLanguage() {
    try { return localStorage.getItem("lang"); } catch { return null; }
  }

  if (toggle) {
    const saved = savedLanguage();
    if (saved === other) {
      location.replace(toggle.href);
    } else if (!saved && navigator.language.toLowerCase().startsWith(other)) {
      toggle.textContent = other === "en" ? "View in English" : "Ver en español";
      toggle.classList.add("suggest");
    }
    toggle.addEventListener("click", () => {
      try { localStorage.setItem("lang", other); } catch {}
    });
  }

  const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)").matches;
  const observes = "IntersectionObserver" in window;

  const revealed = document.querySelectorAll(".reveal");
  if (observes && !reducedMotion) {
    const observer = new IntersectionObserver(entries => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        entry.target.classList.add("in");
        observer.unobserve(entry.target);
      }
    }, { threshold: 0.2, rootMargin: "0px 0px -10% 0px" });
    revealed.forEach(element => observer.observe(element));
  } else {
    revealed.forEach(element => element.classList.add("in"));
  }

  const lazyVideos = document.querySelectorAll("video[data-play-visible]");
  if (reducedMotion || !observes) {
    lazyVideos.forEach(video => { video.controls = true; });
  } else {
    const observer = new IntersectionObserver(entries => {
      for (const entry of entries) {
        if (entry.isIntersecting) entry.target.play().catch(() => {});
        else entry.target.pause();
      }
    }, { threshold: 0.4 });
    lazyVideos.forEach(video => observer.observe(video));
  }

  for (const track of document.querySelectorAll("[data-marquee]")) {
    const copies = [...track.children].map(item => {
      const copy = item.cloneNode(true);
      copy.setAttribute("aria-hidden", "true");
      return copy;
    });
    track.append(...copies);
  }

  const scenes = [...document.querySelectorAll("[data-scroll]")];
  if (scenes.length && !reducedMotion) {
    const clamp = (value, low, high) => Math.min(Math.max(value, low), high);
    let queued = false;
    const update = () => {
      queued = false;
      const height = innerHeight;
      for (const scene of scenes) {
        const box = scene.getBoundingClientRect();
        if (box.bottom < -height || box.top > 2 * height) continue;
        const reach = height / 2 + box.height / 2;
        scene.style.setProperty("--in", clamp((height - box.top) / reach, 0, 1).toFixed(3));
        scene.style.setProperty("--pass", clamp((height / 2 - box.top - box.height / 2) / reach, -1, 1).toFixed(3));
      }
    };
    const request = () => {
      if (queued) return;
      queued = true;
      requestAnimationFrame(update);
    };
    addEventListener("scroll", request, { passive: true });
    addEventListener("resize", request);
    update();
  }

  for (const tracks of document.querySelectorAll("[data-tracks]")) {
    let seed = 7;
    const random = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
    const count = 56;
    const speech = [[5, 15], [21, 29], [36, 47]];
    const system = Array.from({ length: count }, (_, i) => 0.3 + 0.4 * random() + 0.15 * Math.sin(i / 3));
    const microphone = Array.from({ length: count }, (_, i) =>
      speech.some(([from, to]) => i >= from && i <= to) ? 0.35 + 0.6 * random() : 0.05);
    const levels = {
      mix: system.map((level, i) => Math.min(1, Math.max(level, microphone[i]) + 0.2 * Math.min(level, microphone[i]))),
      system,
      microphone,
    };
    for (const wave of tracks.querySelectorAll("[data-wave]")) {
      wave.innerHTML = levels[wave.dataset.wave]
        .map((level, i) => `<i style="--level:${level.toFixed(2)};--i:${i}"></i>`)
        .join("");
    }
  }
})();
