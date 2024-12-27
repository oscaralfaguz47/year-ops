const filterSection = getElementById('filters-section');
const filterSpinner = getElementById('filters-spinner');
const resultsSection = getElementById('results-section');
const noResultsMessage = getElementById('no-results-message');

const startDateFilterInput = getElementById('start-date');
const endDateFilterInput = getElementById('end-date');
const movementTypeFilterInput = getElementById('movement-type');
document.addEventListener('DOMContentLoaded', async function () {
    setTimesheetItemActive();
    var filterOptions = await getFilterOptions();

    const projects = filterOptions.projects;
    const clients = filterOptions.clients;
    const consultants = filterOptions.consultants;

    const populateFilters = (data, containerId) => {
        const container = document.getElementById(containerId);
        container.innerHTML = data
            .map(item => {
                let formattedText = item.text.replace(
                    /\((Active|Inactive)\)/g,
                    (match, status) => {
                        const color = status === "Active" ? "var(--clr-blueLight)" : "red";
                        return `<span style="color: ${color};">${match}</span>`;
                    }
                );

                return `<label><input type="checkbox" value="${item.value}" onchange="updateSelectedCount('${containerId}')"> ${formattedText}</label>`;
            })
            .join("") + '<p class="no-results" style="display:none;">No results found</p>';
    };


    const setupSearchFilter = (searchInputId, containerId) => {
        const searchInput = document.getElementById(searchInputId);
        const container = document.getElementById(containerId);

        searchInput.addEventListener("input", () => {
            const filter = searchInput.value.toLowerCase();
            let visible = 0;

            Array.from(container.children).forEach(label => {
                if (label.tagName === "LABEL") {
                    const text = label.textContent.toLowerCase();
                    const isVisible = text.includes(filter);
                    label.style.display = isVisible ? "" : "none";
                    if (isVisible) visible++;
                }
            });

            const noResults = container.querySelector(".no-results");
            noResults.style.display = visible === 0 ? "block" : "none";
        });
    };

    populateFilters(projects, "project-filters");
    populateFilters(clients, "client-filters");
    populateFilters(consultants, "consultant-filters");

    const today = new Date();
    const startDate = new Date(today.getFullYear(), today.getMonth(), 1);
    const endDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    document.getElementById("start-date").value = startDate.toISOString().split("T")[0];
    document.getElementById("end-date").value = endDate.toISOString().split("T")[0];

    updateSelectedCount("project-filters");
    updateSelectedCount("client-filters");
    updateSelectedCount("consultant-filters");

    setupSearchFilter("search-projects", "project-filters");
    setupSearchFilter("search-clients", "client-filters");
    setupSearchFilter("search-consultants", "consultant-filters");

    filterSpinner.style.display = 'none';
    filterSection.style.display = 'block';
});


const updateSelectedCount = (containerId) => {
    const container = document.getElementById(containerId);
    const selectedCountElement = document.getElementById(`${containerId.replace('-filters', '')}-selected-count`); // Ajustamos el ID

    if (!selectedCountElement) {
        console.error(`Element with ID ${containerId.replace('-filters', '')}-selected-count not found.`);
        return;
    }

    const selectedOptions = Array.from(container.querySelectorAll("input[type='checkbox']:checked")).length;

    selectedCountElement.textContent = selectedOptions > 0
        ? `(${selectedOptions}) Selected`
        : "No selected options";
};



document.getElementById("apply-filters").addEventListener("click", async () => {
    const getCheckedValues = (containerId) =>
        Array.from(document.querySelectorAll(`#${containerId} input[type='checkbox']`))
            .filter(checkbox => checkbox.checked)
            .map(checkbox => parseInt(checkbox.value));

    const buildQueryParam = (name, values) => {
        if (!values || values.length === 0) return "";
        return values.map(value => `${name}=${value}`).join("&");
    };

    const filters = {
        projects: getCheckedValues("project-filters"),
        clients: getCheckedValues("client-filters"),
        consultants: getCheckedValues("consultant-filters"),
        startDate: startDateFilterInput.value,
        endDate: endDateFilterInput.value,
        movementType: movementTypeFilterInput.value
    };
    const startDateValue = encodeURIComponent(filters.startDate);
    const endDateValue = encodeURIComponent(filters.endDate);
    const movementTypeValue = encodeURIComponent(filters.movementType);

    const url = `/TrackingTool/GeneralReports/GetGeneralReport?` +
        `startDate=${startDateValue}&endDate=${endDateValue}` +
        `&movementType=${movementTypeValue}` +
        `${filters.clients.length ? `&${buildQueryParam("clients", filters.clients)}` : ""}` +
        `${filters.projects.length ? `&${buildQueryParam("projects", filters.projects)}` : ""}` +
        `${filters.consultants.length ? `&${buildQueryParam("consultants", filters.consultants)}` : ""}`;
    await getGeneralReport(url);
    
});

async function getGeneralReport(url) {

    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            console.log(errorData);
            if (errorData.messageType === "Validation Error") {
                displayToasterWarningArray(errorData.errors);
                throw new Error('Validation errors!');
            } else {
                displayToasterError(errorData.error);
                hideModal(modalId);
                throw new Error('The request to the server failed!. More details: ' + errorData.detail);
            }
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }

        const data = await response.json();
        console.log(data);
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error(error);
    } finally {
        //displayElement(loadingBoxIntern, 'none');
    }
}

async function getFilterOptions() {
    var url = "/TrackingTool/GeneralReports/GetOptionsForFilters";
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}