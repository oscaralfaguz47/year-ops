var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblUsers').DataTable({
        "ajax": {
            "url": "/AdminCenter/ApplicationUser/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "lastName", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "ocupation", "width": "15%" },
            {
                "data": "id",
                "render": function (data, type, row, meta) {
                    var text = "Activar";
                    var twoFactor = "hide"
                    var actionIcon = "bi-check-circle-fill";
                    if (row['isActive']) {
                        text = "Desactivar";
                        actionIcon = "bi-x-square";
                    }
                    if (row['twoFactorEnabled']) {
                        twoFactor = "show";
                    }
                    return `
                        <div class="w-75 btn-group" role="group">
                         <a href="/AdminCenter/ApplicationUser/Edit?userId=${data}"
                         class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Editar</a>
                         <a onClick=ActivateDeactivate('/AdminCenter/ApplicationUser/ActivateDeactivate?userId=${data}')
                         class="btn btn-danger mx-2"> <i class="bi ${actionIcon}"></i>${text}</a>
                        <a onClick=RemoveAuthenticator('/AdminCenter/ApplicationUser/RemoveAuthenticator?userId=${data}')
                         class="btn btn-danger mx-2 ${twoFactor}"> <i class="bi bi-unlock"></i>Reiniciar Two Factor</a>
					    </div>
                        `
                },
                "width": "20%"
            }
        ]
    });
}

function ActivateDeactivate(url) {
    Swal.fire({
        title: '¿Estas seguro de continuar?',
        text: "",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, continuar!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

function RemoveAuthenticator(url) {
    Swal.fire({
        title: '¿Estas seguro de continuar?',
        text: "",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, continuar!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}