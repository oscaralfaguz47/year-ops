// client-search.js

document.addEventListener('DOMContentLoaded', () => {
    const searchContainers = document.querySelectorAll('[data-client-search]');

    searchContainers.forEach(container => {
        const searchInput = container.querySelector('.client-search-input');
        const searchResults = container.querySelector('.client-search-results');
        const selectedInput = container.querySelector('.client-selected-input');
        const clientIdInput = container.querySelector('.clientId');
        let activeIndex = -1;
        let clients = [];

        // Handle input event to fetch clients
        searchInput.addEventListener('input', async (event) => {
            const query = event.target.value.trim();

            // Hide results if query is too short
            if (query.length < 2) {
                hideResults(searchResults);
                return;
            }

            try {
                // Fetch clients from backend
                const response = await fetch(`/AccountManagement/Clients/SearchClientsByName?nameOrAlias=${encodeURIComponent(query)}`);
                clients = await response.json();

                // Display results if available
                if (clients.clientsList.length > 0) {
                    searchResults.innerHTML = clients.clientsList.map((client, index) => {
                        // Identificar si es Active o Inactive
                        const isActive = client.clientName.includes("(Active)");
                        const statusClass = isActive ? "client-status-active" : "client-status-inactive";

                        // Crear el nombre limpio sin estado
                        const cleanClientName = client.clientName.replace(/ - \((Active|Inactive)\)$/, '').trim();

                        // Generar el HTML del resultado con el estado estilizado
                        return `
                    <div class="client-result" data-client-id="${client.clientId}" data-client-name="${client.clientName}" data-index="${index}">
                        ${cleanClientName} <span class="${statusClass}">${isActive ? "(Active)" : "(Inactive)"}</span>
                    </div>
                `;
                    }).join('');
                    searchResults.style.display = 'block';
                    activeIndex = -1;
                    positionResults(searchInput, searchResults);
                } else {
                    searchResults.innerHTML = `<div class="client-no-results">No results found.</div>`;
                    searchResults.style.display = 'block';
                    activeIndex = -1;
                    positionResults(searchInput, searchResults);
                }
            } catch (error) {
                console.error('Error fetching clients:', error);
                searchResults.innerHTML = `<div class="client-no-results">Error loading clients.</div>`;
                searchResults.style.display = 'block';
                activeIndex = -1;
                positionResults(searchInput, searchResults);
            }
        });


        // Handle keyboard navigation
        searchInput.addEventListener('keydown', (event) => {
            const results = searchResults.querySelectorAll('.client-result');

            if (event.key === 'ArrowDown') {
                activeIndex = (activeIndex + 1) % results.length;
                updateActiveResult(results, activeIndex);
            } else if (event.key === 'ArrowUp') {
                activeIndex = (activeIndex - 1 + results.length) % results.length;
                updateActiveResult(results, activeIndex);
            } else if (event.key === 'Enter') {
                event.preventDefault();
                if (activeIndex >= 0) {
                    selectClient(results[activeIndex], selectedInput, clientIdInput, searchResults, searchInput);
                }
            } else if (event.key === 'Escape') {
                hideResults(searchResults, true, searchInput);
            }
        });

        // Handle mouse click on results
        searchResults.addEventListener('click', (event) => {
            if (event.target.classList.contains('client-result')) {
                selectClient(event.target, selectedInput, clientIdInput, searchResults, searchInput);
            }
        });

        // Close the results list if clicked outside the container
        document.addEventListener('click', (event) => {
            if (!event.target.closest('[data-client-search]')) {
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

// Handle client selection
function selectClient(element, selectedInput, clientIdInput, searchResults, searchInput) {
    // Obtener el nombre completo con estado
    const fullClientName = element.getAttribute('data-client-name');
    const clientId = element.getAttribute('data-client-id');

    // Remover el texto "- (Active)" o "- (Inactive)"
    const cleanClientName = fullClientName.replace(/ - \((Active|Inactive)\)$/, '').trim();

    // Asignar el nombre limpio y el ID al input oculto
    selectedInput.value = cleanClientName;
    clientIdInput.value = clientId;
    selectedInput.style.display = 'block';

    hideResults(searchResults, true, searchInput);
}


// Hide results and optionally clear input
function hideResults(searchResults, clearInput = false, searchInput = null) {
    searchResults.style.display = 'none';
    searchResults.innerHTML = ''; // Clear results
    if (clearInput && searchInput) {
        searchInput.value = '';
    }
}

// Position the results container correctly
function positionResults(searchInput, searchResults) {
    const inputRect = searchInput.getBoundingClientRect();
    const containerRect = searchInput.closest('.client-search-container').getBoundingClientRect();

    searchResults.style.top = `${inputRect.height + 5}px`;
    searchResults.style.left = `0px`;
    searchResults.style.width = `${containerRect.width}px`;
}
