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
    const panel = document.getElementById("detailPanel");
    panel.classList.remove("hidden");
    
    //const backdrop = document.getElementById("panelBackdrop");
    //backdrop.classList.remove("hidden");

    document.getElementById("detailImage").src = `/FramesOutput/${frame.framePath}`;

    document.getElementById("dLabel").innerText = frame.label || "-";
    document.getElementById("dConfidence").innerText = (frame.confidence * 100).toFixed(1) + "%";
    document.getElementById("dTime").innerText = new Date(frame.detectionTime).toLocaleString();
    document.getElementById("dVideo").innerText = frame.VideoFile || "-";

    const video = document.getElementById("videoPlayer");

    if (video.src.indexOf(frame.videoFile) === -1) {
        video.src = `/videos/${frame.videoFile}`;
    }

    video.currentTime = 0;

    updateNavButtons();
    highlightActiveCard(index);
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
    //document.getElementById("panelBackdrop").classList.add("hidden");
}