// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
  const categoryList = document.querySelector("[data-stream-categories]");
  const frame = document.querySelector(".stream-frame");
  const loader = document.querySelector("[data-stream-loader]");
  const streamFrame = document.getElementById("streamFrame");

  if (!categoryList || !frame || !loader || !streamFrame) {
    return;
  }

  const footballStreamUrl = "https://example.com";
  const basketballStreamUrl = "https://example.com";
  const ufcStreamUrl = "https://example.com";
  const tennisStreamUrl = "https://example.com";
  const formula1StreamUrl = "https://example.com";

  const setLoading = (isLoading) => {
    frame.classList.toggle("is-loading", isLoading);
  };

  const updateActive = (targetButton) => {
    const buttons = categoryList.querySelectorAll(".category-item");
    buttons.forEach((button) => button.classList.remove("active"));
    targetButton.classList.add("active");
  };

  const switchStream = (targetButton) => {
    const key = targetButton.getAttribute("data-stream-key");
    const urlMap = {
      football: footballStreamUrl,
      basketball: basketballStreamUrl,
      ufc: ufcStreamUrl,
      tennis: tennisStreamUrl,
      formula1: formula1StreamUrl,
    };
    const nextSrc = urlMap[key];
    if (!nextSrc || streamFrame.src === nextSrc) {
      return;
    }

    updateActive(targetButton);
    setLoading(true);

    streamFrame.addEventListener(
      "load",
      () => {
        setLoading(false);
      },
      { once: true }
    );

    streamFrame.src = nextSrc;
  };

  categoryList.addEventListener("click", (event) => {
    const target = event.target.closest(".category-item");
    if (!target) {
      return;
    }
    switchStream(target);
  });

  setLoading(false);
});

document.addEventListener("DOMContentLoaded", () => {
  const scoreRows = document.querySelectorAll("[data-score-row]");
  if (!scoreRows.length) {
    return;
  }

  const updateRow = (row) => {
    const timeCell = row.querySelector("[data-time-value]");
    const scoreCell = row.querySelector("[data-score-value]");
    if (!timeCell || !scoreCell) {
      return;
    }

    const currentTime = parseInt(timeCell.getAttribute("data-time-value"), 10);
    if (!Number.isFinite(currentTime) || timeCell.textContent === "FT") {
      return;
    }

    const nextTime = Math.min(currentTime + 1, 90);
    timeCell.setAttribute("data-time-value", String(nextTime));
    timeCell.textContent = `${nextTime}'`;

    const scoreValue = scoreCell.getAttribute("data-score-value");
    if (!scoreValue) {
      return;
    }

    const [home, away] = scoreValue.split("-").map((value) => parseInt(value, 10));
    if (!Number.isFinite(home) || !Number.isFinite(away)) {
      return;
    }

    if (Math.random() > 0.85 && nextTime < 90) {
      const updatedHome = home + (Math.random() > 0.5 ? 1 : 0);
      const updatedAway = away + (updatedHome === home ? 1 : 0);
      const nextScore = `${updatedHome}-${updatedAway}`;
      scoreCell.setAttribute("data-score-value", nextScore);
      scoreCell.textContent = `${updatedHome} - ${updatedAway}`;
      row.classList.add("is-updated");
      setTimeout(() => row.classList.remove("is-updated"), 600);
    }
  };

  setInterval(() => {
    scoreRows.forEach(updateRow);
  }, 4000);
});

document.addEventListener("DOMContentLoaded", () => {
  const countdowns = document.querySelectorAll("[data-countdown]");
  if (!countdowns.length) {
    return;
  }

  const updateCountdown = (container) => {
    const targetIso = container.getAttribute("data-target");
    if (!targetIso) {
      return;
    }

    const targetDate = new Date(targetIso);
    if (Number.isNaN(targetDate.getTime())) {
      return;
    }

    const now = new Date();
    const diff = Math.max(0, targetDate - now);
    const totalSeconds = Math.floor(diff / 1000);
    const days = Math.floor(totalSeconds / 86400);
    const hours = Math.floor((totalSeconds % 86400) / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    const setValue = (selector, value) => {
      const el = container.querySelector(selector);
      if (el) {
        el.textContent = String(value).padStart(2, "0");
      }
    };

    setValue("[data-days]", days);
    setValue("[data-hours]", hours);
    setValue("[data-minutes]", minutes);
    setValue("[data-seconds]", seconds);
  };

  countdowns.forEach((container) => {
    updateCountdown(container);
    setInterval(() => updateCountdown(container), 1000);
  });
});

document.addEventListener("DOMContentLoaded", () => {
  const logoImages = document.querySelectorAll(".team-logo img");
  logoImages.forEach((img) => {
    img.addEventListener("error", () => {
      const initials = img.getAttribute("data-fallback-initials") || "TBD";
      const parent = img.parentElement;
      if (!parent) {
        return;
      }
      parent.textContent = initials
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0])
        .join("")
        .toUpperCase();
      parent.classList.add("team-logo-fallback");
    });
  });
});

document.addEventListener("DOMContentLoaded", () => {
  const toggles = document.querySelectorAll("[data-collapse-toggle]");
  if (!toggles.length) {
    return;
  }

  const setPanelHeight = (panel) => {
    const inner = panel.querySelector(".league-collapse-inner");
    if (!inner) {
      return;
    }
    panel.style.maxHeight = `${inner.scrollHeight}px`;
  };

  toggles.forEach((toggle) => {
    toggle.addEventListener("click", () => {
      const targetId = toggle.getAttribute("data-collapse-toggle");
      if (!targetId) {
        return;
      }

      const panel = document.querySelector(`[data-collapse-panel="${targetId}"]`);
      if (!panel) {
        return;
      }

      if (panel.classList.contains("show")) {
        panel.style.maxHeight = `${panel.scrollHeight}px`;
        requestAnimationFrame(() => {
          panel.classList.remove("show");
          panel.style.maxHeight = "0px";
        });
      } else {
        panel.classList.add("show");
        requestAnimationFrame(() => setPanelHeight(panel));
      }
    });
  });

  document.querySelectorAll("[data-collapse-panel].show").forEach((panel) => {
    setPanelHeight(panel);
  });

  window.addEventListener("resize", () => {
    document.querySelectorAll("[data-collapse-panel].show").forEach(setPanelHeight);
  });
});
