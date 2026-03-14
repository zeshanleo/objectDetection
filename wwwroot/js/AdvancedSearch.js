// advanced-search.js
const dropdown = document.getElementById('filterDropdown');
const toggle = document.getElementById('filterToggle');
const selectedContainer = document.getElementById('selectedFilters');
const searchInput = document.getElementById('searchInput');

toggle.addEventListener('click', () => {
    dropdown.style.display = dropdown.style.display === 'block' ? 'none' : 'block';
});

function selectFilter(name) {
    // Create chip container
    const chip = document.createElement('div');
    chip.className = 'filter-chip';

    // Create label span
    const labelSpan = document.createElement('span');
    labelSpan.textContent = name + ':';
    chip.appendChild(labelSpan);

    // Create input for the filter value
    const valueInput = document.createElement('input');
    valueInput.type = 'text';
    valueInput.placeholder = 'value...';
    valueInput.className = 'chip-input';
    valueInput.addEventListener('input', updateSearchInput);
    chip.appendChild(valueInput);

    // Create remove button
    const removeBtn = document.createElement('span');
    removeBtn.textContent = '×';
    removeBtn.onclick = () => chip.remove() || updateSearchInput();
    chip.appendChild(removeBtn);

    // Append chip to container
    selectedContainer.appendChild(chip);

    // Focus on the input immediately
    valueInput.focus();

    dropdown.style.display = 'none';
    updateSearchInput();
}

// Update the main search input based on chips
function updateSearchInput() {
    const chips = selectedContainer.querySelectorAll('.filter-chip');
    const searchTerms = [];
    chips.forEach(chip => {
        const label = chip.querySelector('span').textContent;
        const value = chip.querySelector('input').value;
        if (value.trim()) {
            searchTerms.push(label + value);
        } else {
            searchTerms.push(label); // just the filter name if no value
        }
    });
    searchInput.value = searchTerms.join(' ');
}

function removeChip(el) {
    el.parentElement.remove();
}

document.addEventListener('click', function (event) {
    if (!event.target.closest('.advanced-search-wrapper')) {
        dropdown.style.display = 'none';
    }
});