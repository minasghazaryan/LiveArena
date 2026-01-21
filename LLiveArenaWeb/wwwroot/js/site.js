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
