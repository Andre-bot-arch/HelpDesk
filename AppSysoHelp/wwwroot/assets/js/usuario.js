$(document).ready(function () {
    $('#btnSalvar').click(function () {
        var form = $('#formCadastrarUsuario')[0];

        if (form.checkValidity() === false) {
            $('#formCadastrarUsuario').addClass('was-validated');
        } else {
            var formData = $('#formCadastrarUsuario').serialize(); 

            $.ajax({
                url: '/Usuario/Create',
                type: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Sucesso!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $('#modalCadastrarUsuario').modal('hide');                               
                                location.reload(); 
                            }
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Erro!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro!',
                        text: 'Ocorreu um erro inesperado.',
                        confirmButtonText: 'OK'
                    });
                }
            });
        }
    });

    $(".carregarModal").click(function () {        

        $("#NomeCompleto").val($(this).data("nome"));
        $("#CargoResponsabilidade").val($(this).data("perfil"));
        $("#TelefoneContato").val($(this).data("fone"));
        $("#EmailContato").val($(this).data("email"));
        $("#Cpf").val($(this).data("cpf"));
        $("#PkId").val($(this).data("id"));

        $("#modalCadastrarUsuario").modal("show");
    });
   
});