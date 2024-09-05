$(document).ready(function () {
    $('.btn-salvar').click(function () {
        var form = $('#salvarLicenca');
        var id = $('#ContratoId').val();
        $("#idDoContrato").val(id);

        if (form[0].checkValidity() === false) {
            form.addClass('was-validated');
        } else {         
            $.ajax({
                url: '/Licenca/GravarLicenca',
                type: 'POST',
                data: form.serialize(),
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Sucesso!',
                            text: response.message,
                            confirmButtonText: 'OK'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $('#cadastroDeLicenca').modal('hide');
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