var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable(
        {
        ajax: { url:'/admin/company/getall'},
        columns: [
            { data: 'name', width: '20%' },
            { data: 'address', width: '20%' },
            { data: 'city', width: '15%' },
            { data: 'state', width: '15%' },
            { data: 'postalCode', width: '15%' },
            { data: 'phoneNumber', width: '15%'},
            {
                data: 'companyId',
                render: function (data) {
                    return `
                    <div class="text-center">
                        <a href="/Admin/Company/Edit?id=${data}" class="btn btn-primary text-white" style="cursor:pointer; width:100px;">
                            <i class="far fa-edit"></i> Edit
                        </a>
                        &nbsp;
                        <a onclick=Delete("/Admin/Company/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                            <i class="far fa-trash-alt"></i> Delete
                        </a>
                    </div>
                    `;
                }, width: '15%'
            }
        ]
    });

}

function Delete(url) {
    swal({
        title: "Are you sure you want to delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

