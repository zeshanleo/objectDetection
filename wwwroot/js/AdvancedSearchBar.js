const dropdown = document.getElementById("filterDropdown");
const toggle = document.getElementById("filterToggle");
const chipContainer = document.getElementById("chipContainer");
const searchInput = document.getElementById("searchInput");
const suggestionBox = document.getElementById("suggestionBox");
const validFilters = ["Label", "Camera", "Range"];
let selectedSuggestionIndex = -1;
let currentSuggestions = [];
let activeInput = null;

toggle.onclick = () => {
    dropdown.style.display =
        dropdown.style.display === "block" ? "none" : "block";
};

function selectFilter(name) {

    const chip = document.createElement("div");
    chip.className = "filter-chip";

    const label = document.createElement("span");
    label.textContent = name + ":";

    const input = document.createElement("input");
    if (name === "Label") {
        input.placeholder = "object type";
    }
    else if (name === "Range") {

        input.placeholder = "Select range";

        flatpickr(input, {
            mode: "range",
            dateFormat: "Y-m-d",
        });
    }
    else {
        input.placeholder = "value";
    }

    const remove = document.createElement("span");
    remove.textContent = "×";
    remove.className = "remove";

    remove.onclick = () => chip.remove();

    chip.appendChild(label);
    chip.appendChild(input);
    chip.appendChild(remove);

    chipContainer.appendChild(chip);

    dropdown.style.display = "none";

    input.focus();
    if (name === "Label")
        attachSuggestionToInput(input);
}

document.addEventListener("click", function (event) {

    if (!event.target.closest(".advanced-search-wrapper")) {
        dropdown.style.display = "none";
        suggestionBox.style.display = "none";
    }
});

searchInput.addEventListener("input", async function () {
    let value = searchInput.value;
    if (value.length < 2) {
        suggestionBox.style.display = "none";
        return;
    }

    let response = await fetch(`/SearchFrames/Suggest?q=${value}`);
    let suggestions = await response.json();
    renderSuggestions(suggestions);
});

document.addEventListener("keydown", function (e) {

    const items = suggestionBox.querySelectorAll(".suggestion-item");

    if (!items.length) return;

    if (e.key === "ArrowDown") {

        e.preventDefault();

        selectedSuggestionIndex++;

        if (selectedSuggestionIndex >= items.length)
            selectedSuggestionIndex = 0;

        highlightSuggestion(items);

    }

    if (e.key === "ArrowUp") {

        e.preventDefault();

        selectedSuggestionIndex--;

        if (selectedSuggestionIndex < 0)
            selectedSuggestionIndex = items.length - 1;

        highlightSuggestion(items);

    }

    if (e.key === "Enter") {

        if (selectedSuggestionIndex >= 0) {
            selectSuggestion(selectedSuggestionIndex);
        }
        searchFrames();
    }

});

searchInput.addEventListener("focus", function () {
    activeInput = searchInput;
});

function renderSuggestions(items) {

    suggestionBox.innerHTML = "";
    currentSuggestions = items;
    selectedSuggestionIndex = -1;

    if (!items || items.length === 0) {
        suggestionBox.style.display = "none";
        return;
    }

    items.forEach(item => {

        const div = document.createElement("div");
        div.className = "suggestion-item";
        div.textContent = item;

        div.onclick = () => {
            selectSuggestion(index);
            //searchInput.value = item;
            //suggestionBox.style.display = "none";

            //runSearch();

        };

        suggestionBox.appendChild(div);

    });

    suggestionBox.style.display = "block";
}

function highlightSuggestion(items) {

    items.forEach(i => i.classList.remove("active"));

    items[selectedSuggestionIndex].classList.add("active");

}

function selectSuggestion(index) {

    const value = currentSuggestions[index];

    if (activeInput === searchInput) {

        addFilterChip("Label", value);
        searchInput.value = "";
    }
    else {

        activeInput.value = value;
    }

    suggestionBox.style.display = "none";
    currentSuggestions = [];
    selectedSuggestionIndex = -1;

    //runSearch();
}

function attachSuggestionToInput(input) {

    input.addEventListener("input", async function () {
        activeInput = input;

        let value = input.value;

        if (value.length < 2) return;

        let response = await fetch(`/SearchFrames/Suggest?q=${value}`);

        let suggestions = await response.json();

        renderSuggestionsForInput(input, suggestions);
    });
}

function renderSuggestionsForInput(input, items) {

    suggestionBox.innerHTML = "";
    currentSuggestions = items;
    selectedSuggestionIndex = -1;

    if (!items || items.length === 0) {
        suggestionBox.style.display = "none";
        return;
    }

    items.forEach(item => {

        let div = document.createElement("div");
        div.className = "suggestion-item";
        div.textContent = item;

        div.onclick = () => {

            input.value = item;
            suggestionBox.style.display = "none";
            currentSuggestions = [];
            selectedSuggestionIndex = -1;
        };

        suggestionBox.appendChild(div);

    });

    suggestionBox.style.display = "block";
}

function addFilterChip(name, value) {

    const chip = document.createElement("div");
    chip.className = "filter-chip";

    const label = document.createElement("span");
    label.textContent = name + ":";

    const input = document.createElement("input");
    input.value = value || "";
    input.placeholder = "value";

    const remove = document.createElement("span");
    remove.textContent = "×";
    remove.className = "remove";

    remove.onclick = () => chip.remove();

    chip.appendChild(label);
    chip.appendChild(input);
    chip.appendChild(remove);

    chipContainer.appendChild(chip);

    attachSuggestionToInput(input);
}