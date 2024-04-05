async function fillProjectsDropdown() {
    displaySpinner();
    try {
        const projects = await getProjectsWhereConsultantAssigned();
        const selectElement = document.querySelector('#projectsSelect');
        const fragment = document.createDocumentFragment();
        console.log("Projects: " + projects);
        projects.projects.forEach(project => {
            const option = document.createElement('option');
            option.value = project.projectId;
            option.textContent = project.name;
            fragment.appendChild(option);
        });
        selectElement.innerHTML = '';
        selectElement.appendChild(fragment);
        hideSpinner();
    } catch (error) {
        console.error("Error filling the projects dropdown:", error.message);
        displayToasterError("Failed to load projects");
        hideSpinner();
    }
}

document.addEventListener('DOMContentLoaded', function () {
    fillProjectsDropdown();
});

