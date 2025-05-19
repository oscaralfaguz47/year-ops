document.addEventListener('DOMContentLoaded', () => {
    const searchContainers = document.querySelectorAll('[data-global-search]');

    searchContainers.forEach(container => {
        const searchInput = container.querySelector('.global-search-input');
        const searchResults = container.querySelector('.global-search-results');
        const selectedInput = container.querySelector('.selected-entity-display');
        const entityIdInput = container.querySelector('.selected-entity-id');
        const filterToggleCheckbox = container.querySelector('.filter-toggle-checkbox');
        const entityName = container.getAttribute('data-entity');
        let activeIndex = -1;
        let items = [];

        // Handle input event to fetch items
        searchInput.addEventListener('input', async (event) => {
            const query = event.target.value.trim();

            // Hide results if query is too short
            if (query.length < 2) {
                hideResults(searchResults);
                return;
            }

            try {
                // Fetch items from backend
                const showInactive = filterToggleCheckbox.checked;
                const searchEndpointUrl = container.getAttribute('data-search-url') || `/${entityName}/Search`;
                const resultListPropertyName = container.getAttribute('data-result-list-property') || "items";

                const response = await fetch(`${searchEndpointUrl}?searchText=${encodeURIComponent(query)}&showInactive=${encodeURIComponent(showInactive)}`);
                const data = await response.json();

                // Accede a la propiedad del resultado usando el nombre dinámico
                items = data[resultListPropertyName] || [];

                // Display results if available
                if (items.length > 0) {
                    searchResults.innerHTML = items.map((item, index) => {
                        const isActive = item.text.includes("(Active)");
                        const statusClass = isActive ? "entity-status-active" : "entity-status-inactive";
                        const cleanItemName = item.text.replace(/ - \((Active|Inactive)\)$/, '').trim();

                        return `
            <div class="entity-result" data-entity-id="${item.value}" data-entity-name="${item.text}" data-index="${index}">
                ${cleanItemName} <span class="${statusClass}">${isActive ? "(Active)" : "(Inactive)"}</span>
            </div>
        `;
                    }).join('');
                    searchResults.style.display = 'block';
                    activeIndex = -1;
                    positionResults(searchInput, searchResults);
                } else {
                    searchResults.innerHTML = `<div class="no-results">No results found.</div>`;
                    searchResults.style.display = 'block';
                }
            } catch (error) {
                console.error('Error fetching items:', error);
                searchResults.innerHTML = `<div class="no-results">Error loading ${entityName}.</div>`;
                searchResults.style.display = 'block';
            }
        });

        // Handle keyboard navigation
        searchInput.addEventListener('keydown', (event) => {
            const results = searchResults.querySelectorAll('.entity-result');

            if (event.key === 'ArrowDown') {
                activeIndex = (activeIndex + 1) % results.length;
                updateActiveResult(results, activeIndex);
            } else if (event.key === 'ArrowUp') {
                activeIndex = (activeIndex - 1 + results.length) % results.length;
                updateActiveResult(results, activeIndex);
            } else if (event.key === 'Enter') {
                event.preventDefault();
                if (activeIndex >= 0) {
                    selectEntity(results[activeIndex], selectedInput, entityIdInput, searchResults, searchInput);
                }
            } else if (event.key === 'Escape') {
                hideResults(searchResults, true, searchInput);
            }
        });

        // Handle mouse click on results
        searchResults.addEventListener('click', (event) => {
            if (event.target.classList.contains('entity-result')) {
                selectEntity(event.target, selectedInput, entityIdInput, searchResults, searchInput);
            }
        });

        // Close the results list if clicked outside the container
        document.addEventListener('click', (event) => {
            if (!event.target.closest('[data-global-search]')) {
                hideResults(searchResults, false, searchInput);
            }
        });
    });
});

// Update active result with keyboard navigation
function updateActiveResult(results, activeIndex) {
    results.forEach((result, index) => {
        result.classList.toggle('active', index === activeIndex);
        if (index === activeIndex) {
            result.scrollIntoView({ block: 'nearest' });
        }
    });
}

// Handle entity selection
function selectEntity(element, selectedInput, entityIdInput, searchResults, searchInput) {
    const fullEntityName = element.getAttribute('data-entity-name');
    const entityId = element.getAttribute('data-entity-id');
    const cleanEntityName = fullEntityName.replace(/ - \((Active|Inactive)\)$/, '').trim();

    selectedInput.value = cleanEntityName;
    entityIdInput.value = entityId;
    selectedInput.style.display = 'block';

    hideResults(searchResults, true, searchInput);
}

// Hide results and optionally clear input
function hideResults(searchResults, clearInput = false, searchInput = null) {
    searchResults.style.display = 'none';
    searchResults.innerHTML = '';
    if (clearInput && searchInput) {
        searchInput.value = '';
    }
}

// Position the results container correctly
function positionResults(searchInput, searchResults) {
    const inputRect = searchInput.getBoundingClientRect();
    const containerRect = searchInput.closest('.global-search-container').getBoundingClientRect();

    searchResults.style.top = `${inputRect.height + 5}px`;
    searchResults.style.left = `0px`;
    searchResults.style.width = `${containerRect.width}px`;
}
