let cursor = null;

let loading = false;

let reachedEnd = false;

let observer = null;

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

async function searchFrames() {

    if (loading) return;

    loading = true;

    const loader = document.getElementById("loadingIndicator");
    loader.style.display = "block";

    try {

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

    frames.forEach(f => {

        const card = document.createElement("div");

        card.className = "frame-card";

        card.innerHTML = `<img loading="lazy" src="/Frames/${f.framePath}"/>

        <div class="frame-meta">

        <div>${f.label}</div>

        <div>${new Date(f.timestamp).toLocaleString()}</div>

        </div>

        `;

        container.appendChild(card);
    });
}