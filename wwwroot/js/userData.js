var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable(
        {
        ajax: { url:'/admin/user/getall'},
        columns: [
            { data: 'name', width: '20%' },
            { data: 'email', width: '20%' },
            { data: 'phoneNumber', width: '15%' },
            { data: 'company.name', width: '15%' },
            { data: 'role', width: '15%' },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd' },
                render: function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if(lockout > today){
                        //user is currently locked
                        return `
                        <div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                <i class="fas fa-lock-open"></i> Unlock
                            </a>
                            <a  href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-secondary text-white" style="cursor:pointer; width:150px;">
                                <i class="fas fa-lock-open"></i> Permission
                            </a>
                        </div>
                        `;
                    }
                    else{
                        return `
                        <div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                <i class="fas fa-lock"></i> Lock
                            </a>
                            <a  href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-secondary text-white" style="cursor:pointer; width:150px;">
                                <i class="fas fa-lock-open"></i> Permission
                            </a>
                        </div>
                        `;
                    }
                }, width: '15%'
            }
        ]
    });

}

function LockUnlock(id) {
    swal({
        title: "Are you sure you want to execute this action?",
        text: "You will be able to revert this action!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "POST",
                url: '/admin/user/lockunlock',
                data: JSON.stringify(id),
                contentType: 'application/json',
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

