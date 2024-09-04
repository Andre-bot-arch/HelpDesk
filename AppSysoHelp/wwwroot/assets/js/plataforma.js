$(document).ready(function () {
    $('#btnSalvar').click(function () {
        var form = $('#formCadastrarPlataforma');

        if (form[0].checkValidity() === false) {
            form.addClass('was-validated');
        } else {
            var formData = form.serialize();

            $.ajax({
                url: '/Plataforma/Create',
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
                                $('#modalCadastrarPlataforma').modal('hide');
                                form[0].reset();
                                form.removeClass('was-validated');
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
});