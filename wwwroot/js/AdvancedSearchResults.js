let cursor = null;

let loading = false;

let reachedEnd = false;

let observer = null;

let allFrames = [];

let currentIndex = -1;

document.addEventListener("DOMContentLoaded", function () {
    initilizeInfiniteScroll();
});

function initilizeInfiniteScroll() {
    const sentinel = document.getElementById("scrollSentinel");
    if (!sentinel) {
        console.error("scrollSentinel element not found");
        return;
    }
    observer = new IntersectionObserver(enteries => {
        if (enteries[0].isIntersecting) {
            searchFrames();
        }
    },
    {
        rootMargin:"200px"
    });
    observer.observe(sentinel);
}

async function searchFrames(reset = false) {

    if (loading) return;

    loading = true;

    const loader = document.getElementById("loadingIndicator");
    loader.style.display = "block";

    try {

        if (reset) {
            document.getElementById("resultsContainer").innerHTML = "";
            cursor = null;
            reachedEnd = false;
        }

        const request = buildSearchRequest();

        const response = await fetch("/SearchFrames/Frames", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!response.ok) {
            throw new Error("Server error: " + response.status);
        }

        const frames = await response.json();

        if (!frames || frames.length === 0) {

            reachedEnd = true;

            loader.innerText = "No more frames";

            loading = false;

            return;

        }

        renderFrames(frames);

        cursor = frames[frames.length - 1].detectionTime;

        loader.style.display = "none";

    }
    catch (error) {
        console.error("Frame loading failed:", error);
        loader.innerText = "Failed to load frames";
    }
    finally {

        loading = false;
    }
}

function buildSearchRequest() {

    let text = document.getElementById("searchInput").value;

    let filters = [];

    document.querySelectorAll(".filter-chip").forEach(chip => {

        let key = chip.querySelector("span").innerText.replace(":", "");

        let value = chip.querySelector("input").value;

        filters.push({

            key: key,
            operator: "eq",
            value: value

        });

    });

    return {

        text: text,
        filters: filters,
        cursor: cursor,
        limit: 60
    };
}

function renderFrames(frames) {

    const container = document.getElementById("resultsContainer");

    frames.forEach((f, index) => {

        allFrames.push(f);
        const globalIndex = allFrames.length - 1;

        const card = document.createElement("div");

        card.className = "frame-card";

        card.innerHTML = `<img loading="lazy" src="/FramesOutput/${f.framePath}"/>

        <div class="frame-meta">

        <div>${f.label}</div>

        <div>${new Date(f.detectionTime).toLocaleString()}</div>

        </div>

        `;

        card.onclick = () => openDetailPanel(globalIndex);

        container.appendChild(card);
    });
}

function openDetailPanel(index) {
    currentIndex = index;
    const frame = allFrames[index];

    resetImageView();

    const panel = document.getElementById("detailPanel");
    panel.classList.remove("hidden");
    
    document.getElementById("detailImage").src = `/FramesOutput/${frame.framePath}`;

    document.getElementById("dLabel").innerText = frame.label || "-";
    document.getElementById("dConfidence").innerText = (frame.confidence * 100).toFixed(1) + "%";
    document.getElementById("dTime").innerText = new Date(frame.detectionTime).toLocaleString();
    document.getElementById("dVideo").innerText = frame.VideoFile || "-";

    const video = document.getElementById("videoPlayer");

    //if (video.src.indexOf(frame.videoFile) === -1) {
    //    video.src = `/videos/${frame.videoFile}`;
    //}

    if (!video.src.includes(frame.videoFile)) {
        video.src = `/videos/${frame.videoFile}`;
    }

    video.currentTime = 0;

    updateNavButtons();
    highlightActiveCard(index);
    buildTimeline(frame);
}

function updateNavButtons() {

    document.getElementById("prevBtn").disabled = currentIndex === 0;

    document.getElementById("nextBtn").disabled =
        currentIndex === allFrames.length - 1;
}

function nextFrame() {
    if (currentIndex < allFrames.length - 1) {
        openDetailPanel(currentIndex + 1);
    }
}

function prevFrame() {
    if (currentIndex > 0) {
        openDetailPanel(currentIndex - 1);
    }
}

function highlightActiveCard(index) {

    document.querySelectorAll(".frame-card")
        .forEach(c => c.classList.remove("active"));

    document.querySelectorAll(".frame-card")[index]
        ?.classList.add("active");
}

function closePanel() {
    document.getElementById("detailPanel").classList.add("hidden");
}

function switchTab(tabId, el) {

    document.querySelectorAll(".tab").forEach(t => t.classList.remove("active"));
    document.querySelectorAll(".tab-pane").forEach(p => p.classList.remove("active"));

    el.classList.add("active");
    document.getElementById(tabId).classList.add("active");
}

document.addEventListener("click", function (e) {

    if (e.target.classList.contains("zoomable")) {
        e.target.classList.toggle("zoomed");
    }

});

let scale = 1;

const img = document.getElementById("detailImage");

img.addEventListener("wheel", function (e) {
    e.preventDefault();

    if (e.deltaY < 0) scale += 0.1;
    else scale -= 0.1;

    scale = Math.max(1, Math.min(scale, 3));

    img.style.transform = `scale(${scale})`;
});     

let isDragging = false;
let startX, startY, posX = 0, posY = 0;

img.addEventListener("mousedown", (e) => {
    if (scale <= 1) return;

    isDragging = true;
    startX = e.clientX - posX;
    startY = e.clientY - posY;

    img.style.cursor = "grabbing";
});

document.addEventListener("mouseup", () => {
    isDragging = false;
    img.style.cursor = "grab";
});

document.addEventListener("mousemove", (e) => {
    if (!isDragging) return;

    posX = e.clientX - startX;
    posY = e.clientY - startY;

    img.style.transform = `translate(${posX}px, ${posY}px) scale(${scale})`;
});

function resetImageView() {

    scale = 1;
    posX = 0;
    posY = 0;

    const img = document.getElementById("detailImage");

    img.style.transform = "translate(0px, 0px) scale(1)";
}

function buildTimeline(currentFrame) {

    const timeline = document.getElementById("timelineBar");
    timeline.innerHTML = "";

    const videoFrames = allFrames.filter(f =>
        f.videoFile === currentFrame.videoFile
    );

    if (videoFrames.length === 0) return;

    // Normalize time range
    const times = videoFrames.map(f => new Date(f.detectionTime).getTime());

    const minTime = Math.min(...times);
    const maxTime = Math.max(...times);

    videoFrames.forEach(f => {

        const t = new Date(f.detectionTime).getTime();

        const percent = (t - minTime) / (maxTime - minTime);
        const marker = document.createElement("div");
        marker.className = "timeline-marker";

        marker.style.left = (percent * 100) + "%";

        marker.onclick = () => jumpToFrame(f, minTime, maxTime);

        // 👉 visible label
        const label = document.createElement("div");
        label.className = "marker-label";
        label.innerText = f.label || "obj";

        // 👉 visible time
        const time = document.createElement("div");
        time.className = "marker-time";

        const dt = new Date(f.detectionTime);
        time.innerText = dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        marker.appendChild(label);
        marker.appendChild(time);

        timeline.appendChild(marker);
    });
}

function jumpToFrame(frame, minTime, maxTime) {

    const video = document.getElementById("videoPlayer");

    const t = new Date(frame.detectionTime).getTime();

    const percent = (t - minTime) / (maxTime - minTime);

    const duration = video.duration || 60; // fallback

    const targetTime = percent * duration;

    video.currentTime = targetTime;
    video.play();
}