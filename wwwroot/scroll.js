(() => {
  const bodyDOM = document.querySelector("body");
  bodyDOM.style.overflow = "hidden";

  // 自動位移
  let moving = false;
  window.addEventListener("mousewheel", (e) => {
    const scrollPower = Math.abs(e.deltaY);
    if (moving) return;
    if (scrollPower < 14) return;
    const sectionDOMs = [...document.querySelectorAll(".section")];

    const sectionOffsetTopArray = sectionDOMs.map((dom) => dom.offsetTop);
    //找最接近的距離 index
    const nowIndex = (() => {
      let closeDiff = 100000;
      let resultIndex = 0;
      sectionOffsetTopArray.forEach((top, i) => {
        const tempDiff = Math.abs(window.scrollY - top);
        if (tempDiff < closeDiff) {
          closeDiff = tempDiff;
          resultIndex = i;
        }
      });
      return resultIndex;
    })();
    const goToIndex = (() => {
      if (e.deltaY > 0 && nowIndex < sectionDOMs.length - 1)
        return nowIndex + 1;
      if (e.deltaY < 0 && nowIndex > 0) return nowIndex - 1;
      return nowIndex;
    })();

    moving = true;
    scrollTo({
      top: sectionDOMs[goToIndex].offsetTop,
      duration: 800,
      easingName: "easeOutQuint",
      callback: () => {
        moving = false;
      },
    });
  });

  // RWD 模擬 scroll 事件
  let touchStartPointY = null;
  window.addEventListener("touchstart", (e) => {
    touchStartPointY = e.touches[0].screenY;
  });
  window.addEventListener("touchend", (e) => {
    if (!touchStartPointY) return;
    const distance = touchStartPointY - e.changedTouches[0].screenY;
    touchStartPointY = null;

    //無視短距離的位移
    if (Math.abs(distance) < 50) return;

    window.dispatchEvent(
      new WheelEvent("mousewheel", {
        deltaY: distance > 0 ? 100 : -100,
      })
    );
  });

  // 客製 scrollTo
  function scrollTo({ top, duration = 300, easingName = "linear", callback }) {
    const start = Date.now();
    const elem = !Number.isNaN(document.documentElement.scrollTop)
      ? document.documentElement
      : document.body;
    const from = elem.scrollTop;
    const easing = {
      linear: function (t) {
        return t;
      },
      easeInQuad: function (t) {
        return t * t;
      },
      easeOutQuad: function (t) {
        return t * (2 - t);
      },
      easeInOutQuad: function (t) {
        return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
      },
      easeInCubic: function (t) {
        return t * t * t;
      },
      easeOutCubic: function (t) {
        return --t * t * t + 1;
      },
      easeInOutCubic: function (t) {
        return t < 0.5
          ? 4 * t * t * t
          : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
      },
      easeInQuart: function (t) {
        return t * t * t * t;
      },
      easeOutQuart: function (t) {
        return 1 - --t * t * t * t;
      },
      easeInOutQuart: function (t) {
        return t < 0.5 ? 8 * t * t * t * t : 1 - 8 * --t * t * t * t;
      },
      easeInQuint: function (t) {
        return t * t * t * t * t;
      },
      easeOutQuint: function (t) {
        return 1 + --t * t * t * t * t;
      },
      easeInOutQuint: function (t) {
        return t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * --t * t * t * t * t;
      },
      easeOutCirc: function (t) {
        return Math.sqrt(1 - --t * t);
      }
    };

    const easingFunction = easing[easingName] || easing["linear"];
    if (from === top) {
      if (callback) callback();
      return; /* Prevent scrolling to the Y point if already there */
    }

    function scroll() {
      const currentTime = Date.now();
      const time = Math.min(1, (currentTime - start) / duration);
      const easedT = easingFunction(time);
      elem.scrollTop = easedT * (top - from) + from;

      if (time < 1) requestAnimationFrame(scroll);
      else if (callback) callback();
    }
    requestAnimationFrame(scroll);
  }
})();
