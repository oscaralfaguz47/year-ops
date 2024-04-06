async function fillProjectsDropdown() {
    const dropdownList = document.querySelector('.dropdown-list');
    dropdownList.innerHTML = '<li class="spinner-cont"><div class="spinner"></div></li>';

    try {
        const response = await getProjectsWhereConsultantAssigned();
        const projects = response.projects;

        dropdownList.innerHTML = '';
        projects.forEach(project => {
            const listItem = document.createElement('li');
            listItem.innerHTML = `<div class="circle circle-li">${project.name.charAt(0)}</div>${project.name}`;
            listItem.dataset.value = project.projectId;
            listItem.addEventListener('click', function () {
                document.querySelector('.dropdown-selected').innerHTML = `<div class="circle">${project.name.charAt(0)}</div>`;
                document.getElementById('project-name').innerHTML = `${project.name}`;
                document.querySelector('.dropdown-list').style.display = 'none';
                selectProject(project.projectId);
            });
            dropdownList.appendChild(listItem);
        });
    } catch (error) {
        console.error("Error filling the projects dropdown:", error.message);
        dropdownList.innerHTML = '<li>Error loading options</li>';
    }
}
const header = document.getElementById('header');
const loadingBox = document.getElementById('loading-box');
const errorMessageBox = document.getElementById('error-Message-box');
const contentBox = document.getElementById('content-box');
const noProjectsBox = document.getElementById('no-projects-box');

async function getProjectInfo() {
    loadingBox.style.display = 'flex';
    errorMessageBox.style.display = 'none';
    contentBox.style.display = 'none';
    try {
        const response = await getSelectedProjectInfo();
        const projectInfo = response.projectInfoData;
        if (projectInfo !== null) {
            document.querySelector('.dropdown-selected').innerHTML = `<div class="circle">${projectInfo.projectName.charAt(0)}</div>`;
            document.getElementById('project-name').innerHTML = `${projectInfo.projectName}`;
            document.getElementById('content-footer').innerHTML = `<span>Questions on reporting? Contact the Success Manager, 
        <strong>${projectInfo.sucessManagerName}</strong> at <a href="mailto:${projectInfo.successManagerEmail}">
        ${projectInfo.successManagerEmail}</a> or via Slack.</span>`;
            header.style.display = 'flex';
            loadingBox.style.display = 'none';
            contentBox.style.display = 'block';
        } else {
            noProjectsBox.style.display = 'block';
            loadingBox.style.display = 'none';
        }
    } catch (error) {
        console.error("Error filling the projects dropdown:", error.message);
        loadingBox.style.display = 'none';
        errorMessageBox.style.display = 'flex';
    }
}
document.addEventListener('DOMContentLoaded', function () {
    let dataLoaded = false;
    const dropdownHeader = document.querySelector('.dropdown-header');
    const dropdownList = document.querySelector('.dropdown-list');

    dropdownHeader.addEventListener('click', function () {
        if (!dataLoaded) {
            fillProjectsDropdown();
            dataLoaded = true;
        }
        dropdownList.style.display = dropdownList.style.display === 'block' ? 'none' : 'block';
    });

    document.addEventListener('click', function (event) {
        if (!dropdownHeader.contains(event.target)) {
            dropdownList.style.display = 'none';
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === "Escape") {
            dropdownList.style.display = 'none';
        }
    });
    getProjectInfo();
});

async function selectProject(projectId) {
    loadingBox.style.display = 'flex';
    contentBox.style.display = 'none';
    var token = $('[name="__RequestVerificationToken"]').val();
    var formData = new FormData();
    formData.append('projectId', projectId);
    fetch("/AccountManagement/ProjectsConsultantsAssigned/SelectConsultantProject"
        , {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                header.style.display = 'flex';
                loadingBox.style.display = 'none';
            } else {
                displayToasterError(data.error);
                console.error('There has been a problem with the fetch operation:', data.detail);
                loadingBox.style.display = 'none';
            }
            getProjectInfo();
        });
}