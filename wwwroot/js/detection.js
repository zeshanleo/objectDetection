//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("/detectionHub")
//    .build();

//connection.on("ReceiveDetection", (framePath, jsonResult) => {
//    document.getElementById("frameView").src = framePath + "?t=" + new Date().getTime();
//    document.getElementById("resultJson").textContent = jsonResult;
//});

//connection.start().catch(err => console.error(err));
document.addEventListener("DOMContentLoaded", function () {

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/detectionHub")
        .build();

    connection.start()
        .then(() => console.log("Connected to SignalR hub"))
        .catch(err => console.error("SignalR error:", err));

    // Listen for REAL-TIME progress updates
    connection.on("ReceiveProgress", (data) => {
        console.log("Progress Update:", data);

        const video = data.videoName;
        const statusEl = document.getElementById(`status-${video}`);
        const barEl = document.getElementById(`progress-${video}`);
        const frameEl = document.getElementById(`viewFrames-${video}`);

        if (frameEl) {
            if (data.status == "Completed") {
                frameEl.classList.remove("disabled");
            }
        }

        if (statusEl) statusEl.innerText = data.status;

        if (barEl) {
            barEl.style.width = `${data.progress}%`;
            barEl.innerText = `${data.progress}%`;
        }
    });
});
