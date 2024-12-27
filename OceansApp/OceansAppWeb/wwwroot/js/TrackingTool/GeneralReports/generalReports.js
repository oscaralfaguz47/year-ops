const filterSection = getElementById('filters-section');
const filterSpinner = getElementById('filters-spinner');
const resultsSection = getElementById('results-section');
const noResultsMessage = getElementById('no-results-message');
let resultsToPrint = [];

const startDateFilterInput = getElementById('start-date');
const endDateFilterInput = getElementById('end-date');
const movementTypeFilterInput = getElementById('movement-type');

const errorMessageContainer = document.createElement('div'); 

getElementById('validations-container').appendChild(errorMessageContainer);

// Function to validate date format (YYYY-MM-DD)
function isValidDate(dateString) {
    const datePattern = /^\d{4}-\d{2}-\d{2}$/; // Regex for YYYY-MM-DD
    if (!datePattern.test(dateString)) return false;

    const date = new Date(dateString);
    return date instanceof Date && !isNaN(date);
}

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
    resultsSection.style.display = 'block';
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
    errorMessageContainer.innerHTML = ""; // Clear previous error messages

    // Validate dates
    const startDateValueR = startDateFilterInput.value;
    const endDateValueR = endDateFilterInput.value;

    // Validate date format
    if (!isValidDate(startDateValueR) || !isValidDate(endDateValueR)) {
        errorMessageContainer.innerHTML = "Please enter valid dates in both Start Date and End Date (DD-MM-YYYY).";
        return;
    }

    const startDate = new Date(startDateValueR);
    const endDate = new Date(endDateValueR);

    // Validate Start Date is not greater than End Date
    if (startDate > endDate) {
        errorMessageContainer.innerHTML = "Start Date cannot be greater than End Date.";
        return;
    }

    noResultsMessage.style.display = 'none';
    resultsSection.innerHTML = '<div style="display:flex;justify-content:center;"><div class="filter-spinner"></div><div>';
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
        `startDate=${startDateValueR}&endDate=${endDateValueR}` +
        `&movementType=${movementTypeValue}` +
        `${filters.clients.length ? `&${buildQueryParam("clients", filters.clients)}` : ""}` +
        `${filters.projects.length ? `&${buildQueryParam("projects", filters.projects)}` : ""}` +
        `${filters.consultants.length ? `&${buildQueryParam("consultants", filters.consultants)}` : ""}`;
    const results = await getGeneralReport(url);
    resultsToPrint = results.movementsList;

    if (resultsToPrint.length === 0) {
        noResultsMessage.innerHTML = `<label style="color:#dc3545">No results found.</label>`;
        resultsSection.innerHTML = '';
        resultsSection.appendChild(noResultsMessage);
        noResultsMessage.style.display = 'block';
    } else {
        renderResults(resultsToPrint);
    }
    
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
        return data;
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error(error);
    }
}

function renderResults(data) {
    const exportPdfBtnContainer = document.createElement('div');
    exportPdfBtnContainer.className = 'export-btn-container';
    const exportPdfBtn = document.createElement('button');
    exportPdfBtn.textContent = 'Export to PDF';
    exportPdfBtn.addEventListener('click', () => {
        generatePDF(data);
    });
 
    const resultsSection = document.getElementById('results-section');
    resultsSection.innerHTML = '';

    exportPdfBtnContainer.appendChild(exportPdfBtn);
    resultsSection.appendChild(exportPdfBtnContainer);

    // Group data by ClientName
    const groupedData = data.reduce((acc, item) => {
        if (!acc[item.clientName]) {
            acc[item.clientName] = {};
        }
        if (!acc[item.clientName][item.consultantName]) {
            acc[item.clientName][item.consultantName] = [];
        }
        acc[item.clientName][item.consultantName].push(item);
        return acc;
    }, {});

    for (const clientName in groupedData) {
        // Create title for ClientName
        const clientTitle = document.createElement('h2');
        clientTitle.textContent = clientName;
        clientTitle.style.fontWeight = 'bold';
        resultsSection.appendChild(clientTitle);

        for (const consultantName in groupedData[clientName]) {
            // Create subtitle for ConsultantName
            const consultantSubtitle = document.createElement('h4');
            consultantSubtitle.textContent = consultantName;
            resultsSection.appendChild(consultantSubtitle);

            // Create a container for the table
            const tableContainer = document.createElement('div');
            tableContainer.className = 'global-table-container';

            // Create the table
            const table = document.createElement('table');
            table.style.width = '100%';
            table.style.borderCollapse = 'collapse';

            // Create table headers
            const thead = document.createElement('thead');
            thead.innerHTML = `
                <tr style="background-color: #ecf0f1; text-align: left;">
                    <th style="padding: 8px; border: 1px solid #ddd;">Action Date</th>
                    <th style="padding: 8px; border: 1px solid #ddd;">Project Name</th>
                    <th style="padding: 8px; border: 1px solid #ddd;">Quantity</th>
                    <th style="padding: 8px; border: 1px solid #ddd;">Movement Type</th>
                    <th style="padding: 8px; border: 1px solid #ddd;">Notes</th>
                </tr>
            `;
            table.appendChild(thead);

            const tbody = document.createElement('tbody');
            const records = groupedData[clientName][consultantName];

            // Calculate the sum of Quantity by MovementType
            const quantitySums = {};
            records.forEach((record) => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td style="padding: 8px; border: 1px solid #ddd;">${new Date(record.actionDate).toLocaleDateString()}</td>
                    <td style="padding: 8px; border: 1px solid #ddd;">${record.projectName}</td>
                    <td style="padding: 8px; border: 1px solid #ddd; text-align: center;">${record.quantity}</td>
                    <td style="padding: 8px; border: 1px solid #ddd;"><span style="color:${record.movementType.includes('Non-payable') ? '#dc3545' : ''}">${record.movementType.replace('(Non-payable)', '')}</span></td>
                    <td style="padding: 8px; border: 1px solid #ddd;">${record.notes || ''}</td>
                `;
                tbody.appendChild(row);

                // Summation by MovementType
                if (!quantitySums[record.movementType]) {
                    quantitySums[record.movementType] = 0;
                }
                quantitySums[record.movementType] += record.quantity;
            });

            table.appendChild(tbody);

            // Append the table to the global container
            tableContainer.appendChild(table);

            // Add the container to the results section
            resultsSection.appendChild(tableContainer);

            // Create the summary section
            const summary = document.createElement('div');
            summary.className = 'summary-section';
            summary.innerHTML = '<label>Summary:</label>';

            for (const movementType in quantitySums) {
                summary.innerHTML += `
                    <li>
                        <span class="summary-sub">${movementType.replace('(Non-payable)', '') }:</span> <span class="summary-val">${quantitySums[movementType]}</span>
                    </li>
                `;
            }
            resultsSection.appendChild(summary);
        }
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

function generatePDF(data) {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();
    let yPosition = 20;

    const logoPath = "/img/logo-color.png";

    const startDateFilterInput = document.getElementById("start-date");
    const endDateFilterInput = document.getElementById("end-date");
    const startDateValue = startDateFilterInput?.value || "N/A";
    const endDateValue = endDateFilterInput?.value || "N/A";

    const startDate = formatDate(startDateValue);
    const endDate = formatDate(endDateValue);

    function formatDate(date) {
        const [year, month, day] = date.split("-");
        return `${month}/${day}/${year}`;
    }

    // Add logo to header
    const img = new Image();
    img.src = logoPath;
    img.onload = () => {
        pdf.addImage(img, "PNG", 10, 10, 40, 20); // Logo size (40x20)
        yPosition = 35; // Position after the logo

        // Título principal
        pdf.setFont("helvetica", "bold");
        pdf.setFontSize(16);
        pdf.text("HOURS REPORT", 105, yPosition, { align: "center" });
        yPosition += 6; 

        pdf.setFont("helvetica", "normal");
        pdf.setFontSize(10);
        pdf.text(`From: ${startDate} to: ${endDate}`, 105, yPosition, { align: "center" });
        yPosition += 10;

        generateContent(pdf, data, yPosition);

        pdf.save(`Hours_Report_From_${startDate}_To_${endDate}.pdf`);
    };
}

function generateContent(pdf, data, yPosition) {
    // Column widths
    const columnWidths = {
        actionDate: 25,
        projectName: 50,
        quantity: 20,
        movementType: 40,
        notes: 65, // Narrower Notes column
    };

    // Group data by ClientName and ConsultantName
    const groupedData = data.reduce((acc, item) => {
        if (!acc[item.clientName]) {
            acc[item.clientName] = {};
        }
        if (!acc[item.clientName][item.consultantName]) {
            acc[item.clientName][item.consultantName] = [];
        }
        acc[item.clientName][item.consultantName].push(item);
        return acc;
    }, {});

    // Generate the PDF content
    for (const clientName in groupedData) {
        if (yPosition > 270) {
            pdf.addPage();
            yPosition = 20;
        }

        // Client name title
        pdf.setFontSize(14); // Slightly smaller font
        pdf.setFont("helvetica", "bold");
        pdf.setTextColor("#34495e"); // Client Name color
        pdf.text(clientName, 105, yPosition, { align: "center" });
        yPosition += 8;

        for (const consultantName in groupedData[clientName]) {
            if (yPosition > 270) {
                pdf.addPage();
                yPosition = 20;
            }

            // Consultant name
            pdf.setFontSize(12);
            pdf.setFont("helvetica", "bold"); // Bold Consultant Name
            pdf.setTextColor("#058993"); // Consultant Name color
            pdf.text(consultantName, 10, yPosition);
            yPosition += 8;

            // Table headers
            pdf.setFontSize(10); // Smaller headers
            pdf.setFont("helvetica", "bold");
            pdf.setTextColor(0); // Default black for table headers
            pdf.text("Action Date", 10, yPosition);
            pdf.text("Project Name", 10 + columnWidths.actionDate, yPosition);
            pdf.text("Quantity", 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity / 2, yPosition, { align: "center" });
            pdf.text("Movement Type", 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity, yPosition);
            pdf.text("Notes", 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity + columnWidths.movementType, yPosition);
            yPosition += 6;

            pdf.setDrawColor(200);
            pdf.line(10, yPosition, 200, yPosition); // Line under the headers
            yPosition += 4;

            // Records
            const records = groupedData[clientName][consultantName];
            const quantitySums = {};

            pdf.setFont("helvetica", "normal");
            pdf.setFontSize(9); // Smaller font for records
            pdf.setTextColor(0); // Default black for records

            records.forEach((record) => {
                const actionDate = new Date(record.actionDate).toLocaleDateString();
                const projectName = pdf.splitTextToSize(record.projectName, columnWidths.projectName);
                const quantity = record.quantity.toString();
                const movementType = pdf.splitTextToSize(record.movementType.replace("(Non-payable)", "").trim(), columnWidths.movementType);
                const notes = pdf.splitTextToSize(record.notes || "", columnWidths.notes); // Blank if no Notes

                // Calculate the row height based on the longest column content
                const rowHeight = Math.max(projectName.length, movementType.length, notes.length) * 5; // 5px per line

                // Render each column in its respective position
                pdf.text(actionDate, 10, yPosition);
                pdf.text(projectName, 10 + columnWidths.actionDate, yPosition);
                pdf.text(quantity, 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity / 2, yPosition, { align: "center" });

                // Conditional color for Movement Type
                if (record.movementType.includes("Time Off")) {
                    pdf.setTextColor("#dc3545"); // Red color for "Time Off"
                } else {
                    pdf.setTextColor(0); // Default black
                }
                movementType.forEach((line, index) => {
                    pdf.text(line, 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity, yPosition + index * 5);
                });

                // Notes
                pdf.setTextColor(0); // Reset text color
                notes.forEach((line, index) => {
                    pdf.text(line, 10 + columnWidths.actionDate + columnWidths.projectName + columnWidths.quantity + columnWidths.movementType, yPosition + index * 5);
                });

                // Add a thin gray line below each record
                yPosition += rowHeight;
                pdf.setDrawColor(220); // Light gray
                pdf.line(10, yPosition, 200, yPosition); // Line below record
                yPosition += 4;

                // Update MovementType totals
                if (!quantitySums[movementType[0]]) {
                    quantitySums[movementType[0]] = 0;
                }
                quantitySums[movementType[0]] += record.quantity;

                if (yPosition > 270) {
                    pdf.addPage();
                    yPosition = 20;
                }
            });

            // Summary Section
            pdf.setFont("helvetica", "bold");
            pdf.setTextColor("#01c0b7"); // Summary title color
            pdf.setFontSize(12);
            pdf.text("Total Summary:", 10, yPosition);
            yPosition += 6;

            pdf.setFont("helvetica", "bold");
            pdf.setFontSize(10);
            pdf.setTextColor(0); // Default black for summary content
            for (const movementType in quantitySums) {
                const cleanedType = movementType.replace("(Non-payable)", "").trim();
                pdf.text(`- ${cleanedType}: ${quantitySums[movementType]}`, 15, yPosition);
                yPosition += 5;
                if (yPosition > 270) {
                    pdf.addPage();
                    yPosition = 20;
                }
            }

            // Add a thin cyan line after Summary
            yPosition += 4;
            pdf.setDrawColor("#01c0b7");
            pdf.line(10, yPosition, 200, yPosition); // Cyan line only for Summary
            yPosition += 8;
        }
    }

    // Footer of the document
    pdf.setFontSize(9); // Smaller footer font
    pdf.setFont("helvetica", "italic");
    pdf.text("Document generated by Oceans from Ripple By Oceans", 105, 290, { align: "center" });
}














