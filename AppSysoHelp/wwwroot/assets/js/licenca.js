$(document).ready(function () {

    $(".btn-editar_licenca").click(function () {
        var data = $(this).data("ativacao");
        var partes = data.split('/');
        var dia = partes[0];
        var mes = partes[1];
        var ano = partes[2];

        var anoMes = ano + '-' + mes;

        $("#LicencaId").val($(this).data("id"));
        $("#_Descricao").val($(this).data("desc"));
        $("#Urlacesso").val($(this).data("url"));
        $("#DataAtivacao").val(anoMes);
        $("#Modelo").val($(this).data("modelo"));
        $("#NumeroSerie").val($(this).data("mac"));
        $('#cadastroDeLicenca').modal('show');
    });

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