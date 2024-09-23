$(document).ready(function () {
    $(".btn-detalhar-licenca").click(function () {
        var id = $(this).data("id");
        $.ajax({
            url: '/Contrato/Dispositivos',
            type: 'POST',
            data: { id: id },
            beforeSend: function () {
                Swal.fire({
                    title: 'Aguarde...',
                    html: 'Carregando os dados do dispositivo...',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
            },
            success: function (response) {
                Swal.close();
                $("#modalContentLicenca").html(response);
            },
            error: function () {
                Swal.close();               
                Swal.fire({
                    icon: 'error',
                    title: 'Erro!',
                    text: 'Ocorreu um erro inesperado.',
                    confirmButtonText: 'OK'
                });
            }
        });


    });

    $(".btn-exluir-dispostivo").click(function () {
        var id = $(this).data("id");
        var valor = "#_linha" + id;
        var linha = $(valor).val();     

        Swal.fire({
            icon: 'error',
            title: 'Aten&ccedil;&atilde;o!!',
            text: 'Excluir Dispositivo?',
            confirmButtonText: 'SIM'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Licenca/ExcluirDispoditivo',
                    type: 'POST',
                    data: { id: id},
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Sucesso!',
                                text: response.message,
                                confirmButtonText: 'OK',
                                timer: 2000,  
                                timerProgressBar: true, 
                                willClose: () => { 
                                    $(valor).remove();  // Remove a linha da tabela
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

    $(".btn-inativar-licenca").click(function () {
        var id = $(this).data("id");
        var situacao = $(this).data("ativo")
      
        Swal.fire({
            icon: 'error',
            title: 'Aten&ccedil;&atilde;o!!',
            text: '´Mudar Estatus?',
            confirmButtonText: 'SIM'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Licenca/UpdateEstatusLicenca',
                    type: 'POST',
                    data: { id: id, ativo: situacao },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Sucesso!',
                                text: response.message,
                                confirmButtonText: 'OK'
                            }).then((result) => {
                                if (result.isConfirmed) { 
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